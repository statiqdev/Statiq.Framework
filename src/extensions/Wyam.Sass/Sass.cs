using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SharpScss;
using Wyam.Common;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Tracing;

namespace Wyam.Sass
{
    /// <summary>
    /// Compiles Sass CSS files to CSS stylesheets.
    /// </summary>
    /// <remarks>
    /// The content of the input document is compiled to CSS and the content of the output document contains the compiled CSS stylesheet.
    /// </remarks>
    /// <example>
    /// This is a pipeline that compiles two Sass CSS files, one for Bootstrap (which contains a lot of includes) and a second for custom CSS.
    /// <code>
    /// Pipelines.Add("Sass",
    ///     ReadFiles("master.scss"),
    ///     Concat(ReadFiles("foundation.scss")),
    ///     Sass().WithCompactOutputStyle(),
    ///     WriteFiles(".css")
    /// );
    /// </code>
    /// Another common pattern is building Bootstrap from npm sitting alongside your "input" folder in a "node_modules" folder. This can be accomplished with
    /// a pipeline that looks similar to the following. It loads the Bootstrap Sass files that don't begin with "_" from the Bootstrap node module and then
    /// outputs the results to a specific path under your output folder (in this case, "assets/css/bootstrap.css").
    /// <code>
    /// Pipelines.Add("Bootstrap",
    ///     ReadFiles("../node_modules/bootstrap/scss/**/{!_,}*.scss"),
    ///     Sass()
    ///         .WithCompactOutputStyle(),
    ///     WriteFiles((doc, ctx) => $"assets/css/{doc.String(Keys.RelativeFilePath)}")
    ///         .UseWriteMetadata(false)
    /// );
    /// </code>
    /// </example>
    /// <metadata cref="Keys.SourceFilePath" usage="Input">The default key to use for determining the input document path.</metadata>
    /// <metadata cref="Keys.RelativeFilePath" usage="Input">If <see cref="Keys.SourceFilePath"/> is unavailable, this is used to guess at the source file path.</metadata>
    /// <metadata cref="Keys.RelativeFilePath" usage="Output">Relative path to the output CSS (or map) file.</metadata>
    /// <metadata cref="Keys.WritePath" usage="Output" />
    /// <category>Templates</category>
    public class Sass : IModule
    {
        private readonly List<DirectoryPath> _includePaths = new List<DirectoryPath>();
        private DocumentConfig<FilePath> _inputPath = Config.FromDocument(DefaultInputPathAsync);
        private Func<string, string> _importPathFunc = null;
        private bool _includeSourceComments = false;
        private ScssOutputStyle _outputStyle = ScssOutputStyle.Compact;
        private bool _generateSourceMap = false;

        /// <summary>
        /// Specifies a delegate that should be used to get the input path for each
        /// input document. This allows the Sass processor to search the right
        /// file system and paths for include files. By default, the <see cref="Keys.RelativeFilePath"/>
        /// metadata value is used for the input document path.
        /// </summary>
        /// <param name="inputPath">A delegate that should return a <see cref="FilePath"/>.</param>
        /// <returns>The current instance.</returns>
        public Sass WithInputPath(DocumentConfig<FilePath> inputPath)
        {
            _inputPath = inputPath ?? throw new ArgumentNullException(nameof(inputPath));
            return this;
        }

        /// <summary>
        /// Adds a list of paths to search while processing includes.
        /// </summary>
        /// <param name="paths">The paths to include.</param>
        /// <returns>The current instance.</returns>
        public Sass WithIncludePaths(params DirectoryPath[] paths)
        {
            _includePaths.AddRange(paths);
            return this;
        }

        /// <summary>
        /// A delegate that processes the path in <c>@import</c> statements.
        /// </summary>
        /// <param name="importPathFunc">A delegate that should return the correct import path for a given import.</param>
        /// <returns>The current instance.</returns>
        public Sass WithImportPath(Func<string, string> importPathFunc)
        {
            _importPathFunc = importPathFunc;
            return this;
        }

        /// <summary>
        /// Sets whether the source comments are included (by default they are not).
        /// </summary>
        /// <param name="includeSourceComments"><c>true</c> to include source comments.</param>
        /// <returns>The current instance.</returns>
        public Sass IncludeSourceComments(bool includeSourceComments = true)
        {
            _includeSourceComments = includeSourceComments;
            return this;
        }

        /// <summary>
        /// Sets the output style to compact.
        /// </summary>
        /// <returns>The current instance.</returns>
        public Sass WithCompactOutputStyle()
        {
            _outputStyle = ScssOutputStyle.Compact;
            return this;
        }

        /// <summary>
        /// Sets the output style to expanded.
        /// </summary>
        /// <returns>The current instance.</returns>
        public Sass WithExpandedOutputStyle()
        {
            _outputStyle = ScssOutputStyle.Expanded;
            return this;
        }

        /// <summary>
        /// Sets the output style to compressed.
        /// </summary>
        /// <returns>The current instance.</returns>
        public Sass WithCompressedOutputStyle()
        {
            _outputStyle = ScssOutputStyle.Compressed;
            return this;
        }

        /// <summary>
        /// Sets the output style to nested.
        /// </summary>
        /// <returns>The current instance.</returns>
        public Sass WithNestedOutputStyle()
        {
            _outputStyle = ScssOutputStyle.Nested;
            return this;
        }

        /// <summary>
        /// Specifies whether a source map should be generated (the default
        /// behavior is <c>false</c>).
        /// </summary>
        /// <param name="generateSourceMap"><c>true</c> to generate a source map.</param>
        /// <returns>The current instance.</returns>
        public Sass GenerateSourceMap(bool generateSourceMap = true)
        {
            _generateSourceMap = generateSourceMap;
            return this;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return await inputs.ParallelSelectManyAsync(context, ProcessSassAsync);

            async Task<IEnumerable<IDocument>> ProcessSassAsync(IDocument input)
            {
                Trace.Verbose($"Processing Sass for {input.SourceString()}");

                FilePath inputPath = await _inputPath.GetValueAsync(input, context);
                if (inputPath?.IsAbsolute != true)
                {
                    inputPath = (await context.FileSystem.GetInputFileAsync(new FilePath(Path.GetRandomFileName()))).Path;
                    Trace.Warning($"No input path found for document {input.SourceString()}, using {inputPath.FileName.FullPath}");
                }

                string content = await input.GetStringAsync();

                // Sass conversion
                FileImporter importer = new FileImporter(context.FileSystem, _importPathFunc);
                ScssOptions options = new ScssOptions
                {
                    OutputStyle = _outputStyle,
                    GenerateSourceMap = _generateSourceMap,
                    SourceComments = _includeSourceComments,
                    InputFile = inputPath.FullPath,
                    TryImport = importer.TryImport
                };
                IEnumerable<string> includePaths = await _includePaths
                    .Where(x => x != null)
                    .SelectAsync(async x => x.IsAbsolute ? x.FullPath : (await context.FileSystem.GetContainingInputPathAsync(x))?.Combine(x)?.FullPath);
                options.IncludePaths.AddRange(includePaths.Where(x => x != null));
                ScssResult result = Scss.ConvertToCss(content, options);

                // Process the result
                DirectoryPath relativeDirectory = await context.FileSystem.GetContainingInputPathAsync(inputPath);
                FilePath relativePath = relativeDirectory?.GetRelativePath(inputPath) ?? inputPath.FileName;

                FilePath cssPath = relativePath.ChangeExtension("css");
                IDocument cssDocument = context.GetDocument(
                    input,
                    new MetadataItems
                    {
                            { Keys.RelativeFilePath, cssPath },
                            { Keys.WritePath, cssPath }
                    },
                    await context.GetContentProviderAsync(result.Css ?? string.Empty));

                // Generate a source map if requested
                if (_generateSourceMap && result.SourceMap != null)
                {
                    FilePath sourceMapPath = relativePath.ChangeExtension("map");
                    IDocument sourceMapDocument = context.GetDocument(
                        input,
                        new MetadataItems
                        {
                                { Keys.RelativeFilePath, sourceMapPath },
                                { Keys.WritePath, sourceMapPath }
                        },
                        await context.GetContentProviderAsync(result.SourceMap));
                    return new[] { cssDocument, sourceMapDocument };
                }

                return new[] { cssDocument };
            }
        }

        private static async Task<FilePath> DefaultInputPathAsync(IDocument document, IExecutionContext context)
        {
            FilePath path = document.FilePath(Keys.SourceFilePath);
            if (path == null)
            {
                path = document.FilePath(Keys.RelativeFilePath);
                if (path != null)
                {
                    IFile inputFile = await context.FileSystem.GetInputFileAsync(path);
                    return await inputFile.GetExistsAsync() ? inputFile.Path : null;
                }
            }
            return path;
        }
    }
}
