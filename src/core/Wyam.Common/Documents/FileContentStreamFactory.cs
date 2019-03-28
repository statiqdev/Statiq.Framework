using System;
using System.IO;
using System.Threading.Tasks;
using Wyam.Common.Execution;
using Wyam.Common.IO;

namespace Wyam.Common.Documents
{
    /// <summary>
    /// Provides content streams that are backed by a file in the file system. This
    /// trades performance (disk I/O is considerably slower than memory) for a
    /// reduced memory footprint.
    /// </summary>
    public class FileContentStreamFactory : IContentStreamFactory
    {
        /// <inheritdoc />
        public async Task<Stream> GetStreamAsync(IExecutionContext context, string content = null)
        {
            IFile tempFile = await context.FileSystem.GetTempFileAsync();
            if (!string.IsNullOrEmpty(content))
            {
                await tempFile.WriteAllTextAsync(content);
            }
            return new FileContentStream(tempFile);
        }
    }
}