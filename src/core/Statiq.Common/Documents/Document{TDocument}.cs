using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Statiq.Common.Configuration;
using Statiq.Common.Content;
using Statiq.Common.Execution;
using Statiq.Common.IO;
using Statiq.Common.Meta;
using Statiq.Common.Tracing;
using Statiq.Common.Util;

namespace Statiq.Common.Documents
{
    /// <summary>
    /// A base class for custom document types.
    /// </summary>
    /// <remarks>
    /// Document implementations will consolidate metadata
    /// from explicit metadata, implementation properties,
    /// and finally default metadata (usually the engine
    /// settings).
    /// </remarks>
    /// <typeparam name="TDocument">
    /// The type of this document class (for example, declare
    /// the base class of your custom document type as
    /// <c>public class MyDoc : Document&lt;MyDoc&gt;</c>.
    /// </typeparam>
    public abstract class Document<TDocument> : FactoryDocument, IDocument
        where TDocument : Document<TDocument>, new()
    {
        private IMetadata _baseMetadata;

        /// <inheritdoc />
        public string Id { get; private set; } = Guid.NewGuid().ToString();

        /// <inheritdoc />
        public FilePath Source { get; private set; }

        /// <inheritdoc />
        public FilePath Destination { get; private set; }

        /// <inheritdoc />
        public IMetadata Metadata { get; private set; }

        /// <inheritdoc />
        public IContentProvider ContentProvider { get; private set; }

        protected Document()
            : this(null, null, null, null, null)
        {
        }

        protected Document(
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            : this(null, null, destination, items, contentProvider)
        {
        }

        protected Document(
            FilePath source,
            FilePath destination,
            IContentProvider contentProvider = null)
            : this(null, source, destination, null, contentProvider)
        {
        }

        protected Document(
            FilePath destination,
            IContentProvider contentProvider = null)
            : this(null, null, destination, null, contentProvider)
        {
        }

        protected Document(
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            : this(null, null, null, items, contentProvider)
        {
        }

        protected Document(IContentProvider contentProvider)
            : this(null, null, null, null, contentProvider)
        {
        }

        protected Document(
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            : this(null, source, destination, items, contentProvider)
        {
        }

        protected Document(
            IMetadata baseMetadata,
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            : this(baseMetadata, source, destination, new Metadata(items), contentProvider)
        {
        }

        protected Document(
            IMetadata baseMetadata,
            FilePath source,
            FilePath destination,
            IMetadata metadata,
            IContentProvider contentProvider = null)
        {
            Initialize(baseMetadata, source, destination, metadata, contentProvider);
        }

        internal override IDocument Initialize(
            IMetadata baseMetadata,
            FilePath source,
            FilePath destination,
            IMetadata metadata,
            IContentProvider contentProvider)
        {
            if (source?.IsAbsolute == false)
            {
                throw new ArgumentException("Document sources must be absolute", nameof(source));
            }
            if (destination?.IsRelative == false)
            {
                throw new ArgumentException("Document destinations must be relative to the output path", nameof(source));
            }

            _baseMetadata = baseMetadata;
            Source = source;
            Destination = destination;
            Metadata = metadata ?? new Metadata();

            // Special case to set the content provider to null when cloning
            ContentProvider = contentProvider is NullContent ? null : contentProvider;

            return this;
        }

        /// <inheritdoc />
        public IDocument Clone(
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider)
        {
            TDocument document = Clone();
            document.Id = Id;  // Make sure it's got the same ID in case implementation overrode Clone()
            return document.Initialize(
                _baseMetadata,
                Source ?? source,
                destination ?? Destination,
                items == null ? Metadata : new Metadata(Metadata, items),
                contentProvider ?? ContentProvider);
        }

        protected virtual TDocument Clone() => (TDocument)MemberwiseClone();

        /// <inheritdoc />
        public async Task<string> GetStringAsync()
        {
            Stream stream = await GetStreamAsync();
            if (stream == null || stream == Stream.Null)
            {
                return string.Empty;
            }
            using (StreamReader reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        /// <inheritdoc />
        public async Task<Stream> GetStreamAsync() =>
            ContentProvider == null ? Stream.Null : await ContentProvider.GetStreamAsync();

        /// <inheritdoc />
        public bool HasContent => ContentProvider != null;

        /// <inheritdoc />
        public override string ToString() => Source?.FullPath ?? string.Empty;

        /// <inheritdoc />
        public virtual async Task<int> GetCacheHashCodeAsync()
        {
            HashCode hash = default;
            using (Stream stream = await GetStreamAsync())
            {
                hash.Add(await Crc32.CalculateAsync(stream));
            }

            foreach (KeyValuePair<string, object> item in this)
            {
                hash.Add(item.Key);
                hash.Add(item.Value);
            }

            return hash.ToHashCode();
        }

        // IMetadata

        public bool ContainsKey(string key) =>
            Metadata.ContainsKey(key)
            || PropertyMetadata<TDocument>.For((TDocument)this).ContainsKey(key)
            || (_baseMetadata?.ContainsKey(key) ?? false);

        public object this[string key]
        {
            get
            {
                if (!TryGetValue(key ?? throw new ArgumentNullException(nameof(key)), out object value))
                {
                    throw new KeyNotFoundException("The key " + key + " was not found in the document, use Get() to provide a default value.");
                }
                return value;
            }
        }

        public IEnumerable<string> Keys => this.Select(x => x.Key);

        public IEnumerable<object> Values => this.Select(x => x.Value);

        public bool TryGetRaw(string key, out object value) =>
            Metadata.TryGetRaw(key, out value)
            || PropertyMetadata<TDocument>.For((TDocument)this).TryGetRaw(key, out value)
            || (_baseMetadata?.TryGetRaw(key, out value) ?? false);

        public bool TryGetValue<TValue>(string key, out TValue value) =>
            Metadata.TryGetValue(key, out value)
            || PropertyMetadata<TDocument>.For((TDocument)this).TryGetValue(key, out value)
            || (_baseMetadata?.TryGetValue(key, out value) ?? false);

        public bool TryGetValue(string key, out object value) => TryGetValue<object>(key, out value);

        public IMetadata GetMetadata(params string[] keys) =>
            new Metadata(this.Where(x => keys.Contains(x.Key, StringComparer.OrdinalIgnoreCase)));

#pragma warning disable RCS1077 // We want to count the enumerable items, not recursivly call this property
        public int Count => this.Count();
#pragma warning restore RCS1077

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            HashSet<string> keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, object> item in Metadata
                .Concat(PropertyMetadata<TDocument>.For((TDocument)this))
                .Concat((IEnumerable<KeyValuePair<string, object>>)_baseMetadata ?? Array.Empty<KeyValuePair<string, object>>()))
            {
                if (keys.Add(item.Key))
                {
                    yield return item;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
