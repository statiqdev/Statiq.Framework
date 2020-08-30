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
        public FileContent(IFile file)
            : this(file, file?.MediaType)
        {
        }

        public FileContent(IFile file, string mediaType)
        {
            File = file ?? throw new ArgumentException();
            MediaType = mediaType;
        }

        /// <summary>
        /// The file that this content comes from.
        /// </summary>
        public IFile File { get; }

        /// <inheritdoc />
        public Stream GetStream() => File.OpenRead();

        /// <inheritdoc />
        public long Length => File.Length;

        /// <inheritdoc />
        public string MediaType { get; }

        /// <inheritdoc />
        public IContentProvider CloneWithMediaType(string mediaType) =>
            string.Equals(MediaType, mediaType, StringComparison.OrdinalIgnoreCase) ? this : new FileContent(File, mediaType);
    }
}
