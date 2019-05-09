using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;

namespace Wyam.Core.Modules.IO
{
    /// <summary>
    /// Outputs only those documents that have not yet been written to the file system.
    /// </summary>
    /// <remarks>
    /// The constructors and file resolution logic follows the same semantics as <see cref="WriteFiles"/>.
    /// This module is useful for eliminating documents from the pipeline on subsequent runs depending
    /// on if they've already been written to disk. For example, you might want to put this module
    /// right after <see cref="ReadFiles"/> for a pipeline that does a lot of expensive image processing since
    /// there's no use in processing images that have already been processed. Note that only the
    /// file name is checked and that this module cannot determine if the content would have been
    /// the same had a document not been removed from the pipeline. Also note that <strong>you should only
    /// use this module if you're sure that no other pipelines rely on the output documents</strong>. Because
    /// this module removes documents from the pipeline, those documents will never reach the
    /// end of the pipeline and any other modules or pages that rely on them (for example, an
    /// image directory) will not be correct.
    /// </remarks>
    /// <category>Input/Output</category>
    public class UnwrittenFiles : WriteFiles
    {
        /// <summary>
        /// Uses a delegate to describe where to write the content of each document.
        /// The output of the function should be either a full path to the disk
        /// location (including file name) or a path relative to the output folder.
        /// </summary>
        /// <param name="path">A delegate that returns the desired path.</param>
        public UnwrittenFiles(DocumentConfig<FilePath> path)
            : base(path)
        {
        }

        /// <summary>
        /// Writes the document content to disk with the specified extension with the same
        /// base file name and relative path as the input file. This requires metadata
        /// for <c>RelativeFilePath</c> to be set (which is done by default by the <see cref="ReadFiles"/> module).
        /// </summary>
        /// <param name="extension">The extension to use for writing the file.</param>
        public UnwrittenFiles(string extension)
            : base(extension)
        {
        }

        /// <summary>
        /// Writes the document content to disk with the same file name and relative path
        /// as the input file. This requires metadata for <c>RelativeFilePath</c> to be set
        /// (which is done by default by the <see cref="ReadFiles"/> module).
        /// </summary>
        public UnwrittenFiles()
        {
        }

        /// <inheritdoc />
        public override async Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return
                (await inputs.ParallelSelectAsync(context, SelectUnwrittenFilesAsync))
                .Where(x => x != null);

            async Task<IDocument> SelectUnwrittenFilesAsync(IDocument input)
            {
                if (await ShouldProcessAsync(input, context))
                {
                    FilePath outputPath = await GetOutputPathAsync(input, context);
                    IFile output = outputPath != null ? await context.FileSystem.GetOutputFileAsync(outputPath) : null;
                    if (output != null)
                    {
                        IDirectory outputDirectory = await output.GetDirectoryAsync();
                        bool outputExists = await output.GetExistsAsync();
                        if ((outputDirectory.Path.FullPath != "." && await outputDirectory.GetExistsAsync() && outputExists)
                            || (outputDirectory.Path.FullPath == "." && outputExists))
                        {
                            return null;
                        }
                    }
                }
                return input;
            }
        }
    }
}
