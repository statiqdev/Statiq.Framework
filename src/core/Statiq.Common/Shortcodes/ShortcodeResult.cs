using System.Collections.Generic;
using System.IO;

namespace Statiq.Common
{
    public class ShortcodeResult
    {
        public ShortcodeResult(IContentProvider contentProvider, IEnumerable<KeyValuePair<string, object>> nestedMetadata = null)
        {
            ContentProvider = contentProvider;
            NestedMetadata = nestedMetadata is null
                ? new Dictionary<string, object>()
                : new Dictionary<string, object>(nestedMetadata);
        }

        public ShortcodeResult(Stream content, IEnumerable<KeyValuePair<string, object>> nestedMetadata = null)
            : this(content is null ? null : IExecutionContext.Current.GetContentProvider(content), nestedMetadata)
        {
        }

        public ShortcodeResult(string content, IEnumerable<KeyValuePair<string, object>> nestedMetadata = null)
            : this(content is null ? null : IExecutionContext.Current.MemoryStreamFactory.GetStream(content), nestedMetadata)
        {
        }

        public IContentProvider ContentProvider { get; }

        public IDictionary<string, object> NestedMetadata { get; }

        public static implicit operator ShortcodeResult(Stream content) => content is null ? null : new ShortcodeResult(content);

        public static implicit operator ShortcodeResult(string content) => string.IsNullOrEmpty(content) ? null : new ShortcodeResult(content);
    }
}
