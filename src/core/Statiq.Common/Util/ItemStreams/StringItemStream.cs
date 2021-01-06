using System.Collections.Generic;

namespace Statiq.Common
{
    public class StringItemStream : StringItemStream<string>
    {
        public StringItemStream(IEnumerable<string> items)
            : base(items)
        {
        }

        protected sealed override string GetItemString(string item) => item;
    }
}
