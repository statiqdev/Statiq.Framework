using System;
using System.IO;
using System.Threading.Tasks;
using Wyam.Common.IO;

namespace Wyam.Common.Content
{
    /// <summary>
    /// A content provider for files.
    /// </summary>
    public class FileContent : IContentProvider
    {
        private readonly IFile _file;

        public FileContent(IFile file)
        {
            _file = file ?? throw new ArgumentException();
        }

        public void Dispose()
        {
            // Nothing to do
        }

        public Task<Stream> GetStreamAsync() => _file.OpenReadAsync();
    }
}
