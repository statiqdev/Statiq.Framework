using System;
using System.Collections.Generic;
using System.Text;

namespace Statiq.Common
{
    public abstract class StringItemStream<TItem> : ItemStream<TItem>
    {
        private readonly Encoding _encoding;
        private byte[] _buffer = new byte[256];

        protected StringItemStream(IEnumerable<TItem> items)
            : this(items, Encoding.UTF8)
        {
        }

        protected StringItemStream(IEnumerable<TItem> items, Encoding encoding)
            : base(items)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        }

        protected sealed override ReadOnlyMemory<byte> GetItemMemory(TItem item)
        {
            string itemString = GetItemString(item);
            if (itemString is null)
            {
                return default;
            }
            int byteCount = _encoding.GetByteCount(itemString);
            if (_buffer.Length < byteCount)
            {
                _buffer = new byte[byteCount];
            }
            Memory<byte> memory = new Memory<byte>(_buffer, 0, byteCount);
            _encoding.GetBytes(itemString, memory.Span);
            return memory;
        }

        protected abstract string GetItemString(TItem item);
    }
}
