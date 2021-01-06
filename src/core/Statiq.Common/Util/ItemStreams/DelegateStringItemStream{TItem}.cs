using System;
using System.Collections.Generic;

namespace Statiq.Common
{
    public class DelegateStringItemStream<TItem> : StringItemStream<TItem>
    {
        private readonly Func<TItem, string> _getString;

        public DelegateStringItemStream(IEnumerable<TItem> items, Func<TItem, string> getString)
            : base(items)
        {
            _getString = getString ?? throw new ArgumentNullException(nameof(getString));
        }

        protected override string GetItemString(TItem item) => _getString(item);
    }
}
