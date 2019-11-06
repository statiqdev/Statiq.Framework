using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A content provider for streams.
    /// </summary>
    public class StreamContent : IContentProvider
    {
        private readonly SemaphoreSlim _mutex;
        private readonly IMemoryStreamFactory _memoryStreamFactory;
        private readonly Stream _stream;

        /// <summary>
        /// Creates a stream-based content provider.
        /// </summary>
        /// <param name="memoryStreamFactory">A memory stream factory for use if the content stream can't seek and a buffer needs to be created.</param>
        /// <param name="stream">The stream that contains content.</param>
        public StreamContent(IMemoryStreamFactory memoryStreamFactory, Stream stream)
            : this(memoryStreamFactory, stream, null)
        {
        }

        /// <summary>
        /// Creates a stream-based content provider.
        /// </summary>
        /// <param name="memoryStreamFactory">A memory stream factory for use if the content stream can't seek and a buffer needs to be created.</param>
        /// <param name="stream">The stream that contains content.</param>
        /// <param name="mediaType">The media type of the content.</param>
        public StreamContent(IMemoryStreamFactory memoryStreamFactory, Stream stream, string mediaType)
        {
            if (!stream?.CanRead ?? throw new ArgumentNullException(nameof(stream)))
            {
                throw new ArgumentException("Content streams must support reading.", nameof(stream));
            }
            _mutex = new SemaphoreSlim(1);
            _memoryStreamFactory = memoryStreamFactory ?? throw new ArgumentNullException(nameof(memoryStreamFactory));
            _stream = stream;
            MediaType = mediaType;
        }

        // Used for cloning the content provider with the same stream and locking mutex
        private StreamContent(SemaphoreSlim mutex, IMemoryStreamFactory memoryStreamFactory, Stream stream, string mediaType)
        {
            _mutex = mutex;
            _memoryStreamFactory = memoryStreamFactory;
            _stream = stream;
            MediaType = mediaType;
        }

        /// <inheritdoc />
        public Stream GetStream()
        {
            _mutex.Wait();

            // Make sure we have a seekable stream
            Stream contentStream = _stream;
            if (!contentStream.CanSeek)
            {
                // If the stream can't seek, wrap it in a buffered stream that can
                MemoryStream bufferStream = _memoryStreamFactory.GetStream();
                contentStream = new SeekableStream(_stream, bufferStream);
            }

            contentStream.Position = 0;

            // Even if we don't need to synchronize, return a wrapping stream to ensure the underlying
            // stream isn't disposed after every use
            return new SynchronizedStream(contentStream, _mutex);
        }

        /// <inheritdoc />
        public long Length
        {
            get
            {
                _mutex.Wait();
                long length = _stream.Length;
                _mutex.Release();
                return length;
            }
        }

        /// <inheritdoc />
        public string MediaType { get; }

        /// <inheritdoc />
        public IContentProvider CloneWithMediaType(string mediaType) =>
            new StreamContent(_mutex, _memoryStreamFactory, _stream, mediaType);
    }
}
