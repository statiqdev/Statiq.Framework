using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.ObjectPool;
using Statiq.Common;

namespace Statiq.Razor
{
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Adds all services required for the <see cref="RenderRazor"/> module.
        /// </summary>
        /// <param name="serviceCollection">The service collection to register services in.</param>
        /// <param name="fileSystem">The file system or <c>null</c> if we're expecting one to already be registered.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddRazor(this IServiceCollection serviceCollection, IReadOnlyFileSystem fileSystem = null)
        {
            // Register the file system if we're not expecting one from an engine
            if (fileSystem is object)
            {
                serviceCollection.TryAddSingleton(fileSystem);
            }

            // Register some of our own types if not already registered
            serviceCollection.TryAddSingleton<Microsoft.Extensions.FileProviders.IFileProvider, FileSystemFileProvider>();
            serviceCollection.TryAddSingleton<DiagnosticSource, SilentDiagnosticSource>();
            serviceCollection.TryAddSingleton(new DiagnosticListener("Razor"));
            serviceCollection.TryAddSingleton<IWebHostEnvironment, HostEnvironment>();
            serviceCollection.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            serviceCollection.TryAddSingleton<StatiqRazorProjectFileSystem>();
            serviceCollection.TryAddSingleton<RazorProjectFileSystem, StatiqRazorProjectFileSystem>();
            serviceCollection.TryAddSingleton<RazorService>();

            // Register the view location expander if not already registered
            serviceCollection.Configure<RazorViewEngineOptions>(x =>
            {
                if (!x.ViewLocationExpanders.OfType<ViewLocationExpander>().Any())
                {
                    x.ViewLocationExpanders.Add(new ViewLocationExpander());
                }
            });

            // Add the default services _after_ adding our own
            // (most default registration use .TryAdd...() so they skip already registered types)
            IMvcCoreBuilder builder = serviceCollection
                .AddMvcCore()
                .AddRazorViewEngine();

            // Remove *any* existing IViewCompilerProvider registration before adding the one in AddRazorRuntimeCompilation (see #204)
            ServiceDescriptor viewCompilerProviderDescriptor = builder.Services.FirstOrDefault((ServiceDescriptor f) => f.ServiceType == typeof(IViewCompilerProvider));
            if (viewCompilerProviderDescriptor is object)
            {
                builder.Services.Remove(viewCompilerProviderDescriptor);
            }

            builder = builder.AddRazorRuntimeCompilation();

            // Make the project engine scoped because we have to replace the phases based on base page type and namespaces
            ServiceDescriptor razorProjectEngineDescriptor = serviceCollection.First(x => x.ServiceType == typeof(RazorProjectEngine));
            serviceCollection.Replace(ServiceDescriptor.Describe(
                typeof(RazorProjectEngine),
                razorProjectEngineDescriptor.ImplementationFactory,
                ServiceLifetime.Scoped));

            // Replace the runtime view compiler provider with our own
            // Create a short-lived service provider to get an instance we can inject
            // This is expected to be a RuntimeViewCompilerProvider (see #204)
            viewCompilerProviderDescriptor = serviceCollection.First((ServiceDescriptor f) => f.ServiceType == typeof(IViewCompilerProvider));
            serviceCollection.Replace(ServiceDescriptor.Describe(
                typeof(IViewCompilerProvider),
                serviceProvider =>
                    new StatiqViewCompiler(
                        (IViewCompilerProvider)serviceProvider.CreateInstance(viewCompilerProviderDescriptor),
                        serviceProvider.GetRequiredService<RazorProjectEngine>(),
                        serviceProvider.GetRequiredService<Microsoft.Extensions.FileProviders.IFileProvider>(),
                        serviceProvider.GetService<IMemoryStreamFactory>(),
                        serviceProvider.GetRequiredService<INamespacesCollection>()),
                viewCompilerProviderDescriptor.Lifetime));

            // Add the reference provider as an ApplicationPart, getting an existing
            // ClassCatalog from the service collection if there is one
            ClassCatalog classCatalog = builder.Services.GetImplementationInstance<ClassCatalog>();
            CompilationReferencesProvider referencesProvider = new CompilationReferencesProvider(classCatalog);
            builder.ConfigureApplicationPartManager(x => x.ApplicationParts.Add(referencesProvider));

            return serviceCollection;
        }

        // Need to use a custom ICompilationReferencesProvider because the default one won't provide a path for the running assembly
        // (see Microsoft.AspNetCore.Mvc.ApplicationParts.AssemblyPartExtensions.GetReferencePaths() for why,
        // the running assembly returns a DependencyContext when used with "dotnet run" and therefore won't return it's own path)
        // We also need to lazily calculate the reference paths so that late additions to the ClassCatalog get
        // picked up (like compiled theme projects)
        private class CompilationReferencesProvider : ApplicationPart, ICompilationReferencesProvider
        {
            private static readonly object ReferencePathLock = new object();

            private readonly ClassCatalog _classCatalog;

            private string[] _referencePaths;

            public CompilationReferencesProvider(ClassCatalog classCatalog)
            {
                _classCatalog = classCatalog;
            }

            public HashSet<Assembly> Assemblies { get; } = new HashSet<Assembly>(new AssemblyComparer());

            public override string Name => nameof(CompilationReferencesProvider);

            public IEnumerable<string> GetReferencePaths()
            {
                lock (ReferencePathLock)
                {
                    if (_referencePaths is null)
                    {
                        HashSet<Assembly> assemblies = new HashSet<Assembly>(new AssemblyComparer());
                        assemblies.AddRange((_classCatalog ?? new ClassCatalog()).GetAssemblies());

                        // And a couple needed assemblies that might not be loaded in the AppDomain yet
                        assemblies.Add(typeof(IHtmlContent).Assembly);
                        assemblies.Add(Assembly.Load(new AssemblyName("Microsoft.CSharp")));

                        _referencePaths = assemblies
                            .Where(x => !x.IsDynamic && !string.IsNullOrEmpty(x.Location))
                            .Select(x => x.Location)
                            .ToArray();
                    }

                    return _referencePaths;
                }
            }
        }
    }
}