using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Razor.Hosting;

namespace Statiq.Razor
{
    internal class CompilationResult
    {
        public CompilationResult(
            string assemblyName,
            MemoryStream assemblyStream,
            MemoryStream pdbStream,
            Assembly assembly,
            RazorCompiledItem item)
        {
            AssemblyName = assemblyName;
            AssemblyStream = assemblyStream;
            PdbStream = pdbStream;
            Assembly = assembly;
            CompiledItem = item;
        }

        public string AssemblyName { get; }

        public MemoryStream AssemblyStream { get; private set; }

        public MemoryStream PdbStream { get; private set; }

        public Assembly Assembly { get; }

        public RazorCompiledItem CompiledItem { get; }

        public IRazorPage GetPage(string relativePath)
        {
            IRazorPage page = (IRazorPage)Activator.CreateInstance(CompiledItem.Type);
            page.Path = relativePath;
            return page;
        }

        /// <summary>
        /// Disposes the memory streams and returns them to the pool (if they were recyclable).
        /// Can be called after the assembly has been written to disk for caching.
        /// </summary>
        public void DisposeMemoryStreams()
        {
            if (AssemblyStream is object)
            {
                AssemblyStream.Dispose();
                AssemblyStream = null;
            }
            if (PdbStream is object)
            {
                PdbStream.Dispose();
                PdbStream = null;
            }
        }
    }
}