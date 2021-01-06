using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A read-only, non-seeking string-based stream produced by iterating over a collection of arbitrary objects.
    /// </summary>
    public abstract class StringItemStream<TItem> : ItemStream<TItem>
    {
        private readonly Encoder _encoder;
        private byte[] _buffer = new byte[256];

        protected StringItemStream(IEnumerable<TItem> items)
            : this(items, Encoding.UTF8)
        {
        }

        protected StringItemStream(IEnumerable<TItem> items, Encoding encoding)
            : base(items)
        {
            _encoder = (encoding ?? throw new ArgumentNullException(nameof(encoding))).GetEncoder();
        }

        public override void Reset()
        {
            base.Reset();
            _encoder.Reset();
        }

        protected sealed override ReadOnlyMemory<byte> GetItemMemory(TItem item)
        {
            string itemString = GetItemString(item);
            if (itemString is null)
            {
                return default;
            }
            int byteCount = _encoder.GetByteCount(itemString, false);
            if (_buffer.Length < byteCount)
            {
                _buffer = new byte[byteCount];
            }
            Memory<byte> memory = new Memory<byte>(_buffer, 0, byteCount);
            _encoder.GetBytes(itemString, memory.Span, false);
            return memory;
        }

        protected abstract string GetItemString(TItem item);
    }
}
