using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A read-only, non-seeking stream produced by iterating over a collection of arbitrary objects.
    /// </summary>
    public abstract class ItemStream<TItem> : Stream
    {
        private readonly IEnumerable<TItem> _items;
        private ReadOnlyMemory<byte> _itemMemory;
        private IEnumerator<TItem> _itemEnumerator;

        protected ItemStream(IEnumerable<TItem> items)
        {
            _items = items;
        }

        public sealed override bool CanRead => true;

        public sealed override bool CanSeek => false;

        public sealed override bool CanWrite => false;

        public sealed override long Length => throw new NotSupportedException();

        public sealed override long Position
        {
            get
            {
                if (_itemEnumerator is null)
                {
                    return 0;
                }
                throw new NotSupportedException();
            }
            set
            {
                if (value == 0)
                {
                    // Reset the stream if setting position to 0
                    Reset();
                }
                else
                {
                    // We can't seek to a non-0 position otherwise
                    throw new NotSupportedException();
                }
            }
        }

        public sealed override long Seek(long offset, SeekOrigin origin)
        {
            // We can only seek to the beginning of the stream
            if (offset == 0 && origin == SeekOrigin.Begin)
            {
                Reset();
                return 0;
            }
            throw new NotSupportedException();
        }

        public sealed override void SetLength(long value) => throw new NotSupportedException();

        public sealed override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public sealed override void Flush()
        {
        }

        public virtual void Reset()
        {
            _itemEnumerator = null;
            _itemMemory = default;
        }

        public sealed override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan().Slice(offset, count));

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => Task.FromResult(Read(new Span<byte>(buffer, offset, count)));

        public override ValueTask<int> ReadAsync(Memory<byte> memory, CancellationToken cancellationToken)
            => new ValueTask<int>(Read(memory.Span));

        public sealed override int Read(Span<byte> buffer)
        {
            // If we don't have items, return 0
            if (_items is null)
            {
                return 0;
            }

            // If we're not already enumerating, get an enumerator
            if (_itemEnumerator is null)
            {
                _itemEnumerator = _items.GetEnumerator();
            }

            // Read until we've filled the buffer with the requested count
            int read = 0;
            while (true)
            {
                // Do we need to get more bytes?
                if (_itemMemory.IsEmpty)
                {
                    // Have we reached the end?
                    if (!_itemEnumerator.MoveNext())
                    {
                        return read;
                    }

                    // Get the current item and bytes (the item might be null, in which case we'll loop back here and go to the next)
                    TItem item = _itemEnumerator.Current;
                    if (item is object)
                    {
                        _itemMemory = GetItemMemory(item);
                    }
                }

                // Read the bytes if we have some
                if (!_itemMemory.IsEmpty)
                {
                    // If we have exactly the number of bytes to fill the remaining buffer, copy to the buffer and return
                    if (_itemMemory.Length == buffer.Length)
                    {
                        _itemMemory.Span.CopyTo(buffer);
                        read += _itemMemory.Length;
                        _itemMemory = default;
                        return read;
                    }

                    // If we have more bytes than we need, slice and retain the rest
                    if (_itemMemory.Length > buffer.Length)
                    {
                        ReadOnlyMemory<byte> copyBytes = _itemMemory.Slice(0, buffer.Length);
                        _itemMemory = _itemMemory.Slice(buffer.Length);
                        copyBytes.Span.CopyTo(buffer);
                        read += copyBytes.Length;
                        return read;
                    }

                    // We have fewer bytes than we need, fill what we can and slice the destination buffer to continue trying to fill it
                    _itemMemory.Span.CopyTo(buffer);
                    read += _itemMemory.Length;
                    buffer = buffer.Slice(_itemMemory.Length);
                    _itemMemory = default;
                }
            }
        }

        protected abstract ReadOnlyMemory<byte> GetItemMemory(TItem item);

        // Seal the rest to avoid confusion

        public sealed override bool CanTimeout => base.CanTimeout;

        public sealed override int ReadTimeout { get => base.ReadTimeout; set => base.ReadTimeout = value; }

        public sealed override int WriteTimeout { get => base.WriteTimeout; set => base.WriteTimeout = value; }

        public sealed override object InitializeLifetimeService() => base.InitializeLifetimeService();

        public sealed override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state) => base.BeginRead(buffer, offset, count, callback, state);

        public sealed override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state) => base.BeginWrite(buffer, offset, count, callback, state);

        public sealed override void Close() => base.Close();

        public sealed override void CopyTo(Stream destination, int bufferSize) => base.CopyTo(destination, bufferSize);

        public sealed override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken) => base.CopyToAsync(destination, bufferSize, cancellationToken);

        [Obsolete]
        protected sealed override WaitHandle CreateWaitHandle() => base.CreateWaitHandle();

        protected sealed override void Dispose(bool disposing) => base.Dispose(disposing);

        public sealed override ValueTask DisposeAsync() => base.DisposeAsync();

        public sealed override int EndRead(IAsyncResult asyncResult) => base.EndRead(asyncResult);

        public sealed override void EndWrite(IAsyncResult asyncResult) => base.EndWrite(asyncResult);

        public sealed override Task FlushAsync(CancellationToken cancellationToken) => base.FlushAsync(cancellationToken);

        [Obsolete]
        protected sealed override void ObjectInvariant() => base.ObjectInvariant();

        public sealed override int ReadByte() => base.ReadByte();

        public sealed override void Write(ReadOnlySpan<byte> buffer) => base.Write(buffer);

        public sealed override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => base.WriteAsync(buffer, offset, count, cancellationToken);

        public sealed override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => base.WriteAsync(buffer, cancellationToken);

        public sealed override void WriteByte(byte value) => base.WriteByte(value);
    }
}
