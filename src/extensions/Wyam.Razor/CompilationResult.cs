using System;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Razor.Hosting;

namespace Wyam.Razor
{
    internal class CompilationResult
    {
        private RazorCompiledItem CompiledItem { get; }

        public CompilationResult(RazorCompiledItem item)
        {
            CompiledItem = item;
        }

        public IRazorPage GetPage(string relativePath)
        {
            IRazorPage page = (IRazorPage)Activator.CreateInstance(CompiledItem.Type);
            page.Path = relativePath;
            return page;
        }
    }
}