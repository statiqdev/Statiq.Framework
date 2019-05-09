using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Wyam.Common.Util
{
    public abstract class DelegatingStream : Stream
    {
        protected Stream Stream { get; }

        protected bool Disposed { get; private set; }

        protected DelegatingStream(Stream stream)
        {
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        protected override void Dispose(bool disposing)
        {
            Disposed = true;
        }

        protected virtual void CheckDisposed()
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            CheckDisposed();
            await Stream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override void Flush()
        {
            CheckDisposed();
            Stream.Flush();
        }

        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            CheckDisposed();
            await Stream.FlushAsync(cancellationToken);
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            CheckDisposed();
            return Stream.BeginRead(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            CheckDisposed();
            return Stream.EndRead(asyncResult);
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            CheckDisposed();
            return await Stream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            CheckDisposed();
            return Stream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            CheckDisposed();
            Stream.EndWrite(asyncResult);
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            CheckDisposed();
            await Stream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            CheckDisposed();
            return Stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            CheckDisposed();
            Stream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            CheckDisposed();
            return Stream.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            CheckDisposed();
            return Stream.ReadByte();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            CheckDisposed();
            Stream.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            CheckDisposed();
            Stream.WriteByte(value);
        }

        public override bool CanRead
        {
            get
            {
                CheckDisposed();
                return Stream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                CheckDisposed();
                return Stream.CanSeek;
            }
        }

        public override bool CanTimeout
        {
            get
            {
                CheckDisposed();
                return Stream.CanTimeout;
            }
        }

        public override bool CanWrite
        {
            get
            {
                CheckDisposed();
                return Stream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                CheckDisposed();
                return Stream.Length;
            }
        }

        public override long Position
        {
            get
            {
                CheckDisposed();
                return Stream.Position;
            }
            set
            {
                CheckDisposed();
                Stream.Position = value;
            }
        }

        public override int ReadTimeout
        {
            get
            {
                CheckDisposed();
                return Stream.ReadTimeout;
            }
            set
            {
                CheckDisposed();
                Stream.ReadTimeout = value;
            }
        }

        public override int WriteTimeout
        {
            get
            {
                CheckDisposed();
                return Stream.WriteTimeout;
            }
            set
            {
                CheckDisposed();
                Stream.WriteTimeout = value;
            }
        }
    }
}
