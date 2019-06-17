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
        private readonly Stream _stream;
        private readonly bool _disposeStream;

        /// <summary>
        /// If <paramref name="disposeStream"/> is true (which it is by default), the provided
        /// <see cref="Stream"/> will automatically be disposed when the document is disposed (I.e., the
        /// document takes ownership of the <see cref="Stream"/>).
        /// </summary>
        /// <param name="memoryStreamFactory">A memory stream factory for use if the content stream can't seek and a buffer needs to be created.</param>
        /// <param name="stream">The stream that contains content.</param>
        /// <param name="disposeStream">If <c>true</c>, the provided <see cref="Stream"/> is disposed when no longer used by documents.</param>
        public StreamContent(IMemoryStreamFactory memoryStreamFactory, Stream stream, bool disposeStream = true)
        {
            if (!stream?.CanRead ?? throw new ArgumentNullException(nameof(stream)))
            {
                throw new ArgumentException("Content stream must support reading.", nameof(stream));
            }

            if (stream.CanSeek)
            {
                _stream = stream;
                _disposeStream = disposeStream;
            }
            else
            {
                // If the stream can't seek, wrap it in a buffered stream that can
                MemoryStream bufferStream = memoryStreamFactory.GetStream();
                _stream = new SeekableStream(stream, disposeStream, bufferStream);
                _disposeStream = true;
            }
        }

        public void Dispose()
        {
            if (_disposeStream)
            {
                _stream.Dispose();
            }
        }

        public async Task<Stream> GetStreamAsync()
        {
            await _mutex.WaitAsync();

            _stream.Position = 0;

            // Even if we don't need to synchronize, return a wrapping stream to ensure the underlying
            // stream isn't disposed after every use
            return new SynchronizedStream(_stream, _mutex);
        }
    }
}
