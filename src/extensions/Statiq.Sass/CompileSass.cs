using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpScss;
using Statiq.Common;

namespace Statiq.Sass
{
    /// <summary>
    /// Compiles Sass CSS files to CSS stylesheets.
    /// </summary>
    /// <remarks>
    /// The content of the input document is compiled to CSS and the content of the output document contains the compiled CSS stylesheet.
    /// </remarks>
    /// <category>Templates</category>
    public class CompileSass : ParallelModule
    {
        private readonly List<DirectoryPath> _includePaths = new List<DirectoryPath>();
        private Config<FilePath> _inputPath = Config.FromDocument(DefaultInputPath);
        private Func<string, string> _importPathFunc = null;
        private bool _includeSourceComments = false;
        private ScssOutputStyle _outputStyle = ScssOutputStyle.Compact;
        private bool _generateSourceMap = false;

        /// <summary>
        /// Specifies a delegate that should be used to get the input path for each
        /// input document. This allows the Sass processor to search the right
        /// file system and paths for include files. By default, the <see cref="IDocument.Source"/>
        /// value is used for the input document path.
        /// </summary>
        /// <param name="inputPath">A delegate that should return a <see cref="FilePath"/>.</param>
        /// <returns>The current instance.</returns>
        public CompileSass WithInputPath(Config<FilePath> inputPath)
        {
            _inputPath = inputPath ?? throw new ArgumentNullException(nameof(inputPath));
            return this;
        }

        /// <summary>
        /// Adds a list of paths to search while processing includes.
        /// </summary>
        /// <param name="paths">The paths to include.</param>
        /// <returns>The current instance.</returns>
        public CompileSass WithIncludePaths(params DirectoryPath[] paths)
        {
            _includePaths.AddRange(paths);
            return this;
        }

        /// <summary>
        /// A delegate that processes the path in <c>@import</c> statements.
        /// </summary>
        /// <param name="importPathFunc">A delegate that should return the correct import path for a given import.</param>
        /// <returns>The current instance.</returns>
        public CompileSass WithImportPath(Func<string, string> importPathFunc)
        {
            _importPathFunc = importPathFunc;
            return this;
        }

        /// <summary>
        /// Sets whether the source comments are included (by default they are not).
        /// </summary>
        /// <param name="includeSourceComments"><c>true</c> to include source comments.</param>
        /// <returns>The current instance.</returns>
        public CompileSass IncludeSourceComments(bool includeSourceComments = true)
        {
            _includeSourceComments = includeSourceComments;
            return this;
        }

        /// <summary>
        /// Sets the output style to compact.
        /// </summary>
        /// <returns>The current instance.</returns>
        public CompileSass WithCompactOutputStyle()
        {
            _outputStyle = ScssOutputStyle.Compact;
            return this;
        }

        /// <summary>
        /// Sets the output style to expanded.
        /// </summary>
        /// <returns>The current instance.</returns>
        public CompileSass WithExpandedOutputStyle()
        {
            _outputStyle = ScssOutputStyle.Expanded;
            return this;
        }

        /// <summary>
        /// Sets the output style to compressed.
        /// </summary>
        /// <returns>The current instance.</returns>
        public CompileSass WithCompressedOutputStyle()
        {
            _outputStyle = ScssOutputStyle.Compressed;
            return this;
        }

        /// <summary>
        /// Sets the output style to nested.
        /// </summary>
        /// <returns>The current instance.</returns>
        public CompileSass WithNestedOutputStyle()
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
        public CompileSass GenerateSourceMap(bool generateSourceMap = true)
        {
            _generateSourceMap = generateSourceMap;
            return this;
        }

        protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
        {
            context.LogDebug($"Processing Sass for {input.ToSafeDisplayString()}");

            FilePath inputPath = await _inputPath.GetValueAsync(input, context);
            if (inputPath?.IsAbsolute != true)
            {
                inputPath = context.FileSystem.GetInputFile(new FilePath(Path.GetRandomFileName())).Path;
                context.LogWarning($"No input path found for document {input.ToSafeDisplayString()}, using {inputPath.FileName.FullPath}");
            }

            string content = await input.GetContentStringAsync();

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
            IEnumerable<string> includePaths = _includePaths
                .Where(x => x != null)
                .Select(x => x.IsAbsolute ? x.FullPath : context.FileSystem.GetContainingInputPath(x)?.Combine(x)?.FullPath)
                .Where(x => x != null);
            options.IncludePaths.AddRange(includePaths);
            ScssResult result = Scss.ConvertToCss(content, options);

            // Process the result
            DirectoryPath relativeDirectory = context.FileSystem.GetContainingInputPath(inputPath);
            FilePath relativePath = relativeDirectory?.GetRelativePath(inputPath) ?? inputPath.FileName;

            FilePath cssPath = relativePath.ChangeExtension("css");
            IDocument cssDocument = input.Clone(
                cssPath,
                await context.GetContentProviderAsync(result.Css ?? string.Empty));

            // Generate a source map if requested
            if (_generateSourceMap && result.SourceMap != null)
            {
                FilePath sourceMapPath = relativePath.ChangeExtension("map");
                IDocument sourceMapDocument = input.Clone(
                    sourceMapPath,
                    await context.GetContentProviderAsync(result.SourceMap));
                return new[] { cssDocument, sourceMapDocument };
            }

            return cssDocument.Yield();
        }

        private static FilePath DefaultInputPath(IDocument document, IExecutionContext context)
        {
            if (document.Source != null)
            {
                IFile inputFile = context.FileSystem.GetInputFile(document.Source);
                return inputFile.Exists ? inputFile.Path : null;
            }
            return null;
        }
    }
}
