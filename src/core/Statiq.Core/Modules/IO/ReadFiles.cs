using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;
using Statiq.Common.Configuration;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.IO;
using Statiq.Common.Meta;
using Statiq.Common.Modules;
using Statiq.Common.Tracing;
using Statiq.Core.Modules.Control;

namespace Statiq.Core.Modules.IO
{
    /// <summary>
    /// Reads the content of files from the file system into the content of new documents.
    /// </summary>
    /// <remarks>
    /// This module will be executed once and input documents will be ignored if search patterns are specified. Otherwise, if a delegate
    /// is specified, the module will be executed once per input document and the resulting output documents will be
    /// aggregated. In either case, the input documents will not be returned as output of this module. If you want to add
    /// additional files to a current pipeline, you should enclose your ReadFiles modules with <see cref="ExecuteModules"/>.
    /// <see cref="IDocument.Source"/> will be set to the absolute path of the file
    /// (use <see cref="FilePath.GetRelativeInputPath(IExecutionContext)"/> to get a source path relative to the input folders).
    /// <see cref="IDocument.Destination"/> will be set to the relative path of the file (so that <see cref="WriteFiles"/> will write it
    /// to the same relative path in the output folder).
    /// </remarks>
    /// <category>Input/Output</category>
    public class ReadFiles : IModule
    {
        private readonly DocumentConfig<IEnumerable<string>> _patterns;
        private Func<IFile, Task<bool>> _predicate = null;

        /// <summary>
        /// Reads all files that match the specified globbing patterns and/or absolute paths. This allows you to
        /// specify different patterns and/or paths depending on the input.
        /// </summary>
        /// <param name="patterns">A delegate that returns one or more globbing patterns and/or absolute paths.</param>
        public ReadFiles(DocumentConfig<IEnumerable<string>> patterns)
        {
            _patterns = patterns ?? throw new ArgumentNullException(nameof(patterns));
        }

        /// <summary>
        /// Reads all files that match the specified globbing patterns and/or absolute paths.
        /// </summary>
        /// <param name="patterns">The globbing patterns and/or absolute paths to read.</param>
        public ReadFiles(params string[] patterns)
            : this((DocumentConfig<IEnumerable<string>>)(patterns ?? throw new ArgumentNullException(nameof(patterns))))
        {
        }

        /// <summary>
        /// Specifies a predicate that must be satisfied for the file to be read.
        /// </summary>
        /// <param name="predicate">A predicate that returns <c>true</c> if the file should be read.</param>
        /// <returns>The current module instance.</returns>
        public ReadFiles Where(Func<IFile, Task<bool>> predicate)
        {
            Func<IFile, Task<bool>> currentPredicate = _predicate;
            _predicate = currentPredicate == null ? predicate : async x => await currentPredicate(x) && await predicate(x);
            return this;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return _patterns.RequiresDocument
                ? await inputs.ParallelSelectManyAsync(context, async input =>
                    await ExecuteAsync(input, await _patterns.GetValueAsync(input, context), context))
                : await ExecuteAsync(null, await _patterns.GetValueAsync(null, context), context);
        }

        private async Task<IEnumerable<IDocument>> ExecuteAsync(IDocument input, IEnumerable<string> patterns, IExecutionContext context)
        {
            if (patterns != null)
            {
                IEnumerable<IFile> files = await context.FileSystem.GetInputFilesAsync(patterns);
                files = await files.ParallelWhereAsync(async file => _predicate == null || await _predicate(file));
                return files.AsParallel().Select(file =>
                {
                    Trace.Verbose($"Read file {file.Path.FullPath}");
                    return input == null
                        ? context.CreateDocument(file.Path, file.Path.GetRelativeInputPath(context), context.GetContentProvider(file))
                        : input.Clone(file.Path, file.Path.GetRelativeInputPath(context), context.GetContentProvider(file));
                });
            }
            return Array.Empty<IDocument>();
        }
    }
}
