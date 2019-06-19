using System;
using System.IO;
using System.Threading.Tasks;
using Statiq.Common.IO;

namespace Statiq.Common.Content
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

        public Task<Stream> GetStreamAsync() => _file.OpenReadAsync();
    }
}
