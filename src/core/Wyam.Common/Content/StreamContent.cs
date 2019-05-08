using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Wyam.Common.Execution;

namespace Wyam.Common.Content
{
    /// <summary>
    /// A content provider for streams.
    /// </summary>
    public class StreamContent : IContentProvider
    {
        private readonly Stream _stream;
        private readonly bool _disposeStream;
        private readonly SemaphoreSlim _mutex;

        /// <summary>
        /// If <paramref name="disposeStream"/> is true (which it is by default), the provided
        /// <see cref="Stream"/> will automatically be disposed when the document is disposed (I.e., the
        /// document takes ownership of the <see cref="Stream"/>).
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="stream">The stream that contains content.</param>
        /// <param name="disposeStream">If <c>true</c>, the provided <see cref="Stream"/> is disposed when no longer used by documents.</param>
        /// <param name="synchronized">If <c>true</c>, access to the provided stream will be synchronized so that only one caller can access it at a time.</param>
        public StreamContent(IExecutionContext context, Stream stream, bool disposeStream = true, bool synchronized = true)
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
                MemoryStream bufferStream = context.MemoryStreamManager.GetStream();
                _stream = new SeekableStream(stream, disposeStream, bufferStream);
                _disposeStream = true;
            }

            _mutex = synchronized ? new SemaphoreSlim(1) : null;
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
            if (_mutex != null)
            {
                await _mutex.WaitAsync();
            }

            _stream.Position = 0;

            // Even if we don't need to synchronize, return a wrapping stream to ensure the underlying
            // stream isn't disposed after every use
            return new SynchronizedStream(_stream, _mutex);
        }
    }
}
