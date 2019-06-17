using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Statiq.Common.Documents;
using Statiq.Common.IO;
using Statiq.Common.Meta;

namespace Statiq.Core.Documents
{
    /// <summary>
    /// Derive custom document types from this class to get built-in support.
    /// </summary>
    public abstract class CustomDocument : IDocument
    {
        internal IDocument Document { get; set; }

        /// <summary>
        /// Clones this instance of the document. You must return a new instance of your
        /// custom document type, even if nothing will change, otherwise the document factory
        /// will throw an exception. The default implementation of this method performs a
        /// <code>object.MemberwiseClone()</code>.
        /// </summary>
        /// <returns>A new custom document instance with the same values as the current instance.</returns>
        protected internal virtual CustomDocument Clone() => (CustomDocument)MemberwiseClone();

        /// <inheritdoc />
        public bool HasContent => Document.HasContent;

        /// <inheritdoc />
        public IMetadata WithoutSettings => Document.WithoutSettings;

        /// <inheritdoc />
        public virtual Task<int> GetCacheHashCodeAsync() => Document.GetCacheHashCodeAsync();

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => Document.GetEnumerator();

        /// <inheritdoc />
        public int Count => Document.Count;

        /// <inheritdoc />
        public bool ContainsKey(string key) => Document.ContainsKey(key);

        /// <inheritdoc />
        public object this[string key] => Document[key];

        /// <inheritdoc />
        public IEnumerable<string> Keys => Document.Keys;

        /// <inheritdoc />
        public IEnumerable<object> Values => Document.Values;

        /// <inheritdoc />
        public object GetRaw(string key) => Document.GetRaw(key);

        /// <inheritdoc />
        public bool TryGetValue<T>(string key, out T value) => Document.TryGetValue<T>(key, out value);

        /// <inheritdoc />
        public bool TryGetValue(string key, out object value) => TryGetValue<object>(key, out value);

        /// <inheritdoc />
        public IMetadata GetMetadata(params string[] keys) => Document.GetMetadata(keys);

        /// <inheritdoc />
        public void Dispose() => Document.Dispose();

        /// <inheritdoc />
        public FilePath Source => Document.Source;

        /// <inheritdoc />
        public FilePath Destination => Document.Destination;

        /// <inheritdoc />
        public string Id => Document.Id;

        /// <inheritdoc />
        public int Version => Document.Version;

        /// <inheritdoc />
        public IMetadata Metadata => Document.Metadata;

        /// <inheritdoc />
        public Task<string> GetStringAsync() => Document.GetStringAsync();

        /// <inheritdoc />
        public Task<Stream> GetStreamAsync() => Document.GetStreamAsync();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
