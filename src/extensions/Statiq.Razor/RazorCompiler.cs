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

        private static readonly MethodInfo CompileAndEmitMethod;
        private static readonly MethodInfo CreateCompilationFailedException;

        private readonly ConcurrentCache<CompilerCacheKey, CompilationResult> _compilationCache
            = new ConcurrentCache<CompilerCacheKey, CompilationResult>();

        private readonly IServiceScopeFactory _serviceScopeFactory;

        static RazorCompiler()
        {
            Type runtimeViewCompilerType = typeof(FileProviderRazorProjectItem).Assembly
                .GetType("Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation.RuntimeViewCompiler");
            CompileAndEmitMethod = runtimeViewCompilerType.GetMethod(
                "CompileAndEmit",
                BindingFlags.Instance | BindingFlags.NonPublic,
                Type.DefaultBinder,
                new Type[] { typeof(RazorCodeDocument), typeof(string) },
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

            IExecutionContext.CurrentOrNull?.LogDebug($"Creating new {nameof(RazorCompiler)} for {parameters.BasePageType?.Name ?? "null base page type"}");

            // Do a check to make sure required services are registered
            RazorProjectEngine razorProjectEngine = serviceProvider.GetService<RazorProjectEngine>();
            if (razorProjectEngine == null)
            {
                // Razor services haven't been registered so create a new services container for this compiler
                ServiceCollection serviceCollection = new ServiceCollection();
                serviceCollection.AddSingleton(serviceProvider.GetRequiredService<ILoggerFactory>());
                serviceCollection.AddRazor(
                    serviceProvider.GetRequiredService<IReadOnlyFileSystem>(),
                    serviceProvider.GetService<ClassCatalog>());
                serviceProvider = serviceCollection.BuildServiceProvider();
                razorProjectEngine = serviceProvider.GetRequiredService<RazorProjectEngine>();
            }

            _serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

            // Calculate the base page type
            Type basePageType = parameters.BasePageType ?? typeof(StatiqRazorPage<>);
            string baseClassName = basePageType.FullName;
            int tickIndex = baseClassName.IndexOf('`');
            if (tickIndex > 0)
            {
                baseClassName = baseClassName.Substring(0, tickIndex);
            }
            string baseType = basePageType.IsGenericTypeDefinition ? $"{baseClassName}<TModel>" : baseClassName;

            // We need to register a new document classifier phase because builder.SetBaseType() (which uses builder.ConfigureClass())
            // use the DefaultRazorDocumentClassifierPhase which stops applying document classifier passes after DocumentIntermediateNode.DocumentKind is set
            // (which gets set by the Razor document classifier passes registered in RazorExtensions.Register())
            // Also need to add it just after the DocumentClassifierPhase, otherwise it'll miss the C# lowering phase
            List<IRazorEnginePhase> phases = razorProjectEngine.Engine.Phases.ToList();
            phases.Insert(
                phases.IndexOf(phases.OfType<IRazorDocumentClassifierPhase>().Last()) + 1,
                new StatiqDocumentPhase(baseType, parameters.Namespaces)
                {
                    Engine = razorProjectEngine.Engine
                });
            FieldInfo phasesField = razorProjectEngine.Engine.GetType().GetField("<Phases>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            phasesField.SetValue(razorProjectEngine.Engine, phases.ToArray());
        }

        public void ExpireChangeTokens()
        {
            // Use a new scope to get the file provider
            using (IServiceScope scope = _serviceScopeFactory.CreateScope())
            {
                Microsoft.Extensions.FileProviders.IFileProvider fileProvider =
                    scope.ServiceProvider.GetService<Microsoft.Extensions.FileProviders.IFileProvider>();
                ((FileSystemFileProvider)fileProvider).ExpireChangeTokens();
            }
        }

        public async Task RenderPageAsync(RenderRequest request)
        {
            using (IServiceScope scope = _serviceScopeFactory.CreateScope())
            {
                IServiceProvider serviceProvider = scope.ServiceProvider;
                IRazorPage page = GetPageFromStream(serviceProvider, request);
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

            IEnumerable<string> viewStartLocations = request.ViewStartLocation != null
                ? new[] { request.ViewStartLocation }
                : projectFileSystem.FindHierarchicalItems(request.RelativePath, ViewStartFileName).Select(x => x.FilePath);

            List<IRazorPage> viewStartPages = viewStartLocations
                .Select(serviceProvider.GetRequiredService<IRazorPageFactoryProvider>().CreateFactory)
                .Where(x => x.Success)
                .Select(x => x.RazorPageFactory())
                .Reverse()
                .ToList();

            if (request.LayoutLocation != null)
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
        private IRazorPage GetPageFromStream(IServiceProvider serviceProvider, RenderRequest request)
        {
            string relativePath = request.RelativePath;

            if (relativePath.StartsWith("~/", StringComparison.Ordinal))
            {
                // For tilde slash paths, drop the leading ~ to make it work with the underlying IFileProvider.
                relativePath = relativePath.Substring(1);
            }

            // Get the file info by combining the stream content with info found at the document's original location (if any)
            StatiqRazorProjectFileSystem projectFileSystem = serviceProvider.GetRequiredService<StatiqRazorProjectFileSystem>();
            RazorProjectItem projectItem = projectFileSystem.GetItem(relativePath, request.Input);

            // Compute a hash for the content since pipelines could have changed it from the underlying file
            // We have to pre-compute the hash (I.e., no CryptoStream) since we need to check for a hit before reading/compiling the view
            byte[] hash = SHA512.Create().ComputeHash(request.Input);
            request.Input.Position = 0;

            CompilationResult compilationResult = CompilePage(request, hash, projectItem);

            return compilationResult.GetPage(request.RelativePath);
        }

        private CompilationResult CompilePage(RenderRequest request, byte[] hash, RazorProjectItem projectItem)
        {
            CompilerCacheKey cacheKey = new CompilerCacheKey(request, hash);
            return _compilationCache.GetOrAdd(cacheKey, _ => GetCompilation(projectItem));
        }

        private CompilationResult GetCompilation(RazorProjectItem projectItem)
        {
            IExecutionContext.CurrentOrNull?.LogDebug($"Compiling " + projectItem.FilePath);
            using (IServiceScope scope = _serviceScopeFactory.CreateScope())
            {
                IServiceProvider serviceProvider = scope.ServiceProvider;

                // See RazorViewCompiler.CompileAndEmit()
                RazorProjectEngine projectEngine = serviceProvider.GetRequiredService<RazorProjectEngine>();
                RazorCodeDocument codeDocument = projectEngine.Process(projectItem);
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
                Assembly assembly = (Assembly)CompileAndEmitMethod.Invoke(
                    viewCompiler,
                    new object[] { codeDocument, cSharpDocument.GeneratedCode });

                // Get the runtime item
                RazorCompiledItemLoader compiledItemLoader = new RazorCompiledItemLoader();
                RazorCompiledItem compiledItem = compiledItemLoader.LoadItems(assembly).SingleOrDefault();
                return new CompilationResult(compiledItem);
            }
        }
    }
}