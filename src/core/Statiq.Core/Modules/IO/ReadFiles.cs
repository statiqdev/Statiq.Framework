using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Reads the content of files from the file system into the content of new documents.
    /// </summary>
    /// <remarks>
    /// This module will be executed once and input documents will be ignored if search patterns are specified. Otherwise, if a delegate
    /// is specified, the module will be executed once per input document and the resulting output documents will be
    /// aggregated. In either case, the input documents will not be returned as output of this module.
    /// <see cref="IDocument.Source"/> will be set to the absolute path of the file
    /// (use <see cref="FilePath.GetRelativeInputPath(IExecutionContext)"/> to get a source path relative to the input folders).
    /// <see cref="IDocument.Destination"/> will be set to the relative path of the file (so that <see cref="WriteFiles"/> will write it
    /// to the same relative path in the output folder).
    /// </remarks>
    /// <category>Input/Output</category>
    public class ReadFiles : ConfigModule<IEnumerable<string>>, IParallelModule
    {
        private Func<IFile, Task<bool>> _predicate = null;

        /// <summary>
        /// Reads all files that match the specified globbing patterns and/or absolute paths. This allows you to
        /// specify different patterns and/or paths depending on the input.
        /// </summary>
        /// <param name="patterns">A delegate that returns one or more globbing patterns and/or absolute paths.</param>
        public ReadFiles(Config<IEnumerable<string>> patterns)
            : base(patterns, false)
        {
        }

        /// <summary>
        /// Reads all files that match the specified globbing patterns and/or absolute paths.
        /// </summary>
        /// <param name="patterns">The globbing patterns and/or absolute paths to read.</param>
        public ReadFiles(params string[] patterns)
            : base((Config<IEnumerable<string>>)(patterns ?? throw new ArgumentNullException(nameof(patterns))), false)
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

        protected override async Task<IEnumerable<IDocument>> ExecuteAsync(IDocument input, IExecutionContext context, IEnumerable<string> value)
        {
            if (value != null)
            {
                IEnumerable<IFile> files = await context.FileSystem.GetInputFilesAsync(value);
                files = await files.ParallelWhereAsync(async file => _predicate == null || await _predicate(file));
                return files.AsParallel(context).Select(file =>
                {
                    Trace.Verbose($"Read file {file.Path.FullPath}");
                    return context.CloneOrCreateDocument(input, file.Path, file.Path.GetRelativeInputPath(context), context.GetContentProvider(file));
                });
            }
            return null;
        }
    }
}
