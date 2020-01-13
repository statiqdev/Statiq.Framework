using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.CodeAnalysis.Scripting
{
    public static class ScriptHelper
    {
        public const string AssemblyName = "ScriptAssembly";
        public const string ScriptClassName = "Script";

        private static readonly HashSet<string> ReservedPropertyNames =
            new HashSet<string>(typeof(ScriptBase).GetMembers().Select(x => x.Name), StringComparer.OrdinalIgnoreCase);

        public static byte[] Compile(string code, IDocument document, IExecutionContext context)
        {
            _ = code ?? throw new ArgumentNullException(nameof(code));
            _ = document ?? throw new ArgumentNullException(nameof(document));
            _ = context ?? throw new ArgumentNullException(nameof(context));

            // Parse the code
            code = Parse(code, document, context);

            // Get the compilation
            CSharpParseOptions parseOptions = new CSharpParseOptions();
            SourceText sourceText = SourceText.From(code, Encoding.UTF8);
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceText, parseOptions, AssemblyName);
            CSharpCompilationOptions compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).
                WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic>
                {
                    // ensure that specific warnings about assembly references are always suppressed
                    // https://github.com/dotnet/roslyn/issues/5501
                    { "CS1701", ReportDiagnostic.Suppress },
                    { "CS1702", ReportDiagnostic.Suppress },
                    { "CS1705", ReportDiagnostic.Suppress },

                    // we don't care about unreachable code
                    { "CS0162", ReportDiagnostic.Suppress },
                });

            // For some reason, Roslyn really wants these added by filename
            // See http://stackoverflow.com/questions/23907305/roslyn-has-no-reference-to-system-runtime
            string assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
            CSharpCompilation compilation = CSharpCompilation.Create(
                AssemblyName,
                new[] { syntaxTree },
                AppDomain.CurrentDomain.GetAssemblies()
                    .Where(x => !x.IsDynamic && !string.IsNullOrEmpty(x.Location))
                    .Select(x => MetadataReference.CreateFromFile(x.Location)), compilationOptions)
                    .AddReferences(
                        MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")),
                        MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")),
                        MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll")),
                        MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")));

            // Emit the assembly
            using (MemoryStream ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                // Log warnings
                List<string> warningMessages = result.Diagnostics
                    .Where(x => x.Severity == DiagnosticSeverity.Warning)
                    .Select(GetCompilationErrorMessage)
                    .ToList();
                if (warningMessages.Count > 0)
                {
                    context.LogWarning(
                        "{0} warnings compiling script:{1}{2}",
                        warningMessages.Count,
                        Environment.NewLine,
                        string.Join(Environment.NewLine, warningMessages));
                }

                // Log errors
                List<string> errorMessages = result.Diagnostics
                    .Where(x => x.Severity == DiagnosticSeverity.Error)
                    .Select(GetCompilationErrorMessage)
                    .ToList();
                if (errorMessages.Count > 0)
                {
                    context.LogError(
                        "{0} errors compiling script:{1}{2}",
                        errorMessages.Count,
                        Environment.NewLine,
                        string.Join(Environment.NewLine, errorMessages));
                }

                // Throw for errors or not success
                if (!result.Success || errorMessages.Count > 0)
                {
                    throw new ScriptCompilationException(errorMessages);
                }

                ms.Seek(0, SeekOrigin.Begin);
                return ms.ToArray();
            }
        }

        public static Task<object> EvaluateAsync(byte[] rawAssembly, IDocument document, IExecutionContext context)
        {
            _ = rawAssembly ?? throw new ArgumentNullException(nameof(rawAssembly));
            _ = document ?? throw new ArgumentNullException(nameof(document));
            _ = context ?? throw new ArgumentNullException(nameof(context));

            Assembly assembly = Assembly.Load(rawAssembly);
            Type scriptType = assembly.GetExportedTypes().First(t => t.Name == ScriptClassName);
            ScriptBase script = (ScriptBase)Activator.CreateInstance(scriptType, document, context);
            return script.EvaluateAsync();
        }

        private static string GetCompilationErrorMessage(Diagnostic diagnostic)
        {
            string line = diagnostic.Location.IsInSource ? "Line " + (diagnostic.Location.GetMappedLineSpan().Span.Start.Line + 1) : "Metadata";
            return $"{line}: {diagnostic.Id}: {diagnostic.GetMessage()}";
        }

        // Internal for testing
        internal static string Parse(string code, IDocument document, IExecutionContext context)
        {
            // Generate a syntax tree from the code
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code, new CSharpParseOptions(kind: SourceCodeKind.Script), cancellationToken: context.CancellationToken);

            // "Lift" class and method declarations
            LiftingWalker liftingWalker = new LiftingWalker();
            liftingWalker.Visit(syntaxTree.GetRoot(context.CancellationToken));

            // Get the using statements
            string usingStatements = string.Join(
                Environment.NewLine,
                context.Namespaces
                    .Concat(new[] { "Statiq.CodeAnalysis.Scripting" })
                    .Select(x => "using " + x + ";"));

            // Get the document metadata properties
            string metadataProperties = string.Join(
                Environment.NewLine,
                document.Keys
                    .Where(x => !string.IsNullOrWhiteSpace(x) && !ReservedPropertyNames.Contains(x))
                    .Select(x => $"public object {GetValidIdentifier(x)} => Document.Get(\"{x}\");"));

            // Determine if we need a return statement
            string returnStatement = liftingWalker.HasReturnStatement ? string.Empty : "return null;";

            // Return the fully parsed script
            return
$@"{usingStatements}
{liftingWalker.UsingDirectives}
public class {ScriptClassName} : ScriptBase
{{
public {ScriptClassName}(IDocument document, IExecutionContext context) : base(document, context) {{ }}
public override async Task<object> EvaluateAsync()
{{
await Task.CompletedTask;
{liftingWalker.ScriptCode}
{returnStatement}
}}
{liftingWalker.MethodDeclarations}
{metadataProperties}
}}
{liftingWalker.TypeDeclarations}
public static class ScriptExtensionMethods
{{
{liftingWalker.ExtensionMethodDeclarations}
}}";
        }

        // https://stackoverflow.com/a/34268364/807064
        internal static string GetValidIdentifier(string identifier)
        {
            StringBuilder sb = new StringBuilder();
            identifier = identifier.Trim();
            if (!SyntaxFacts.IsIdentifierStartCharacter(identifier[0]))
            {
                sb.Append("_");
            }
            foreach (char ch in identifier)
            {
                if (SyntaxFacts.IsIdentifierPartCharacter(ch))
                {
                    sb.Append(ch);
                }
            }
            string result = sb.ToString();
            if (SyntaxFacts.GetKeywordKind(result) != SyntaxKind.None)
            {
                result = "@" + result;
            }
            return result;
        }
    }
}
