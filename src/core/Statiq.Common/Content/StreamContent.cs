using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Statiq.Common.Execution;

namespace Statiq.Common.Content
{
    /// <summary>
    /// A content provider for streams.
    /// </summary>
    public class StreamContent : IContentProvider
    {
        private readonly SemaphoreSlim _mutex = new SemaphoreSlim(1);
        private readonly IMemoryStreamFactory _memoryStreamFactory;
        private readonly Stream _stream;

        /// <summary>
        /// Creates a stream-based content provider.
        /// </summary>
        /// <param name="memoryStreamFactory">A memory stream factory for use if the content stream can't seek and a buffer needs to be created.</param>
        /// <param name="stream">The stream that contains content.</param>
        public StreamContent(IMemoryStreamFactory memoryStreamFactory, Stream stream)
        {
            if (!stream?.CanRead ?? throw new ArgumentNullException(nameof(stream)))
            {
                throw new ArgumentException("Content streams must support reading.", nameof(stream));
            }
            _memoryStreamFactory = memoryStreamFactory ?? throw new ArgumentNullException(nameof(memoryStreamFactory));
            _stream = stream;
        }

        public async Task<Stream> GetStreamAsync()
        {
            await _mutex.WaitAsync();

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
    }
}
