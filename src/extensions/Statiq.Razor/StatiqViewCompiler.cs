using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Statiq.Common;

namespace Statiq.Razor
{
    // RuntimeViewCompiler is internal and the Razor team has made it clear they're not interested in maintaining a public API
    // So we've got to encapsulate it and get what we need via reflection
    internal class StatiqViewCompiler : IViewCompiler, IViewCompilerProvider
    {
        public static readonly RazorCompiledItemLoader CompiledItemLoader = new RazorCompiledItemLoader();

        private static readonly EmitOptions AssemblyEmitOptions = new EmitOptions(debugInformationFormat: DebugInformationFormat.PortablePdb);

        private static readonly MethodInfo CreateCompilationMethod;

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

            Type compilationFailedExceptionFactory = typeof(FileProviderRazorProjectItem).Assembly
                .GetType("Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation.CompilationFailedExceptionFactory");
            CreateCompilationFailedExceptionMethod = compilationFailedExceptionFactory.GetMethod(
                "Create",
                new Type[] { typeof(RazorCodeDocument), typeof(IEnumerable<RazorDiagnostic>) });
        }

        public static Exception CreateCompilationFailedException(RazorCodeDocument codeDocument, IEnumerable<RazorDiagnostic> diagnostics) =>
            (Exception)CreateCompilationFailedExceptionMethod.Invoke(null, new object[] { codeDocument, diagnostics });

        public StatiqViewCompiler(IViewCompilerProvider innerViewCompilerProvider)
        {
            InnerViewCompilerProvider = innerViewCompilerProvider;
        }

        public IViewCompilerProvider InnerViewCompilerProvider { get; }

        public async Task<CompiledViewDescriptor> CompileAsync(string relativePath)
        {
            return await InnerViewCompilerProvider.GetCompiler().CompileAsync(relativePath);
        }

        public IViewCompiler GetCompiler() => this;

        public CSharpCompilation CreateCompilation(string generatedCode, string assemblyName) =>
            (CSharpCompilation)CreateCompilationMethod.Invoke(InnerViewCompilerProvider.GetCompiler(), new object[] { generatedCode, assemblyName });

        // Adapted from RuntimeViewCompiler.CompileAndEmit() (Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation.dll) to save assembly to disk for caching
        public CompilationResult CompileAndEmit(IMemoryStreamFactory memoryStreamFactory, RazorCodeDocument codeDocument, string generatedCode)
        {
            // Create the compilation
            string assemblyName = Path.GetRandomFileName();
            CSharpCompilation compilation = CreateCompilation(generatedCode, assemblyName);

            // Emit the compilation to memory streams (disposed later at the end of this execution round)
            MemoryStream assemblyStream = memoryStreamFactory?.GetStream() ?? new MemoryStream();
            MemoryStream pdbStream = memoryStreamFactory?.GetStream() ?? new MemoryStream();
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