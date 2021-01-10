using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A content provider that wraps a seekable stream.
    /// </summary>
    public class StreamContent : IContentProvider
    {
        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(true);
        private readonly Stream _stream;

        public StreamContent(Stream stream)
            : this(stream, null)
        {
        }

        public StreamContent(Stream stream, string mediaType)
        {
            _stream = stream;
            if (_stream is object && !_stream.CanSeek)
            {
                throw new ArgumentException("Stream should be seekable", nameof(stream));
            }
            MediaType = mediaType;
        }

        /// <inheritdoc />
        public Stream GetStream()
        {
            if (_stream is null)
            {
                return Stream.Null;
            }
            _resetEvent.WaitOne();
            _stream.Position = 0; // Reset the position for each read
            return new SignalingStream(_stream, _resetEvent);
        }

        /// <inheritdoc />
        public TextReader GetTextReader() => _stream is null ? TextReader.Null : new StreamReader(GetStream());

        /// <inheritdoc />
        public long GetLength() => _stream?.Length ?? 0;

        /// <inheritdoc />
        public string MediaType { get; }

        /// <inheritdoc />
        public IContentProvider CloneWithMediaType(string mediaType) =>
            new StreamContent(_stream, mediaType);

        /// <inheritdoc />
        public async Task<int> GetCacheHashCodeAsync()
        {
            // The stream might have changed so we can't cache the hash code, get it fresh each time
            using (Stream stream = GetStream())
            {
                return (int)await Crc32.CalculateAsync(stream);
            }
        }
    }
}