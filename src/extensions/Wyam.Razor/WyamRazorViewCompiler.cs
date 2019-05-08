using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace Wyam.Razor
{
    internal class WyamRazorViewCompiler : RazorViewCompiler
    {
        public WyamRazorViewCompiler(
            IFileProvider fileProvider,
            RazorProjectEngine projectEngine,
            CSharpCompiler csharpCompiler,
            Action<RoslynCompilationContext> compilationCallback,
            IList<CompiledViewDescriptor> precompiledViews,
            ILogger logger)
            : base(
                  fileProvider,
                  projectEngine,
                  csharpCompiler,
                  compilationCallback,
                  precompiledViews,
                  logger)
        {
        }

        protected override CompiledViewDescriptor CompileAndEmit(string relativePath)
        {
            CompiledViewDescriptor descriptor = base.CompileAndEmit(relativePath);

            // The Razor compiler adds attributes to the generated IRazorPage code that provide the relative path of the page
            // but since Wyam uses "invisible" input path(s) that appear in the physical file system but not the virtual one,
            // we have to remove the input path from the start of the relative path - otherwise we'll end up looking for nested
            // views in locations like "/input/input/_foo.cshtml"
            if (descriptor.RelativePath.EndsWith(relativePath))
            {
                descriptor.RelativePath = relativePath;
            }

            return descriptor;
        }
    }
}