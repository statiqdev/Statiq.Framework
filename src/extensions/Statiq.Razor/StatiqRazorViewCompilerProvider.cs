using System;
using System.Threading;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Statiq.Razor
{
    /// <summary>
    /// This is copied from <see cref="RazorViewCompilerProvider"/> and exists entirely to provide
    /// <see cref="StatiqRazorViewCompiler"/> instead of <see cref="RazorViewCompiler"/>.
    /// </summary>
    internal class StatiqRazorViewCompilerProvider : IViewCompilerProvider
    {
        private readonly RazorProjectEngine _razorProjectEngine;
        private readonly ApplicationPartManager _applicationPartManager;
        private readonly IRazorViewEngineFileProviderAccessor _fileProviderAccessor;
        private readonly CSharpCompiler _csharpCompiler;
        private readonly RazorViewEngineOptions _viewEngineOptions;
        private readonly ILogger<RazorViewCompiler> _logger;
        private readonly Func<IViewCompiler> _createCompiler;
        private object _initializeLock = new object();
        private bool _initialized;
        private IViewCompiler _compiler;

        public StatiqRazorViewCompilerProvider(ApplicationPartManager applicationPartManager, RazorProjectEngine razorProjectEngine, IRazorViewEngineFileProviderAccessor fileProviderAccessor, CSharpCompiler csharpCompiler, IOptions<RazorViewEngineOptions> viewEngineOptionsAccessor, ILoggerFactory loggerFactory)
        {
            _applicationPartManager = applicationPartManager;
            _razorProjectEngine = razorProjectEngine;
            _fileProviderAccessor = fileProviderAccessor;
            _csharpCompiler = csharpCompiler;
            _viewEngineOptions = viewEngineOptionsAccessor.Value;
            _logger = loggerFactory.CreateLogger<RazorViewCompiler>();
            _createCompiler = new Func<IViewCompiler>(CreateCompiler);
        }

        public IViewCompiler GetCompiler()
        {
            if (_fileProviderAccessor.FileProvider is NullFileProvider)
            {
                throw new InvalidOperationException();
            }
            return LazyInitializer.EnsureInitialized<IViewCompiler>(ref _compiler, ref _initialized, ref _initializeLock, _createCompiler);
        }

        private IViewCompiler CreateCompiler()
        {
            ViewsFeature feature = new ViewsFeature();
            _applicationPartManager.PopulateFeature<ViewsFeature>(feature);
            return (IViewCompiler)new StatiqRazorViewCompiler(_fileProviderAccessor.FileProvider, _razorProjectEngine, _csharpCompiler, _viewEngineOptions.CompilationCallback, feature.ViewDescriptors, (ILogger)_logger);
        }
    }
}