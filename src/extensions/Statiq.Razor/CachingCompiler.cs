using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ConcurrentCollections;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Razor
{
    internal abstract class CachingCompiler
    {
        private readonly ConcurrentCache<CompilerCacheKey, CompilationResult> _compilationCache
            = new ConcurrentCache<CompilerCacheKey, CompilationResult>();

        // Used to track compilation result requests on each execution so stale cache entries can be cleared
        private readonly ConcurrentHashSet<CompilerCacheKey> _requestedCompilationResults = new ConcurrentHashSet<CompilerCacheKey>();

        /// <summary>
        /// Populates the compiler cache with existing items.
        /// </summary>
        public int PopulateCache(IEnumerable<KeyValuePair<AssemblyCacheKey, string>> items)
        {
            int count = 0;
            foreach (KeyValuePair<AssemblyCacheKey, string> item in items)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFile(item.Value);
                    CompilationResult compilationResult = new CompilationResult(
                        Path.GetFileName(item.Value),
                        null,
                        null,
                        assembly,
                        StatiqViewCompiler.CompiledItemLoader.LoadItems(assembly).SingleOrDefault());
                    _compilationCache.TryAdd(item.Key.CompilerCacheKey, () => compilationResult);
                    count++;
                }
                catch (Exception ex)
                {
                    IExecutionContext.Current.LogDebug($"Could not load Razor assembly at {item.Value}: {ex.Message}");
                }
            }
            return count;
        }

        /// <summary>
        /// Resets the cache and expires change tokens (typically called after each execution).
        /// </summary>
        /// <returns>The current compiler cache after removing stale entries.</returns>
        public IReadOnlyDictionary<CompilerCacheKey, CompilationResult> ResetCache()
        {
            // Remove any compilations that weren't requested in the last run
            int removed = 0;
            foreach (CompilerCacheKey compilationCacheKey in _compilationCache.Keys.ToArray())
            {
                if (!_requestedCompilationResults.Contains(compilationCacheKey)
                    && _compilationCache.TryRemove(compilationCacheKey, out CompilationResult compilationResult))
                {
                    compilationResult.DisposeMemoryStreams(); // Just in case
                    removed++;
                }
            }
            _requestedCompilationResults.Clear();
            IExecutionContext.Current.LogDebug($"Removed {removed} stale Razor compilation results from the cache");
            return _compilationCache;
        }

        protected CompilationResult GetOrAddCachedCompilation(CompilerCacheKey cacheKey, Func<CompilerCacheKey, CompilationResult> valueFactory)
        {
            _requestedCompilationResults.Add(cacheKey);
            return _compilationCache.GetOrAdd(cacheKey, valueFactory);
        }
    }
}