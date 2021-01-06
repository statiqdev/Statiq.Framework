using System;
using System.Collections.Generic;

namespace Statiq.Common
{
    public class MemoryItemStream : ItemStream<byte[]>
    {
        public MemoryItemStream(IEnumerable<byte[]> items)
            : base(items)
        {
        }

        protected sealed override ReadOnlyMemory<byte> GetItemMemory(byte[] item) => item;
    }
}
