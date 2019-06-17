using System;
using System.IO;

namespace Statiq.Common.Content
{
    internal class SeekableStream : Stream
    {
        private readonly Stream _stream;
        private readonly bool _disposeStream;
        private readonly MemoryStream _bufferStream;
        private bool _endOfStream = false;
        private bool _disposed = false;

        public SeekableStream(Stream stream, bool disposeStream, MemoryStream bufferStream)
        {
            if (!stream?.CanRead ?? throw new ArgumentNullException(nameof(stream)))
            {
                throw new ArgumentException("Wrapped stream must be readable.");
            }
            _stream = stream;
            _disposeStream = disposeStream;
            _bufferStream = bufferStream ?? throw new ArgumentNullException(nameof(bufferStream));
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;

        public override void Flush()
        {
            _stream.Flush();
        }

        public override long Length
        {
            get
            {
                CheckDisposed();
                if (!_endOfStream)
                {
                    Fill();
                }
                return _bufferStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                CheckDisposed();
                return _bufferStream.Position;
            }
            set
            {
                CheckDisposed();
                Seek(value, SeekOrigin.Begin);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            CheckDisposed();
            int streamBytes = 0;
            int memoryBytes = _bufferStream.Read(buffer, offset, count);
            if ((count > memoryBytes) && (!_endOfStream))
            {
                int read = _stream.Read(buffer, offset + memoryBytes + streamBytes, count - memoryBytes - streamBytes);
                streamBytes += read;
                if (read == 0)
                {
                    _endOfStream = true;
                }
                _bufferStream.Write(buffer, offset + memoryBytes, streamBytes);
            }
            return memoryBytes + streamBytes;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            CheckDisposed();
            long newPosition;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    newPosition = offset;
                    break;
                case SeekOrigin.Current:
                    newPosition = _bufferStream.Position + offset;
                    break;
                case SeekOrigin.End:
                    if (!_endOfStream)
                    {
                        Fill();
                    }
                    newPosition = _bufferStream.Length + offset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(origin));
            }

            // Read additional bytes from the underlying stream if seeking past the end of the buffer
            if ((newPosition > _bufferStream.Length) && (!_endOfStream))
            {
                _bufferStream.Position = _bufferStream.Length;
                int bytesToRead = (int)(newPosition - _bufferStream.Length);
                byte[] buffer = new byte[1024];
                do
                {
                    bytesToRead -= Read(buffer, 0, (bytesToRead >= buffer.Length) ? buffer.Length : bytesToRead);
                }
                while ((bytesToRead > 0) && (!_endOfStream));
            }
            _bufferStream.Position = (newPosition <= _bufferStream.Length) ? newPosition : _bufferStream.Length;
            return 0;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        private void Fill()
        {
            if (_endOfStream)
            {
                return;
            }

            _bufferStream.Position = _bufferStream.Length;
            byte[] buffer = new byte[1024];
            int bytesRead;
            do
            {
                bytesRead = _stream.Read(buffer, 0, buffer.Length);
                _bufferStream.Write(buffer, 0, bytesRead);
            }
            while (bytesRead != 0);
            _endOfStream = true;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_disposed || !disposing)
            {
                return;
            }

            _bufferStream.Dispose();
            if (_disposeStream)
            {
                _stream.Dispose();
            }
            _disposed = true;
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(SeekableStream));
            }
        }
    }
}