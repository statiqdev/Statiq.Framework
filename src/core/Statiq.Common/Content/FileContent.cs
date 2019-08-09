using System;
using System.IO;
using System.Threading.Tasks;

namespace Statiq.Common
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

        public Stream GetStream() => _file.OpenRead();
    }
}
