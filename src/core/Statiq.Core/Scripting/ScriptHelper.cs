using System;
using System.Collections.Concurrent;
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
    internal class ScriptHelper : IScriptHelper
    {
        public const string AssemblyName = "ScriptAssembly";
        public const string ScriptClassName = "Script";
        public const string FactoryClassName = ScriptClassName + "Factory";

        private static readonly HashSet<string> ReservedPropertyNames =
            new HashSet<string>(typeof(ScriptBase).GetMembers().Select(x => x.Name), StringComparer.OrdinalIgnoreCase);

        // Keyed by script
        private static readonly ConcurrentDictionary<string, ScriptFactoryCache> _cachedScriptFactories =
            new ConcurrentDictionary<string, ScriptFactoryCache>();

        private readonly IExecutionState _executionState;

        public ScriptHelper(IExecutionState executionState)
        {
            _executionState = executionState ?? throw new ArgumentNullException(nameof(executionState));
        }

        /// <inheritdoc/>
        public async Task<object> EvaluateAsync(string code, IMetadata metadata)
        {
            ScriptFactoryCache factoryCache = _cachedScriptFactories.GetOrAdd(code, _ => new ScriptFactoryCache(this));
            ScriptBase script = factoryCache.GetScript(
                code,
                metadata,
                IExecutionContext.CurrentOrNull ?? _executionState,
                IExecutionContext.CurrentOrNull);
            return await script.EvaluateAsync();
        }

        /// <summary>
        /// Loads a script factory from a raw script assembly.
        /// </summary>
        /// <remarks>
        /// This loads the assembly and finds the first script factory type.
        /// </remarks>
        /// <param name="rawAssembly">The raw assembly bytes.</param>
        /// <returns>The script factory or <c>null</c> if a script factory was not found in the assembly.</returns>
        internal static Type LoadFactory(byte[] rawAssembly)
        {
            _ = rawAssembly ?? throw new ArgumentNullException(nameof(rawAssembly));
            Assembly assembly = Assembly.Load(rawAssembly);
            return Array.Find(assembly.GetExportedTypes(), t => t.Name == FactoryClassName);
        }

        /// <summary>
        /// Gets property name and scripting types for a given <see cref="IMetadata"/> object
        /// by mapping metadata properties to their defined types and other metadata to <see cref="object"/>.
        /// </summary>
        /// <param name="metadata">The metadata to get scriptable properties for.</param>
        /// <returns>The property name and scriptable type for each property of the metadata.</returns>
        internal static IEnumerable<KeyValuePair<string, string>> GetMetadataProperties(IMetadata metadata)
        {
            if (metadata != null)
            {
                Type metadataType = metadata.GetType();
                foreach (string key in metadata.Keys)
                {
                    yield return new KeyValuePair<string, string>(key, metadataType.GetProperty(key)?.PropertyType.FullName);
                }
            }
        }

        /// <summary>
        /// Compiles a script into an in-memory script assembly for later evaluation.
        /// </summary>
        /// <param name="code">The code to compile.</param>
        /// <param name="metadataPropertyKeys">
        /// Metadata property keys that will be exposed as properties in the script as
        /// the name of the key and can be used directly in the script.
        /// The <see cref="KeyValuePair{TKey, TValue}.Key"/> should be the name of the
        /// property and <see cref="KeyValuePair{TKey, TValue}.Value"/> should be the name
        /// of the property type (or <c>null</c> for "object").
        /// </param>
        /// <returns>Raw assembly bytes.</returns>
        internal byte[] Compile(string code, IEnumerable<KeyValuePair<string, string>> metadataPropertyKeys)
        {
            _ = code ?? throw new ArgumentNullException(nameof(code));

            // Parse the code
            code = Parse(code, metadataPropertyKeys, _executionState);

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

            // Get reference assemblies
            CompilationReferences references = new CompilationReferences();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                references.TryAddReference(assembly);
            }
            foreach (Assembly assembly in _executionState.ClassCatalog.GetAssemblies())
            {
                references.TryAddReference(assembly);
            }
            references.TryAddReference(Assembly.GetEntryAssembly(), true);
            references.TryAddReference(Assembly.GetCallingAssembly(), true);
            references.TryAddReference(Assembly.GetExecutingAssembly(), true);

            // Create the compilation
            string assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
            CSharpCompilation compilation = CSharpCompilation.Create(AssemblyName, new[] { syntaxTree }, references, compilationOptions);

            // For some reason, Roslyn really wants these added by filename
            // See http://stackoverflow.com/questions/23907305/roslyn-has-no-reference-to-system-runtime
            compilation = compilation.AddReferences(
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")));

            // Emit the assembly
            ILogger logger = _executionState.Services.GetRequiredService<ILogger<ScriptHelper>>();
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
        // metadataPropertyKeys.Key = property name, metadataPropertyKeys.Value = property type
        internal static string Parse(string code, IEnumerable<KeyValuePair<string, string>> metadataPropertyKeys, IExecutionState executionState)
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
                            .Where(x => !string.IsNullOrWhiteSpace(x.Key) && !ReservedPropertyNames.Contains(x.Key))
                            .Select(x => $"public {x.Value ?? "object"} {GetValidIdentifier(x.Key)} => Metadata.Get<{x.Value ?? "object"}>(\"{x.Key}\");"));

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
public {ScriptClassName}(IMetadata metadata, IExecutionState executionState, IExecutionContext executionContext)
    : base(metadata, executionState, executionContext) {{ }}
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
public class {FactoryClassName} : ScriptFactoryBase
{{
public override ScriptBase GetScript(IMetadata metadata, IExecutionState executionState, IExecutionContext executionContext)
{{
return new {ScriptClassName}(metadata, executionState, executionContext);
}}
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
