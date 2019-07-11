using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Statiq.Common.Content;
using Statiq.Common.IO;
using Statiq.Common.Meta;
using Statiq.Common.Util;

namespace Statiq.Common.Documents
{
    /// <summary>
    /// A special type of <see cref="IDocument"/> that wraps an underlying
    /// object, providing the object properties as metadata.
    /// </summary>
    /// <remarks>
    /// Note that unlike <see cref="Document"/> and other <see cref="Document{TDocument}"/>
    /// derived documents, settings are not "bubbled up" through the document metadata.
    /// </remarks>
    /// <typeparam name="T">The type of underlying object.</typeparam>
    public class ObjectDocument<T> : IDocument
    {
        private readonly IMetadata _metadata;

        /// <inheritdoc />
        public Guid Id { get; }

        /// <summary>
        /// The underlying object.
        /// </summary>
        public T Object { get; }

        /// <inheritdoc />
        public FilePath Source { get; }

        /// <inheritdoc />
        public FilePath Destination { get; }

        /// <inheritdoc />
        public IContentProvider ContentProvider { get; }

        public ObjectDocument(T obj)
            : this(obj, null, null, null, null)
        {
        }

        public ObjectDocument(
            T obj,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            : this(obj, null, destination, items, contentProvider)
        {
        }

        public ObjectDocument(
            T obj,
            FilePath source,
            FilePath destination,
            IContentProvider contentProvider = null)
            : this(obj, source, destination, null, contentProvider)
        {
        }

        public ObjectDocument(
            T obj,
            FilePath destination,
            IContentProvider contentProvider = null)
            : this(obj, null, destination, null, contentProvider)
        {
        }

        public ObjectDocument(
            T obj,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            : this(obj, null, null, items, contentProvider)
        {
        }

        public ObjectDocument(
            T obj,
            IContentProvider contentProvider)
            : this(obj, null, null, null, contentProvider)
        {
        }

        public ObjectDocument(
            T obj,
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            : this(Guid.NewGuid(), obj, source, destination, new Metadata(items), contentProvider)
        {
        }

        public ObjectDocument(
            T obj,
            FilePath source,
            FilePath destination,
            IMetadata metadata,
            IContentProvider contentProvider = null)
            : this(Guid.NewGuid(), obj, source, destination, metadata, contentProvider)
        {
        }

        private ObjectDocument(
            Guid id,
            T obj,
            FilePath source,
            FilePath destination,
            IMetadata metadata,
            IContentProvider contentProvider)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
            if (source?.IsAbsolute == false)
            {
                throw new ArgumentException("Document sources must be absolute", nameof(source));
            }
            if (destination?.IsRelative == false)
            {
                throw new ArgumentException("Document destinations must be relative to the output path", nameof(source));
            }

            Id = id;
            Object = obj;
            Source = source;
            Destination = destination;
            _metadata = metadata ?? new Metadata();

            // Special case to set the content provider to null when cloning
            ContentProvider = contentProvider is NullContent ? null : contentProvider;
        }

        public IDocument Clone(
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider) =>
            new ObjectDocument<T>(
                Id,
                Object,
                Source ?? source,
                destination ?? Destination,
                items == null ? _metadata : new Metadata(_metadata, items),
                contentProvider ?? ContentProvider);

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
            _metadata.ContainsKey(key) || PropertyMetadata<T>.For(Object).ContainsKey(key);

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
            _metadata.TryGetRaw(key, out value) || PropertyMetadata<T>.For(Object).TryGetRaw(key, out value);

        public bool TryGetValue<TValue>(string key, out TValue value) =>
            _metadata.TryGetValue(key, out value) || PropertyMetadata<T>.For(Object).TryGetValue(key, out value);

        public bool TryGetValue(string key, out object value) => TryGetValue<object>(key, out value);

        public IMetadata GetMetadata(params string[] keys) =>
            new Metadata(this.Where(x => keys.Contains(x.Key, StringComparer.OrdinalIgnoreCase)));

#pragma warning disable RCS1077 // We want to count the enumerable items, not recursivly call this property
        public int Count => this.Count();
#pragma warning restore RCS1077

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            HashSet<string> keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, object> item in _metadata.Concat(PropertyMetadata<T>.For(Object)))
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
