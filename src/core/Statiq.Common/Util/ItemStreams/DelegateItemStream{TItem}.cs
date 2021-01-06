using System;
using System.Collections.Generic;

namespace Statiq.Common
{
    public class DelegateItemStream<TItem> : ItemStream<TItem>
    {
        private readonly Func<TItem, ReadOnlyMemory<byte>> _getBytes;

        public DelegateItemStream(IEnumerable<TItem> items, Func<TItem, ReadOnlyMemory<byte>> getBytes)
            : base(items)
        {
            _getBytes = getBytes ?? throw new ArgumentNullException(nameof(getBytes));
        }

        protected override ReadOnlyMemory<byte> GetItemMemory(TItem item) => _getBytes(item);
    }
}
