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
        /// <inheritdoc />
        public override async Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return
                (await inputs.ParallelSelectAsync(context, SelectUnwrittenFilesAsync))
                .Where(x => x != null);

            async Task<IDocument> SelectUnwrittenFilesAsync(IDocument input)
            {
                if (await ShouldProcessAsync(input, context) && input.Destination != null)
                {
                    IFile output = await context.FileSystem.GetOutputFileAsync(input.Destination);
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
