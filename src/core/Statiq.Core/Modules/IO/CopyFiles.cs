using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Copies the content of files from one path on to another path.
    /// </summary>
    /// <remarks>
    /// By default, files are copied from the input folder (or a subfolder) to the same relative
    /// location in the output folder, but this doesn't have to be the case. The output of this module are documents
    /// representing the files copied by the module. Note that the input documents are not output by this module.
    /// </remarks>
    /// <category>Input/Output</category>
    public class CopyFiles : ParallelConfigModule<IEnumerable<string>>
    {
        private Func<IFile, IFile, Task<NormalizedPath>> _destinationPath;
        private Func<IFile, Task<bool>> _predicate = null;

        /// <summary>
        /// Copies all files that match the specified globbing patterns and/or absolute paths. This allows you to specify different
        /// patterns and/or paths depending on the input document.
        /// When this constructor is used, the module is evaluated once for every input document, which may result in copying the same file
        /// more than once (and may also result in IO conflicts since copying is typically done in parallel). It is recommended you only
        /// specify a function-based source path if there will be no overlap between the path returned from each input document.
        /// </summary>
        /// <param name="patterns">A delegate that returns one or more globbing patterns and/or absolute paths.</param>
        public CopyFiles(Config<IEnumerable<string>> patterns)
            : base(patterns, false)
        {
        }

        /// <summary>
        /// Copies all files that match the specified globbing patterns and/or absolute paths. When this constructor is used, the module is
        /// evaluated only once against empty input document. This makes it possible to string multiple CopyFiles modules together in one pipeline.
        /// Keep in mind that the result of the whole pipeline in this case will be documents representing the files copied only by the last CopyFiles
        /// module in the pipeline (since the output documents of the previous CopyFiles modules will have been consumed by the last one).
        /// </summary>
        /// <param name="patterns">The globbing patterns and/or absolute paths to read.</param>
        public CopyFiles(params string[] patterns)
            : base((Config<IEnumerable<string>>)(patterns ?? throw new ArgumentNullException(nameof(patterns))), false)
        {
        }

        /// <summary>
        /// Specifies a predicate that must be satisfied for the file to be copied.
        /// </summary>
        /// <param name="predicate">A predicate that returns <c>true</c> if the file should be copied.</param>
        /// <returns>The current module instance.</returns>
        public CopyFiles Where(Func<IFile, Task<bool>> predicate)
        {
            Func<IFile, Task<bool>> currentPredicate = _predicate;
            _predicate = currentPredicate == null ? predicate : async x => await currentPredicate(x) && await predicate(x);
            return this;
        }

        /// <summary>
        /// Specifies an alternate destination path for each file (by default files are copied to their
        /// same relative path in the output directory). The output of the function should be the full
        /// file path (including file name) of the destination file. If the delegate returns
        /// <c>null</c> for a particular file, that file will not be copied.
        /// </summary>
        /// <param name="destinationPath">A delegate that specifies an alternate destination.
        /// The parameter contains the source <see cref="IFile"/>.</param>
        /// <returns>The current module instance.</returns>
        public CopyFiles To(Func<IFile, Task<NormalizedPath>> destinationPath)
        {
            if (destinationPath == null)
            {
                throw new ArgumentNullException(nameof(destinationPath));
            }

            _destinationPath = async (source, _) => await destinationPath(source);
            return this;
        }

        /// <summary>
        /// Specifies an alternate destination path for each file (by default files are copied to their
        /// same relative path in the output directory). The output of the function should be the full
        /// file path (including file name) of the destination file. If the delegate returns
        /// <c>null</c> for a particular file, that file will not be copied. This overload allows you to
        /// view the <see cref="IFile"/> where the module would normally have copied the file to and then
        /// manipulate it (or not) as appropriate.
        /// </summary>
        /// <param name="destinationPath">A delegate that specifies an alternate destination.
        /// The first parameter contains the source <see cref="IFile"/> and the second contains
        /// an <see cref="IFile"/> representing the calculated destination.</param>
        /// <returns>The current module instance.</returns>
        public CopyFiles To(Func<IFile, IFile, Task<NormalizedPath>> destinationPath)
        {
            _destinationPath = destinationPath ?? throw new ArgumentNullException(nameof(destinationPath));
            return this;
        }

        protected override async Task<IEnumerable<IDocument>> ExecuteConfigAsync(IDocument input, IExecutionContext context, IEnumerable<string> value)
        {
            if (value != null)
            {
                IEnumerable<IFile> inputFiles = context.FileSystem.GetInputFiles(value);
                inputFiles = await inputFiles.ParallelWhereAsync(async x => _predicate == null || await _predicate(x));
                return await inputFiles
                    .ToAsyncEnumerable()
                    .SelectAwait(async file =>
                    {
                        try
                        {
                            // Get the default destination file
                            NormalizedPath relativePath = file.Path.GetRelativeInputPath(context);
                            IFile destination = context.FileSystem.GetOutputFile(relativePath);

                            // Calculate an alternate destination if needed
                            if (_destinationPath != null)
                            {
                                destination = context.FileSystem.GetOutputFile(await _destinationPath(file, destination));
                            }

                            // Copy to the destination
                            await file.CopyToAsync(destination);
                            context.LogDebug("Copied file {0} to {1}", file.Path.FullPath, destination.Path.FullPath);

                            // Return the document
                            return context.CloneOrCreateDocument(input, file.Path, relativePath, file?.GetContentProvider());
                        }
                        catch (Exception ex)
                        {
                            context.LogError($"Error while copying file {file.Path.FullPath}: {ex.Message}");
                            throw;
                        }
                    })
                    .Where(x => x != null)
                    .ToListAsync(context.CancellationToken);
            }
            return null;
        }
    }
}
