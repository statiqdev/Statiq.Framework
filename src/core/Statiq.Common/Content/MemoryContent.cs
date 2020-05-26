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
        private readonly int _index;
        private readonly int _count;

        public MemoryContent(byte[] buffer)
            : this(buffer, 0, buffer?.Length ?? 0, null)
        {
        }

        public MemoryContent(byte[] buffer, int index, int count)
            : this(buffer, index, count, null)
        {
        }

        public MemoryContent(byte[] buffer, string mediaType)
            : this(buffer, 0, buffer?.Length ?? 0, mediaType)
        {
        }

        public MemoryContent(byte[] buffer, int index, int count, string mediaType)
        {
            if (buffer == null && index != 0)
            {
                throw new ArgumentException($"{nameof(index)} cannot be specified for null buffer");
            }
            if (buffer == null && count != 0)
            {
                throw new ArgumentException($"{nameof(count)} cannot be specified for null buffer");
            }
            if (index > buffer.Length)
            {
                throw new ArgumentException($"{nameof(index)} is greater than buffer length");
            }
            if (index + count > buffer.Length)
            {
                throw new ArgumentException($"{nameof(index)} + {nameof(count)} is greater than buffer length");
            }

            _buffer = buffer;
            _index = index;
            _count = count;

            MediaType = mediaType;
        }

        /// <inheritdoc />
        public Stream GetStream() => _buffer == null ? Stream.Null : new MemoryStream(_buffer, _index, _count, false);

        /// <inheritdoc />
        public long Length
        {
            get => _count;
        }

        /// <inheritdoc />
        public string MediaType { get; }

        /// <inheritdoc />
        public IContentProvider CloneWithMediaType(string mediaType) =>
            new MemoryContent(_buffer, mediaType);
    }
}