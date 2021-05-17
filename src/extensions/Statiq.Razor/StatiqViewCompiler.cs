using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ConcurrentCollections;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Statiq.Common;

namespace Statiq.Razor
{
    // RuntimeViewCompiler is internal and the Razor team has made it clear they're not interested in maintaining a public API
    // So we've got to encapsulate it and get what we need via reflection
    // This is called for document Razor files by RazorCompiler which caches those itself, it's also called directly for layouts and partials, which are cached here
    internal class StatiqViewCompiler : CachingCompiler, IViewCompiler, IViewCompilerProvider
    {
        public static readonly RazorCompiledItemLoader CompiledItemLoader = new RazorCompiledItemLoader();

        private static readonly EmitOptions AssemblyEmitOptions = new EmitOptions(debugInformationFormat: DebugInformationFormat.PortablePdb);

        private static readonly MethodInfo CreateCompilationMethod;

        private static readonly MethodInfo GetNormalizedPathMethod;

        private static readonly MethodInfo CreateCompilationFailedExceptionMethod;

        static StatiqViewCompiler()
        {
            Type runtimeViewCompilerType = typeof(FileProviderRazorProjectItem).Assembly
                .GetType("Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation.RuntimeViewCompiler");
            CreateCompilationMethod = runtimeViewCompilerType.GetMethod(
                "CreateCompilation",
                BindingFlags.Instance | BindingFlags.NonPublic,
                Type.DefaultBinder,
                new Type[] { typeof(string), typeof(string) },
                null);
            GetNormalizedPathMethod = runtimeViewCompilerType.GetMethod(
                "GetNormalizedPath",
                BindingFlags.Instance | BindingFlags.NonPublic,
                Type.DefaultBinder,
                new Type[] { typeof(string) },
                null);

            Type compilationFailedExceptionFactory = typeof(FileProviderRazorProjectItem).Assembly
                .GetType("Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation.CompilationFailedExceptionFactory");
            CreateCompilationFailedExceptionMethod = compilationFailedExceptionFactory.GetMethod(
                "Create",
                new Type[] { typeof(RazorCodeDocument), typeof(IEnumerable<RazorDiagnostic>) });
        }

        public static Exception CreateCompilationFailedException(RazorCodeDocument codeDocument, IEnumerable<RazorDiagnostic> diagnostics) =>
            (Exception)CreateCompilationFailedExceptionMethod.Invoke(null, new object[] { codeDocument, diagnostics });

        private readonly RazorProjectEngine _projectEngine;

        private readonly Microsoft.Extensions.FileProviders.IFileProvider _fileProvider;

        private readonly IMemoryStreamFactory _memoryStreamFactory;

        public StatiqViewCompiler(
            IViewCompilerProvider innerViewCompilerProvider,
            RazorProjectEngine projectEngine,
            Microsoft.Extensions.FileProviders.IFileProvider fileProvider,
            IMemoryStreamFactory memoryStreamFactory,
            INamespacesCollection namespaces)
        {
            InnerViewCompilerProvider = innerViewCompilerProvider;
            _projectEngine = projectEngine;
            _fileProvider = fileProvider;
            _memoryStreamFactory = memoryStreamFactory;

            CompilationParameters = new CompilationParameters
            {
                BasePageType = null,
                IsDocumentModel = false,
                CacheCode = 0
            };

            // Ensure that the custom phases are registered for the global view engine
            EnsurePhases(projectEngine, CompilationParameters, namespaces.ToArray());
        }

        public IViewCompilerProvider InnerViewCompilerProvider { get; }

        public CompilationParameters CompilationParameters { get; }

        // This is the reason this whole class exists - it's the only place and way to intercept layout and partial compilation
        public async Task<CompiledViewDescriptor> CompileAsync(string relativePath)
        {
            // Get the project item which contains the absolute physical path and then get the file
            string normalizedPath = GetNormalizedPath(relativePath);
            List<IChangeToken> expirationTokens = new List<IChangeToken>
            {
                _fileProvider.Watch(normalizedPath),
            };
            FileProviderRazorProjectItem projectItem = (FileProviderRazorProjectItem)_projectEngine.FileSystem.GetItem(normalizedPath, fileKind: null);
            IFile file = (projectItem.FileInfo as StatiqFileInfo)?.File; // Might be a NotFoundFileInfo
            if (!projectItem.Exists || file is null)
            {
                IExecutionContext.Current.LogDebug($"Requested Razor view {normalizedPath} does not exist");
                return new CompiledViewDescriptor
                {
                    RelativePath = normalizedPath,
                    ExpirationTokens = expirationTokens
                };
            }

            // Check the cache
            CompilerCacheKey cacheKey = CompilerCacheKey.Get(null, await file.GetCacheCodeAsync());
            CompilationResult compilationResult = GetOrAddCachedCompilation(cacheKey, _ =>
            {
                RazorCodeDocument codeDocument = _projectEngine.Process(projectItem);
                RazorCSharpDocument cSharpDocument = codeDocument.GetCSharpDocument();
                if (cSharpDocument.Diagnostics.Count > 0)
                {
                    throw CreateCompilationFailedException(codeDocument, cSharpDocument.Diagnostics);
                }
                IExecutionContext.Current.LogDebug($"Compiling " + projectItem.FilePath);
                return CompileAndEmit(codeDocument, cSharpDocument.GeneratedCode);
            });

            // Create a view descriptor from the result
            return new CompiledViewDescriptor(compilationResult.CompiledItem)
            {
                RelativePath = normalizedPath,
                ExpirationTokens = expirationTokens
            };
        }

        public IViewCompiler GetCompiler() => this;

        public CSharpCompilation CreateCompilation(string generatedCode, string assemblyName) =>
            (CSharpCompilation)CreateCompilationMethod.Invoke(InnerViewCompilerProvider.GetCompiler(), new object[] { generatedCode, assemblyName });

        public string GetNormalizedPath(string relativePath) =>
            (string)GetNormalizedPathMethod.Invoke(InnerViewCompilerProvider.GetCompiler(), new object[] { relativePath });

        // Adapted from RuntimeViewCompiler.CompileAndEmit() (Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation.dll) to save assembly to disk for caching
        // Also called from RazorCompiler for consistency (from the single global StatiqViewCompiler instance for access to the InnerViewCompilerProvider)
        public CompilationResult CompileAndEmit(RazorCodeDocument codeDocument, string generatedCode)
        {
            // Create the compilation
            string assemblyName = Path.GetRandomFileName();
            CSharpCompilation compilation = CreateCompilation(generatedCode, assemblyName);

            // Emit the compilation to memory streams (disposed later at the end of this execution round)
            MemoryStream assemblyStream = _memoryStreamFactory?.GetStream() ?? new MemoryStream();
            MemoryStream pdbStream = _memoryStreamFactory?.GetStream() ?? new MemoryStream();
            EmitResult result = compilation.Emit(
                assemblyStream,
                pdbStream,
                options: AssemblyEmitOptions);

            if (!result.Success)
            {
                throw CreateCompilationFailedException(codeDocument, Array.Empty<RazorDiagnostic>());
            }

            // Load the assembly from the streams
            assemblyStream.Seek(0, SeekOrigin.Begin);
            pdbStream.Seek(0, SeekOrigin.Begin);
            Assembly assembly = Assembly.Load(assemblyStream.ToArray(), pdbStream.ToArray());

            // Get the Razor item and return
            RazorCompiledItem razorCompiledItem = CompiledItemLoader.LoadItems(assembly).SingleOrDefault();
            return new CompilationResult(assemblyName, assemblyStream, pdbStream, assembly, razorCompiledItem);
        }
    }
}