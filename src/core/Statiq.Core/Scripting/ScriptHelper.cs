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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    public static class ScriptHelper
    {
        public const string AssemblyName = "ScriptAssembly";
        public const string ScriptClassName = "Script";

        private static readonly HashSet<string> ReservedPropertyNames =
            new HashSet<string>(typeof(ScriptBase).GetMembers().Select(x => x.Name), StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Compiles and evaluates a script.
        /// </summary>
        /// <param name="code">The code to compile.</param>
        /// <param name="metadata">
        /// The metadata used to construct the script. Metadata items are exposed a properties with
        /// the name of the key and can be used directly in the script.
        /// </param>
        /// <param name="executionState">The current execution state.</param>
        /// <returns>Raw assembly bytes.</returns>
        public static async Task<object> EvaluateAsync(string code, IMetadata metadata, IExecutionState executionState)
        {
            byte[] rawAssembly = Compile(code, metadata, executionState);
            return await EvaluateAsync(rawAssembly, metadata, executionState);
        }

        /// <summary>
        /// Evaluates a script stored as raw assembly bytes.
        /// </summary>
        /// <remarks>
        /// This loads the assembly, finds the first <see cref="ScriptBase"/> type, instantiates it, and evaluates it.
        /// </remarks>
        /// <param name="rawAssembly">The raw assembly bytes.</param>
        /// <param name="metadata">The metadata that should be used for evaluation.</param>
        /// <param name="executionState">The execution state that should be used for evaluation.</param>
        /// <returns>The result of the script.</returns>
        public static async Task<object> EvaluateAsync(byte[] rawAssembly, IMetadata metadata, IExecutionState executionState)
        {
            Type scriptType = Load(rawAssembly);
            if (scriptType == null)
            {
                throw new ArgumentException("The assembly does not contain a script object", nameof(rawAssembly));
            }
            return await EvaluateAsync(scriptType, metadata, executionState);
        }

        /// <summary>
        /// Evaluates a script stored as raw assembly bytes.
        /// </summary>
        /// <remarks>
        /// This instantiates the script and evaluates it.
        /// </remarks>
        /// <param name="scriptType">A type derived from <see cref="ScriptBase"/>.</param>
        /// <param name="metadata">The metadata that should be used for evaluation.</param>
        /// <param name="executionState">The execution state that should be used for evaluation.</param>
        /// <returns>The result of the script.</returns>
        public static async Task<object> EvaluateAsync(Type scriptType, IMetadata metadata, IExecutionState executionState)
        {
            _ = scriptType ?? throw new ArgumentNullException(nameof(scriptType));
            _ = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _ = executionState ?? throw new ArgumentNullException(nameof(executionState));

            if (!typeof(ScriptBase).IsAssignableFrom(scriptType))
            {
                throw new ArgumentException("Provided type is not a script", nameof(scriptType));
            }

            ScriptBase script = (ScriptBase)Activator.CreateInstance(scriptType, metadata, executionState);
            return await script.EvaluateAsync();
        }

        /// <summary>
        /// Loads a script from a raw script assembly.
        /// </summary>
        /// <remarks>
        /// This loads the assembly and finds the first <see cref="ScriptBase"/> type.
        /// </remarks>
        /// <param name="rawAssembly">The raw assembly bytes.</param>
        /// <returns>The script type or <c>null</c> if a script was not found in the assembly.</returns>
        public static Type Load(byte[] rawAssembly)
        {
            _ = rawAssembly ?? throw new ArgumentNullException(nameof(rawAssembly));
            Assembly assembly = Assembly.Load(rawAssembly);
            return Array.Find(assembly.GetExportedTypes(), t => t.Name == ScriptClassName);
        }

        /// <summary>
        /// Compiles a script into an in-memory script assembly for later evaluation.
        /// </summary>
        /// <param name="code">The code to compile.</param>
        /// <param name="metadata">
        /// The metadata used to construct the script. Metadata items are exposed a properties with
        /// the name of the key and can be used directly in the script.
        /// </param>
        /// <param name="executionState">The current execution state.</param>
        /// <returns>Raw assembly bytes.</returns>
        public static byte[] Compile(string code, IMetadata metadata, IExecutionState executionState) =>
            Compile(code, metadata?.Keys, executionState);

        /// <summary>
        /// Compiles a script into an in-memory script assembly for later evaluation.
        /// </summary>
        /// <param name="code">The code to compile.</param>
        /// <param name="metadataPropertyKeys">
        /// Metadata property keys that will be exposed as properties in the script as
        /// the name of the key and can be used directly in the script.
        /// </param>
        /// <param name="executionState">The current execution state.</param>
        /// <returns>Raw assembly bytes.</returns>
        public static byte[] Compile(string code, IEnumerable<string> metadataPropertyKeys, IExecutionState executionState)
        {
            _ = code ?? throw new ArgumentNullException(nameof(code));
            _ = executionState ?? throw new ArgumentNullException(nameof(executionState));

            // Parse the code
            code = Parse(code, metadataPropertyKeys, executionState);

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
            ILogger logger = executionState.Services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(ScriptBase));
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
                    logger.LogWarning(
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
                    logger.LogError(
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

        private static string GetCompilationErrorMessage(Diagnostic diagnostic)
        {
            string line = diagnostic.Location.IsInSource ? "Line " + (diagnostic.Location.GetMappedLineSpan().Span.Start.Line + 1) : "Metadata";
            return $"{line}: {diagnostic.Id}: {diagnostic.GetMessage()}";
        }

        // Internal for testing
        internal static string Parse(string code, IEnumerable<string> metadataPropertyKeys, IExecutionState executionState)
        {
            // Generate a syntax tree from the code
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code, new CSharpParseOptions(kind: SourceCodeKind.Script), cancellationToken: executionState.CancellationToken);

            // "Lift" class and method declarations
            LiftingWalker liftingWalker = new LiftingWalker();
            liftingWalker.Visit(syntaxTree.GetRoot(executionState.CancellationToken));

            // Get the using statements
            HashSet<string> namespaces = new HashSet<string>(executionState.Namespaces);
            namespaces.Add("System");
            namespaces.Add("System.Collections");
            namespaces.Add("System.Collections.Generic");
            namespaces.Add("System.Linq");
            namespaces.Add("System.Text");
            namespaces.Add("Statiq.Core");
            string usingStatements = string.Join(Environment.NewLine, namespaces.Select(x => "using " + x + ";"));

            // Get the document metadata properties and add to the script host object,
            // but only if they don't conflict with properties from ScriptBase
            string metadataProperties =
                metadataPropertyKeys == null
                    ? string.Empty
                    : string.Join(
                        Environment.NewLine,
                        metadataPropertyKeys
                            .Where(x => !string.IsNullOrWhiteSpace(x) && !ReservedPropertyNames.Contains(x))
                            .Select(x => $"public object {GetValidIdentifier(x)} => Metadata.Get(\"{x}\");"));

            // Determine if we need a return statement
            string preScript = null;
            string postScript = null;
            if (!code.Contains(';'))
            {
                // This is an expression, so add the return before and semicolon after
                preScript = "return";
                postScript = ";";
            }
            else if (!liftingWalker.HasReturnStatement)
            {
                // Includes multiple statements but no return so add one
                postScript = "return null;";
            }

            // Get call signatures for IExecutionState and IMetadata
            string executionStateCalls = string.Join(
                Environment.NewLine,
                ReflectionHelper.GetCallSignatures(typeof(IExecutionState), nameof(ScriptBase.ExecutionState)));
            string metadataCalls = string.Join(
                Environment.NewLine,
                ReflectionHelper.GetCallSignatures(typeof(IMetadata), nameof(ScriptBase.Metadata)));

            // Return the fully parsed script
            return
$@"{usingStatements}
{liftingWalker.UsingDirectives}
public class {ScriptClassName} : ScriptBase, IExecutionState, IMetadata
{{
public {ScriptClassName}(IMetadata metadata, IExecutionState executionState) : base(metadata, executionState) {{ }}
public override async Task<object> EvaluateAsync()
{{
await Task.CompletedTask;
{preScript}
{liftingWalker.ScriptCode}
{postScript}
}}
{liftingWalker.MethodDeclarations}
{metadataProperties}
{executionStateCalls}
{metadataCalls}
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
