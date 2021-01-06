using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// A read-only, non-seeking string-based stream produced by iterating over a collection of strings.
    /// </summary>
    public class StringItemStream : StringItemStream<string>
    {
        public StringItemStream(IEnumerable<string> items)
            : base(items)
        {
        }

        protected sealed override string GetItemString(string item) => item;
    }
}
