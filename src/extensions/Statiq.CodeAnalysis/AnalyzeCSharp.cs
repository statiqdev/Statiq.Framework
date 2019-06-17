using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Buildalyzer;
using Buildalyzer.Workspaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Statiq.CodeAnalysis.Analysis;
using Statiq.Common;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.IO;
using Statiq.Common.Meta;
using Statiq.Common.Modules;
using Statiq.Common.Tracing;

namespace Statiq.CodeAnalysis
{
    /// <summary>
    /// Performs static code analysis on the input documents, outputting a new document for each symbol.
    /// </summary>
    /// <remarks>
    /// This module acts as the basis for code analysis scenarios such as generating source code documentation.
    /// All input documents are assumed to contain C# source in their content and are used to create a Roslyn
    /// compilation. All symbols (namespaces, types, members, etc.) in the compilation are then recursively
    /// processed and output from this module as documents, one per symbol. The output documents have empty content
    /// and all information about the symbol is contained in the metadata. This lets you pass the output documents
    /// for each symbol on to a template engine like Razor and generate pages for each symbol by having the
    /// template use the document metadata.
    /// </remarks>
    /// <metadata cref="CodeAnalysisKeys.IsResult" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.SymbolId" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Symbol" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Name" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.FullName" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.QualifiedName" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.DisplayName" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Kind" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.SpecificKind" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.ContainingNamespace" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.ContainingAssembly" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.IsStatic" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.IsAbstract" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.IsVirtual" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.IsOverride" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.CommentId" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.MemberTypes" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.MemberNamespaces" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.ContainingType" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.BaseTypes" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.AllInterfaces" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Members" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Operators" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.ExtensionMethods" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.DerivedTypes" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.ImplementingTypes" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Constructors" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.TypeParameters" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Accessibility" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Attributes" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Parameters" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.ReturnType" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.OverriddenMethod" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Type" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.HasConstantValue" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.ConstantValue" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.DeclaringType" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.AttributeData" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.CommentXml" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Example" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Remarks" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Summary" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Returns" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Value" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Exceptions" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Permissions" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Params" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.TypeParams" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.SeeAlso" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Syntax" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.OutputBuildLog" usage="Setting"/>
    /// <category>Metadata</category>
    public class AnalyzeCSharp : IModule
    {
        internal const string CompilationAssemblyName = nameof(CompilationAssemblyName);

        // Use an intermediate Dictionary to initialize with defaults
        private readonly ConcurrentDictionary<string, string> _cssClasses
            = new ConcurrentDictionary<string, string>(
                new Dictionary<string, string>
                {
                    { "table", "table" }
                });

        private readonly List<string> _assemblyGlobs = new List<string>();
        private readonly List<string> _projectGlobs = new List<string>();
        private readonly List<string> _solutionGlobs = new List<string>();

        private Func<ISymbol, bool> _symbolPredicate;
        private Func<ISymbol, FilePath> _destination;
        private DirectoryPath _destinationPrefix = null;
        private bool _docsForImplicitSymbols = false;
        private bool _inputDocuments = true;
        private bool _assemblySymbols = false;
        private bool _implicitInheritDoc = false;

        /// <summary>
        /// This will assume <c>inheritdoc</c> if a symbol has no other code comments.
        /// </summary>
        /// <param name="implicitInheritDoc">If set to <c>true</c>, the symbol will inherit documentation comments
        /// if no other comments are provided.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithImplicitInheritDoc(bool implicitInheritDoc = true)
        {
            _implicitInheritDoc = implicitInheritDoc;
            return this;
        }

        /// <summary>
        /// By default, XML documentation comments are not parsed and rendered for documents that are not part
        /// of the initial result set. This can control that behavior and be used to generate documentation
        /// metadata for all documents, regardless if they were part of the initial result set.
        /// </summary>
        /// <param name="docsForImplicitSymbols">If set to <c>true</c>, documentation metadata is generated for XML comments on all symbols.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithDocsForImplicitSymbols(bool docsForImplicitSymbols = true)
        {
            _docsForImplicitSymbols = docsForImplicitSymbols;
            return this;
        }

        /// <summary>
        /// Controls whether the content of input documents is treated as code and used in the analysis (the default is <c>true</c>).
        /// </summary>
        /// <param name="inputDocuments"><c>true</c> to analyze the content of input documents.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithInputDocuments(bool inputDocuments = true)
        {
            _inputDocuments = inputDocuments;
            return this;
        }

        /// <summary>
        /// Analyzes the specified assemblies.
        /// </summary>
        /// <param name="assemblies">A globbing pattern indicating the assemblies to analyze.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithAssemblies(string assemblies)
        {
            if (!string.IsNullOrEmpty(assemblies))
            {
                _assemblyGlobs.Add(assemblies);
            }
            return this;
        }

        /// <summary>
        /// Analyzes the specified assemblies.
        /// </summary>
        /// <param name="assemblies">Globbing patterns indicating the assemblies to analyze.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithAssemblies(IEnumerable<string> assemblies)
        {
            if (assemblies != null)
            {
                _assemblyGlobs.AddRange(assemblies.Where(x => !string.IsNullOrEmpty(x)));
            }
            return this;
        }

        /// <summary>
        /// Analyzes the specified projects.
        /// </summary>
        /// <param name="projects">A globbing pattern indicating the projects to analyze.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithProjects(string projects)
        {
            if (!string.IsNullOrEmpty(projects))
            {
                _projectGlobs.Add(projects);
            }
            return this;
        }

        /// <summary>
        /// Analyzes the specified projects.
        /// </summary>
        /// <param name="projects">Globbing patterns indicating the projects to analyze.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithProjects(IEnumerable<string> projects)
        {
            if (projects != null)
            {
                _projectGlobs.AddRange(projects.Where(x => !string.IsNullOrEmpty(x)));
            }
            return this;
        }

        /// <summary>
        /// Analyzes the specified solutions.
        /// </summary>
        /// <param name="solutions">A globbing pattern indicating the solutions to analyze.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithSolutions(string solutions)
        {
            if (!string.IsNullOrEmpty(solutions))
            {
                _solutionGlobs.Add(solutions);
            }
            return this;
        }

        /// <summary>
        /// Analyzes the specified solutions.
        /// </summary>
        /// <param name="solutions">Globbing patterns indicating the solutions to analyze.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithSolutions(IEnumerable<string> solutions)
        {
            if (solutions != null)
            {
                _solutionGlobs.AddRange(solutions.Where(x => !string.IsNullOrEmpty(x)));
            }
            return this;
        }

        /// <summary>
        /// Controls which symbols are processed as part of the initial result set.
        /// </summary>
        /// <param name="predicate">A predicate that returns <c>true</c> if the symbol should be included in the initial result set.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WhereSymbol(Func<ISymbol, bool> predicate)
        {
            if (predicate != null)
            {
                Func<ISymbol, bool> currentPredicate = _symbolPredicate;
                _symbolPredicate = currentPredicate == null ? predicate : x => currentPredicate(x) && predicate(x);
            }
            return this;
        }

        /// <summary>
        /// Restricts the initial result set to named type symbols (I.e., classes, interfaces, etc.). Also allows supplying
        /// an additional predicate on the named type.
        /// </summary>
        /// <param name="predicate">A predicate that returns <c>true</c> if the symbol should be included in the initial result set.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithNamedTypes(Func<INamedTypeSymbol, bool> predicate = null) =>
            WhereSymbol(x => x is INamedTypeSymbol namedTypeSymbol && (predicate?.Invoke(namedTypeSymbol) ?? true));

        /// <summary>
        /// Limits symbols in the initial result set to those in the specified namespaces.
        /// </summary>
        /// <param name="includeGlobal">If set to <c>true</c>, symbols in the unnamed global namespace are included.</param>
        /// <param name="namespaces">The namespaces to include symbols from (if <c>namespaces</c> is <c>null</c>, symbols from all
        /// namespaces are included).</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WhereNamespaces(bool includeGlobal, params string[] namespaces)
        {
            return WhereSymbol(x =>
            {
                if (x is IAssemblySymbol)
                {
                    return true;
                }
                if (!(x is INamespaceSymbol namespaceSymbol))
                {
                    return x.ContainingNamespace != null
                           && (namespaces.Length == 0 || namespaces.Any(y => x.ContainingNamespace.ToString().StartsWith(y)));
                }
                if (namespaces.Length == 0)
                {
                    return includeGlobal || !namespaceSymbol.IsGlobalNamespace;
                }
                return (includeGlobal && ((INamespaceSymbol)x).IsGlobalNamespace)
                       || namespaces.Any(y => x.ToString().StartsWith(y));
            });
        }

        /// <summary>
        /// Limits symbols in the initial result set to those in the namespaces that satisfy the specified predicate.
        /// </summary>
        /// <param name="predicate">A predicate that returns true if symbols in the namespace should be included.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WhereNamespaces(Func<string, bool> predicate)
        {
            return WhereSymbol(x =>
            {
                if (x is IAssemblySymbol)
                {
                    return true;
                }
                if (!(x is INamespaceSymbol namespaceSymbol))
                {
                    return x.ContainingNamespace != null && predicate(x.ContainingNamespace.ToString());
                }
                return predicate(namespaceSymbol.ToString());
            });
        }

        /// <summary>
        /// Limits symbols in the initial result set to those that are public (and optionally protected)
        /// </summary>
        /// <param name="includeProtected">If set to <c>true</c>, protected symbols are also included.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WherePublic(bool includeProtected = true)
        {
            return WhereSymbol(x =>
            {
                if (x is IAssemblySymbol)
                {
                    return true;
                }
                return x.DeclaredAccessibility == Accessibility.Public
                    || (includeProtected && x.DeclaredAccessibility == Accessibility.Protected)
                    || x.DeclaredAccessibility == Accessibility.NotApplicable;
            });
        }

        /// <summary>
        /// While converting XML documentation to HTML, any tags with the specified name will get the specified CSS class(s).
        /// This is helpful to style your XML documentation comment rendering to support the stylesheet of your site.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="cssClasses">The CSS classes to set for the specified tag name. Separate multiple CSS classes
        /// with a space (just like you would in HTML).</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithCssClasses(string tagName, string cssClasses)
        {
            if (tagName == null)
            {
                throw new ArgumentNullException(nameof(tagName));
            }
            if (string.IsNullOrWhiteSpace(cssClasses))
            {
                _cssClasses.TryRemove(tagName, out cssClasses);
            }
            else
            {
                _cssClasses[tagName] = cssClasses;
            }
            return this;
        }

        /// <summary>
        /// Controls whether assembly symbol documents are output.
        /// </summary>
        /// <param name="assemblySymbols"><c>true</c> to output assembly symbol documents.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithAssemblySymbols(bool assemblySymbols = true)
        {
            _assemblySymbols = assemblySymbols;
            return WhereSymbol(x => !(x is IAssemblySymbol) || (_assemblySymbols && x.Name != CompilationAssemblyName));
        }

        /// <summary>
        /// This changes the default behavior for the generated destination paths, which is to place files in a path
        /// with the same name as their containing namespace. Namespace documents will be named "index.html" while other type documents
        /// will get a name equal to their SymbolId. Member documents will get the same name as their containing type plus an
        /// anchor to their SymbolId.
        /// </summary>
        /// <param name="destination">
        /// A function that takes the metadata for a given symbol and returns a <c>FilePath</c> to use for the destination.
        /// </param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithDestination(Func<ISymbol, FilePath> destination)
        {
            _destination = destination;
            return this;
        }

        /// <summary>
        /// This lets you add a prefix to the default destination behavior (such as nesting symbol documents inside
        /// a folder like "api/"). Whatever you supply will be combined with the destination. This method has no
        /// effect if you've supplied a custom destination behavior.
        /// </summary>
        /// <param name="destinationPreview">The prefix to use for each generated destination.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithDestinationPrefix(DirectoryPath destinationPreview)
        {
            _destinationPrefix = destinationPreview;
            return this;
        }

        private FilePath DefaultDestination(ISymbol symbol, DirectoryPath prefix)
        {
            INamespaceSymbol containingNamespace = symbol.ContainingNamespace;
            FilePath destinationPath;

            if (symbol.Kind == SymbolKind.Assembly)
            {
                // Assemblies output to the index page in a folder of their name
                destinationPath = new FilePath($"{symbol.GetDisplayName()}/index.html");
            }
            else if (symbol.Kind == SymbolKind.Namespace)
            {
                // Namespaces output to the index page in a folder of their full name
                // If this namespace does not have a containing namespace, it's the global namespace
                destinationPath = new FilePath(containingNamespace == null ? "global/index.html" : $"{symbol.GetDisplayName()}/index.html");
            }
            else if (symbol.Kind == SymbolKind.NamedType)
            {
                // Types output to the index page in a folder of their SymbolId under the folder for their namespace
                destinationPath = new FilePath(containingNamespace.ContainingNamespace == null
                    ? $"global/{symbol.GetId()}/index.html"
                    : $"{containingNamespace.GetDisplayName()}/{symbol.GetId()}/index.html");
            }
            else
            {
                // Members output to a page equal to their SymbolId under the folder for their type
                FilePath containingPath = DefaultDestination(symbol.ContainingType, null);
                destinationPath = containingPath.ChangeFileName($"{symbol.GetId()}.html");
            }

            // Add the prefix
            if (prefix != null)
            {
                destinationPath = prefix.CombineFile(destinationPath);
            }

            return destinationPath;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            // Create the compilation (have to supply an XmlReferenceResolver to handle include XML doc comments)
            MetadataReference mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            Compilation compilation = CSharpCompilation
                .Create(CompilationAssemblyName)
                .WithReferences(mscorlib)
                .WithOptions(new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    xmlReferenceResolver: new XmlFileResolver(context.FileSystem.RootPath.FullPath)));

            // Add the input source and references
            List<ISymbol> symbols = new List<ISymbol>();
            compilation = await AddSourceFilesAsync(inputs, context, compilation);
            compilation = await AddProjectReferencesAsync(context, symbols, compilation);
            compilation = await AddSolutionReferencesAsync(context, symbols, compilation);
            compilation = await AddAssemblyReferencesAsync(context, symbols, compilation);

            // Get and return the document tree
            symbols.Add(compilation.Assembly.GlobalNamespace);
            AnalyzeSymbolVisitor visitor = new AnalyzeSymbolVisitor(
                compilation,
                context,
                _symbolPredicate,
                _destination ?? (x => DefaultDestination(x, _destinationPrefix)),
                _cssClasses,
                _docsForImplicitSymbols,
                _assemblySymbols,
                _implicitInheritDoc);
            Parallel.ForEach(symbols, s => visitor.Visit(s));
            return visitor.Finish();
        }

        private async Task<Compilation> AddSourceFilesAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context, Compilation compilation)
        {
            ConcurrentBag<SyntaxTree> syntaxTrees = new ConcurrentBag<SyntaxTree>();
            if (_inputDocuments)
            {
                // Get syntax trees (supply path so that XML doc includes can be resolved)
                await context.ParallelForEachAsync(inputs, async x => await AddSyntaxTreesAsync(x));
                compilation = compilation.AddSyntaxTrees(syntaxTrees);
            }
            return compilation;

            async Task AddSyntaxTreesAsync(IDocument input)
            {
                using (Stream stream = await input.GetStreamAsync())
                {
                    SourceText sourceText = SourceText.From(stream);
                    syntaxTrees.Add(CSharpSyntaxTree.ParseText(
                        sourceText,
                        path: input.Source?.FullPath ?? string.Empty));
                }
            }
        }

        private async Task<Compilation> AddAssemblyReferencesAsync(IExecutionContext context, List<ISymbol> symbols, Compilation compilation)
        {
            IEnumerable<IFile> assemblyFiles = await context.FileSystem.GetInputFilesAsync(_assemblyGlobs);
            assemblyFiles = await assemblyFiles.WhereAsync(async x => (x.Path.Extension == ".dll" || x.Path.Extension == ".exe") && await x.GetExistsAsync());
            MetadataReference[] assemblyReferences = (await assemblyFiles.SelectAsync(CreateMetadataReferences)).ToArray();
            if (assemblyReferences.Length > 0)
            {
                compilation = compilation.AddReferences(assemblyReferences);
                symbols.AddRange(assemblyReferences
                    .Select(x => (IAssemblySymbol)compilation.GetAssemblyOrModuleSymbol(x))
                    .Select(x => _assemblySymbols ? x : (ISymbol)x.GlobalNamespace));
            }
            return compilation;

            async Task<MetadataReference> CreateMetadataReferences(IFile assemblyFile)
            {
                // Create the metadata reference for the compilation
                IFile xmlFile = await context.FileSystem.GetFileAsync(assemblyFile.Path.ChangeExtension("xml"));
                if (await xmlFile.GetExistsAsync())
                {
                    Trace.Verbose($"Creating metadata reference for assembly {assemblyFile.Path.FullPath} with XML documentation file at {xmlFile.Path.FullPath}");
                    return MetadataReference.CreateFromFile(
                        assemblyFile.Path.FullPath,
                        documentation: XmlDocumentationProvider.CreateFromFile(xmlFile.Path.FullPath));
                }
                Trace.Verbose($"Creating metadata reference for assembly {assemblyFile.Path.FullPath} without XML documentation file");
                return (MetadataReference)MetadataReference.CreateFromFile(assemblyFile.Path.FullPath);
            }
        }

        private async Task<Compilation> AddProjectReferencesAsync(IExecutionContext context, List<ISymbol> symbols, Compilation compilation)
        {
            // Generate a single Workspace and add all of the projects to it
            StringWriter log = new StringWriter();
            AnalyzerManager manager = new AnalyzerManager(new AnalyzerManagerOptions
            {
                LogWriter = log
            });
            AdhocWorkspace workspace = new AdhocWorkspace();
            IEnumerable<IFile> projectFiles = await context.FileSystem.GetInputFilesAsync(_projectGlobs);
            projectFiles = await projectFiles.WhereAsync(async x => x.Path.Extension == ".csproj" && await x.GetExistsAsync());
            List<Project> projects = new List<Project>();
            foreach (IFile projectFile in projectFiles)
            {
                Project project = workspace.CurrentSolution.Projects.FirstOrDefault(x => new FilePath(x.FilePath).Equals(projectFile.Path));
                if (project != null)
                {
                    Trace.Verbose($"Project {projectFile.Path.FullPath} was already in the workspace");
                }
                else
                {
                    Trace.Verbose($"Creating workspace project for {projectFile.Path.FullPath}");
                    ProjectAnalyzer analyzer = manager.GetProject(projectFile.Path.FullPath);
                    if (context.Bool(CodeAnalysisKeys.OutputBuildLog))
                    {
                        analyzer.AddBinaryLogger();
                    }
                    AnalyzerResult result = ReadWorkspace.CompileProjectAndTrace(analyzer, log);
                    if (result != null)
                    {
                        project = result.AddToWorkspace(workspace);
                        if (!project.Documents.Any())
                        {
                            Trace.Warning($"Project at {projectFile.Path.FullPath} contains no documents, which may be an error (check previous log output for any MSBuild warnings)");
                        }
                    }
                }
                projects.Add(project);
            }
            return await AddProjectReferencesAsync(projects, symbols, compilation);
        }

        private async Task<Compilation> AddSolutionReferencesAsync(IExecutionContext context, List<ISymbol> symbols, Compilation compilation)
        {
            IEnumerable<IFile> solutionFiles = await context.FileSystem.GetInputFilesAsync(_solutionGlobs);
            solutionFiles = await solutionFiles.WhereAsync(async x => x.Path.Extension == ".sln" && await x.GetExistsAsync());
            foreach (IFile solutionFile in solutionFiles)
            {
                Trace.Verbose($"Creating workspace solution for {solutionFile.Path.FullPath}");
                StringWriter log = new StringWriter();
                AnalyzerManager manager = new AnalyzerManager(
                    solutionFile.Path.FullPath,
                    new AnalyzerManagerOptions
                    {
                        LogWriter = log
                    });

                AnalyzerResult[] results = manager.Projects.Values
                    .Select(analyzer =>
                    {
                        if (context.Bool(CodeAnalysisKeys.OutputBuildLog))
                        {
                            analyzer.AddBinaryLogger();
                        }
                        return ReadWorkspace.CompileProjectAndTrace(analyzer, log);
                    })
                    .Where(x => x != null)
                    .ToArray();

                AdhocWorkspace workspace = new AdhocWorkspace();
                foreach (AnalyzerResult result in results)
                {
                    result.AddToWorkspace(workspace);
                }

                compilation = await AddProjectReferencesAsync(workspace.CurrentSolution.Projects, symbols, compilation);
            }
            return compilation;
        }

        private async Task<Compilation> AddProjectReferencesAsync(IEnumerable<Project> projects, List<ISymbol> symbols, Compilation compilation)
        {
            // Add a references to the compilation for each project in the solution
            MetadataReference[] compilationReferences = (await projects
                .Where(x => x.SupportsCompilation)
                .SelectAsync(async x =>
                {
                    Trace.Verbose($"Creating compilation reference for project {x.Name}");
                    Compilation projectCompilation = await x.GetCompilationAsync();
                    return projectCompilation.ToMetadataReference(new[] { x.AssemblyName }.ToImmutableArray());
                }))
                .ToArray();
            if (compilationReferences.Length > 0)
            {
                compilation = compilation.AddReferences(compilationReferences);
                symbols.AddRange(compilationReferences
                    .Select(x => (IAssemblySymbol)compilation.GetAssemblyOrModuleSymbol(x))
                    .Select(x => _assemblySymbols ? x : (ISymbol)x.GlobalNamespace));
            }
            return compilation;
        }
    }
}