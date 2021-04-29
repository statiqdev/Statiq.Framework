using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using ConcurrentCollections;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Routing;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Razor
{
    /// <summary>
    /// Holds references to Razor objects based on the compilation parameters. This ensures the compilation cache and other
    /// service objects are persisted from one generation to the next, given the same compilation parameters.
    /// </summary>
    internal class RazorCompiler
    {
        private const string ViewStartFileName = "_ViewStart.cshtml";

        private static readonly RazorCompiledItemLoader CompiledItemLoader = new RazorCompiledItemLoader();

        private static readonly EmitOptions AssemblyEmitOptions = new EmitOptions(debugInformationFormat: DebugInformationFormat.PortablePdb);

        private static readonly MethodInfo CreateCompilationMethod;
        private static readonly MethodInfo CreateCompilationFailedException;

        private readonly ConcurrentCache<CompilerCacheKey, CompilationResult> _compilationCache
            = new ConcurrentCache<CompilerCacheKey, CompilationResult>();

        // Used to track compilation result requests on each execution so stale cache entries can be cleared
        private readonly ConcurrentHashSet<CompilerCacheKey> _requestedCompilationResults = new ConcurrentHashSet<CompilerCacheKey>();

        private readonly RazorProjectEngine _projectEngine;

        private readonly IServiceScopeFactory _serviceScopeFactory;

        private readonly object _phasesInitializationLock = new object();
        private bool _phasesInitialized;

        static RazorCompiler()
        {
            Type runtimeViewCompilerType = typeof(FileProviderRazorProjectItem).Assembly
                .GetType("Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation.RuntimeViewCompiler");
            CreateCompilationMethod = runtimeViewCompilerType.GetMethod(
                "CreateCompilation",
                BindingFlags.Instance | BindingFlags.NonPublic,
                Type.DefaultBinder,
                new Type[] { typeof(string), typeof(string) },
                null);
            Type compilationFailedExceptionFactory = typeof(FileProviderRazorProjectItem).Assembly
                .GetType("Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation.CompilationFailedExceptionFactory");
            CreateCompilationFailedException = compilationFailedExceptionFactory.GetMethod(
                "Create",
                new Type[] { typeof(RazorCodeDocument), typeof(IEnumerable<RazorDiagnostic>) });
        }

        /// <summary>
        /// Creates a Razor compiler using an existing set of services (which must already have Razor services registered).
        /// </summary>
        /// <param name="parameters">The compilation parameters.</param>
        /// <param name="serviceProvider">The service provider to use.</param>
        public RazorCompiler(CompilationParameters parameters, IServiceProvider serviceProvider)
        {
            serviceProvider.ThrowIfNull(nameof(serviceProvider));

            IExecutionContext.Current.LogDebug($"Creating new {nameof(RazorCompiler)} for {parameters.BasePageType ?? "null base page type"}");

            // Do a check to make sure required services are registered
            _projectEngine = serviceProvider.GetService<RazorProjectEngine>();
            if (_projectEngine is null)
            {
                // Razor services haven't been registered so create a new services container for this compiler
                ServiceCollection serviceCollection = new ServiceCollection();
                serviceCollection.AddSingleton(serviceProvider.GetRequiredService<ILoggerFactory>());
                serviceCollection.AddRazor(
                    serviceProvider.GetRequiredService<IReadOnlyFileSystem>(),
                    serviceProvider.GetService<ClassCatalog>());
                serviceProvider = serviceCollection.BuildServiceProvider();
                _projectEngine = serviceProvider.GetRequiredService<RazorProjectEngine>();
            }

            _serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        }

        // We need to initialize lazily since restoring from the cache won't have the actual namespaces, only a cache code
        public void EnsurePhases(CompilationParameters parameters, string[] namespaces)
        {
            parameters.ThrowIfNull(nameof(parameters));

            if (!_phasesInitialized)
            {
                lock (_phasesInitializationLock)
                {
                    // We need to register a new document classifier phase because builder.SetBaseType() (which uses builder.ConfigureClass())
                    // use the DefaultRazorDocumentClassifierPhase which stops applying document classifier passes after DocumentIntermediateNode.DocumentKind is set
                    // (which gets set by the Razor document classifier passes registered in RazorExtensions.Register())
                    // Also need to add it just after the DocumentClassifierPhase, otherwise it'll miss the C# lowering phase
                    List<IRazorEnginePhase> phases = _projectEngine.Engine.Phases.ToList();
                    StatiqDocumentClassifierPhase phase =
                        new StatiqDocumentClassifierPhase(parameters.BasePageType, namespaces, parameters.IsDocumentModel, _projectEngine.Engine);
                    phases.Insert(phases.IndexOf(phases.OfType<IRazorDocumentClassifierPhase>().Last()) + 1, phase);
                    FieldInfo phasesField = _projectEngine.Engine.GetType().GetField("<Phases>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
                    phasesField.SetValue(_projectEngine.Engine, phases.ToArray());
                }
                _phasesInitialized = true;
            }
        }

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
                        CompiledItemLoader.LoadItems(assembly).SingleOrDefault());
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

            // Use a new scope to get the file provider and expire change tokens
            using (IServiceScope scope = _serviceScopeFactory.CreateScope())
            {
                Microsoft.Extensions.FileProviders.IFileProvider fileProvider =
                    scope.ServiceProvider.GetService<Microsoft.Extensions.FileProviders.IFileProvider>();
                ((FileSystemFileProvider)fileProvider).ExpireChangeTokens();
            }

            return _compilationCache;
        }

        public async Task RenderPageAsync(RenderRequest request)
        {
            using (IServiceScope scope = _serviceScopeFactory.CreateScope())
            {
                IServiceProvider serviceProvider = scope.ServiceProvider;
                IRazorPage page = await GetPageFromStreamAsync(serviceProvider, request);
                IView view = GetViewFromStream(serviceProvider, request, page);

                using (StreamWriter writer = request.Output.GetWriter())
                {
                    Microsoft.AspNetCore.Mvc.Rendering.ViewContext viewContext = GetViewContext(serviceProvider, request, view, writer);
                    await viewContext.View.RenderAsync(viewContext);
                }
            }
        }

        private ViewContext GetViewContext(IServiceProvider serviceProvider, RenderRequest request, IView view, TextWriter output)
        {
            HttpContext httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProvider
            };

            ActionContext actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            ViewDataDictionary viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), actionContext.ModelState)
            {
                Model = request.Model
            };

            if (request.ViewData != null)
            {
                foreach (KeyValuePair<string, object> pair in request.ViewData)
                {
                    viewData.Add(pair);
                }
            }

            ITempDataDictionary tempData = new TempDataDictionary(actionContext.HttpContext, serviceProvider.GetRequiredService<ITempDataProvider>());

            return new ViewContext(
                actionContext,
                view,
                viewData,
                tempData,
                output,
                new HtmlHelperOptions(),
                request.Document,
                request.Context,
                serviceProvider);
        }

        /// <summary>
        /// Gets the view for an input document (which is different than the view for a layout, partial, or
        /// other indirect view because it's not necessarily on disk or in the file system).
        /// </summary>
        private IView GetViewFromStream(IServiceProvider serviceProvider, RenderRequest request, IRazorPage page)
        {
            StatiqRazorProjectFileSystem projectFileSystem = serviceProvider.GetRequiredService<StatiqRazorProjectFileSystem>();

            IEnumerable<string> viewStartLocations = request.ViewStartLocation is object
                ? new[] { request.ViewStartLocation }
                : projectFileSystem.FindHierarchicalItems(request.RelativePath, ViewStartFileName).Select(x => x.FilePath);

            List<IRazorPage> viewStartPages = viewStartLocations
                .Select(serviceProvider.GetRequiredService<IRazorPageFactoryProvider>().CreateFactory)
                .Where(x => x.Success)
                .Select(x => x.RazorPageFactory())
                .Reverse()
                .ToList();

            if (request.LayoutLocation is object)
            {
                page.Layout = request.LayoutLocation;
            }

            IRazorViewEngine viewEngine = serviceProvider.GetRequiredService<IRazorViewEngine>();
            IRazorPageActivator pageActivator = serviceProvider.GetRequiredService<IRazorPageActivator>();
            HtmlEncoder htmlEncoder = serviceProvider.GetRequiredService<HtmlEncoder>();
            DiagnosticListener diagnosticListener = serviceProvider.GetRequiredService<DiagnosticListener>();

            return new RazorView(viewEngine, pageActivator, viewStartPages, page, htmlEncoder, diagnosticListener);
        }

        /// <summary>
        /// Gets the Razor page for an input document stream. This is roughly modeled on
        /// DefaultRazorPageFactory and CompilerCache. Note that we don't actually bother
        /// with caching the page if it's from a live stream.
        /// </summary>
        private async Task<IRazorPage> GetPageFromStreamAsync(IServiceProvider serviceProvider, RenderRequest request)
        {
            string relativePath = request.RelativePath;

            if (relativePath.StartsWith("~/", StringComparison.Ordinal))
            {
                // For tilde slash paths, drop the leading ~ to make it work with the underlying IFileProvider.
                relativePath = relativePath.Substring(1);
            }

            // Get the file info by combining the stream content with info found at the document's original location (if any)
            StatiqRazorProjectFileSystem projectFileSystem = serviceProvider.GetRequiredService<StatiqRazorProjectFileSystem>();
            RazorProjectItem projectItem = projectFileSystem.GetItem(relativePath, request.Document);

            // Compute a hash for the content since pipelines could have changed it from the underlying file
            int contentCacheCode = await request.Document.ContentProvider.GetCacheCodeAsync();

            CompilationResult compilationResult = CompilePage(request, contentCacheCode, projectItem);
            return compilationResult.GetPage(request.RelativePath);
        }

        /// <summary>
        /// Checks the cache for a matching compilation and then compiles and loads the dynamic assembly if a miss.
        /// </summary>
        private CompilationResult CompilePage(RenderRequest request, int contentCacheCode, RazorProjectItem projectItem)
        {
            CompilerCacheKey compilerCacheKey = CompilerCacheKey.Get(request, contentCacheCode);
            _requestedCompilationResults.Add(compilerCacheKey);
            return _compilationCache.GetOrAdd(compilerCacheKey, _ => GetCompilation(projectItem));
        }

        /// <summary>
        /// Gets the assembly and loads it for a compilation cache miss.
        /// </summary>
        private CompilationResult GetCompilation(RazorProjectItem projectItem)
        {
            IExecutionContext.Current.LogDebug($"Compiling " + projectItem.FilePath);
            using (IServiceScope scope = _serviceScopeFactory.CreateScope())
            {
                IServiceProvider serviceProvider = scope.ServiceProvider;

                // See RazorViewCompiler.CompileAndEmit()
                RazorCodeDocument codeDocument = _projectEngine.Process(projectItem);
                RazorCSharpDocument cSharpDocument = codeDocument.GetCSharpDocument();
                if (cSharpDocument.Diagnostics.Count > 0)
                {
                    throw (Exception)CreateCompilationFailedException.Invoke(
                        null,
                        new object[] { codeDocument, cSharpDocument.Diagnostics });
                }

                // Use the RazorViewCompiler to finish compiling the view for consistency with layouts
                IViewCompilerProvider viewCompilerProvider = serviceProvider.GetRequiredService<IViewCompilerProvider>();
                IViewCompiler viewCompiler = viewCompilerProvider.GetCompiler();
                IMemoryStreamFactory memoryStreamFactory = serviceProvider.GetRequiredService<IMemoryStreamFactory>();
                return CompileAndEmit(memoryStreamFactory, viewCompiler, codeDocument, cSharpDocument.GeneratedCode);
            }
        }

        // Adapted from RuntimeViewCompiler.CompileAndEmit() (Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation.dll) to save assembly to disk for caching
        private CompilationResult CompileAndEmit(IMemoryStreamFactory memoryStreamFactory, IViewCompiler viewCompiler, RazorCodeDocument codeDocument, string generatedCode)
        {
            // Create the compilation
            string assemblyName = Path.GetRandomFileName();
            CSharpCompilation compilation = (CSharpCompilation)CreateCompilationMethod.Invoke(
                    viewCompiler,
                    new object[] { generatedCode, assemblyName });

            // Emit the compilation to memory streams (disposed later at the end of this execution round)
            MemoryStream assemblyStream = memoryStreamFactory.GetStream();
            MemoryStream pdbStream = memoryStreamFactory.GetStream();
            EmitResult result = compilation.Emit(
                assemblyStream,
                pdbStream,
                options: AssemblyEmitOptions);

            if (!result.Success)
            {
                throw (Exception)CreateCompilationFailedException.Invoke(
                    null,
                    new object[] { codeDocument, result.Diagnostics });
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