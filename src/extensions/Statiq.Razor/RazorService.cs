﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Razor
{
    /// <summary>
    /// Razor compiler should be shared so that pages are only compiled once.
    /// </summary>
    internal class RazorService
    {
        private const string CacheFileName = "razorcache.json";

        private readonly ConcurrentCache<CompilationParameters, RazorCompiler> _compilers
            = new ConcurrentCache<CompilationParameters, RazorCompiler>();

        private Dictionary<AssemblyCacheKey, string> _cachedAssemblies = new Dictionary<AssemblyCacheKey, string>();

        private bool _firstExecution = true;

        public async Task RenderAsync(RenderRequest request)
        {
            int namespacesCacheCode = await request.Context.Namespaces.GetCacheCodeAsync();
            await _compilers.AddOrUpdate(
                CompilationParameters.Get(namespacesCacheCode, request.BaseType, request.Model is IDocument),
                parameters =>
                {
                    RazorCompiler compiler = new RazorCompiler(parameters, request.Context);
                    compiler.EnsurePhases(parameters, request.Context.Namespaces.ToArray());
                    return compiler;
                },
                (parameters, compiler) =>
                {
                    compiler.EnsurePhases(parameters, request.Context.Namespaces.ToArray());
                    return compiler;
                })
                .RenderPageAsync(request);
        }

        public async Task BeforeEngineExecutionAsync(BeforeEngineExecution args)
        {
            if (_firstExecution)
            {
                // See if we have an exiting assembly cache
                IFile cacheFile = args.Engine.FileSystem.GetCacheFile(CacheFileName);
                bool restoredCache = false;
                try
                {
                    if (cacheFile.Exists)
                    {
                        // See the SerializeJsonAsync below for the correct deserialization type
                        KeyValuePair<AssemblyCacheKey, string>[] cachedAssembliesArray =
                            await cacheFile.DeserializeJsonAsync<KeyValuePair<AssemblyCacheKey, string>[]>(null, args.Engine.CancellationToken);
                        _cachedAssemblies = cachedAssembliesArray.ToDictionary(x => x.Key, x => x.Value);
                        restoredCache = true;
                    }
                }
                catch (Exception ex)
                {
                    args.Engine.Logger.LogDebug($"Error while restoring Razor assembly cache file {cacheFile.Path.FullPath}: {ex.Message}");
                }

                // Populate the compiler caches
                if (restoredCache)
                {
                    int count = 0;
                    foreach (IGrouping<CompilationParameters, KeyValuePair<AssemblyCacheKey, string>> cacheGroup in _cachedAssemblies.GroupBy(x => x.Key.CompilationParameters))
                    {
                        RazorCompiler compiler = _compilers.GetOrAdd(cacheGroup.Key, parameters => new RazorCompiler(parameters, args.Engine.Services));
                        count += compiler.PopulateCache(cacheGroup.Select(x => new KeyValuePair<AssemblyCacheKey, string>(x.Key, args.Engine.FileSystem.GetCachePath(x.Value).FullPath)));
                    }
                    IExecutionContext.Current.LogInformation($"Restored Razor compilation cache from {cacheFile.Path.FullPath} with {count} assemblies");
                }
            }
            _firstExecution = false;
        }

        public async Task AfterEngineExecutionAsync(AfterEngineExecution args)
        {
            // Expire the internal Razor cache change tokens if this is a new execution
            // This needs to be done so that layouts/partials can be re-rendered if they've changed,
            // otherwise Razor will just use the previously cached version of them
            foreach (KeyValuePair<CompilationParameters, RazorCompiler> compilerItem in _compilers)
            {
                IReadOnlyDictionary<CompilerCacheKey, CompilationResult> compilerCache = compilerItem.Value.ResetCache();

                // Remove stale entries
                int removeCount = 0;
                foreach (AssemblyCacheKey removeAssemblyCacheKey in _cachedAssemblies.Keys.Where(x => x.CompilationParameters.Equals(compilerItem.Key)).ToArray())
                {
                    if (!compilerCache.ContainsKey(removeAssemblyCacheKey.CompilerCacheKey))
                    {
                        // The file will get deleted when we clean up the directory further down
                        _cachedAssemblies.Remove(removeAssemblyCacheKey);
                        removeCount++;
                    }
                }
                args.Engine.Logger.LogDebug($"Removed {removeCount} stale Razor assemblies from the cache");

                // Add new entries
                int addCount = 0;
                foreach (KeyValuePair<CompilerCacheKey, CompilationResult> compilerCacheItem in compilerCache)
                {
                    AssemblyCacheKey addAssemblyCacheKey = AssemblyCacheKey.Get(compilerItem.Key, compilerCacheItem.Key);
                    if (!_cachedAssemblies.ContainsKey(addAssemblyCacheKey) && compilerCacheItem.Value.AssemblyStream is object)
                    {
                        // Make a best effort to same the assemblies, but don't panic if they don't
                        try
                        {
                            string assemblyFileName = $"razor/{compilerCacheItem.Value.AssemblyName}.dll";
                            IFile assemblyFile = args.Engine.FileSystem.GetCacheFile(assemblyFileName);
                            compilerCacheItem.Value.AssemblyStream.Seek(0, SeekOrigin.Begin);
                            await assemblyFile.WriteFromAsync(compilerCacheItem.Value.AssemblyStream, cancellationToken: args.Engine.CancellationToken);

                            // Also save the PDB if we have one
                            if (compilerCacheItem.Value.PdbStream is object)
                            {
                                IFile pdbFile = args.Engine.FileSystem.GetCacheFile(Path.ChangeExtension(assemblyFileName, ".pdb"));
                                compilerCacheItem.Value.PdbStream.Seek(0, SeekOrigin.Begin);
                                await pdbFile.WriteFromAsync(compilerCacheItem.Value.PdbStream, cancellationToken: args.Engine.CancellationToken);
                            }

                            _cachedAssemblies.Add(addAssemblyCacheKey, assemblyFileName);
                            addCount++;
                        }
                        catch (Exception ex)
                        {
                            args.Engine.Logger.LogDebug($"Error while saving assembly or pdb file {compilerCacheItem.Value.AssemblyName} to cache: {ex.Message}");
                        }
                    }

                    // Whether we saved the assembly or not, go ahead and dispose the memory streams so they can be collected
                    compilerCacheItem.Value.DisposeMemoryStreams();
                }
                args.Engine.Logger.LogDebug($"Cached and saved {addCount} new Razor assemblies");

                // Clean up the cache folder
                IDirectory cacheDirectory = args.Engine.FileSystem.GetCacheDirectory("razor");
                if (cacheDirectory.Exists)
                {
                    int deleteCount = 0;
                    HashSet<string> cachedAssemblyFiles = new HashSet<string>(_cachedAssemblies.Values);
                    foreach (IFile assemblyFile in cacheDirectory.GetFiles())
                    {
                        if (!_cachedAssemblies.Values.Contains($"razor/{assemblyFile.Path.FileNameWithoutExtension}.dll"))
                        {
                            try
                            {
                                assemblyFile.Delete();
                                deleteCount++;
                            }
                            catch (Exception ex)
                            {
                                args.Engine.Logger.LogDebug($"Error while deleting stale cached assembly or pdb file {assemblyFile.Path.FullPath}: {ex.Message}");
                            }
                        }
                    }
                    args.Engine.Logger.LogDebug($"Deleted {deleteCount} stale cached Razor assembly files");
                }
            }

            // Write the updated cache file
            // System.Text.Json doesn't current support non-string dictionary keys, so write out as an array
            // See https://github.com/dotnet/runtime/issues/30524
            IFile cacheFile = args.Engine.FileSystem.GetCacheFile(CacheFileName);
            KeyValuePair<AssemblyCacheKey, string>[] cachedAssembliesArray = _cachedAssemblies.ToArray();
            await cacheFile.SerializeJsonAsync(cachedAssembliesArray, null, args.Engine.CancellationToken);
        }
    }
}