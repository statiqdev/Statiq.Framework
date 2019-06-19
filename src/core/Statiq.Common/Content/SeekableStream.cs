using System;
using System.IO;

namespace Statiq.Common.Content
{
    internal class SeekableStream : Stream
    {
        private readonly Stream _stream;
        private readonly MemoryStream _bufferStream;
        private bool _endOfStream = false;

        public SeekableStream(Stream stream, MemoryStream bufferStream)
        {
            if (!stream?.CanRead ?? throw new ArgumentNullException(nameof(stream)))
            {
                throw new ArgumentException("Wrapped stream must be readable.");
            }
            _stream = stream;
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
                if (!_endOfStream)
                {
                    Fill();
                }
                return _bufferStream.Length;
            }
        }

        public override long Position
        {
            get => _bufferStream.Position;
            set => Seek(value, SeekOrigin.Begin);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
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
    }
}