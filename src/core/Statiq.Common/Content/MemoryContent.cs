using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A content provider for raw memory.
    /// </summary>
    public class MemoryContent : IContentProvider
    {
        private readonly byte[] _buffer;

        public MemoryContent(byte[] buffer)
            : this(buffer, null)
        {
        }

        public MemoryContent(byte[] buffer, string mediaType)
        {
            _buffer = buffer;
            MediaType = mediaType;
        }

        /// <inheritdoc />
        public Stream GetStream() => _buffer == null ? Stream.Null : new MemoryStream(_buffer, false);

        /// <inheritdoc />
        public long Length
        {
            get => _buffer?.Length ?? 0;
        }

        /// <inheritdoc />
        public string MediaType { get; }

        /// <inheritdoc />
        public IContentProvider CloneWithMediaType(string mediaType) =>
            new MemoryContent(_buffer, mediaType);
    }
}