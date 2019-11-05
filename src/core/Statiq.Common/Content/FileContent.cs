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
            : this(file, file?.MediaType)
        {
        }

        public FileContent(IFile file, string mediaType)
        {
            _file = file ?? throw new ArgumentException();
            MediaType = mediaType;
        }

        public Stream GetStream() => _file.OpenRead();

        public string MediaType { get; }
    }
}
