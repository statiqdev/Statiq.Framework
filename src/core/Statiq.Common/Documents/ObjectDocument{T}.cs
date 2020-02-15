using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Common
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
    public sealed class ObjectDocument<T> : IDocument
    {
        private readonly IMetadata _metadata;

        /// <inheritdoc />
        public Guid Id { get; }

        /// <summary>
        /// The underlying object.
        /// </summary>
        public T Object { get; }

        /// <inheritdoc />
        public NormalizedPath Source { get; }

        /// <inheritdoc />
        public NormalizedPath Destination { get; }

        /// <inheritdoc />
        public IContentProvider ContentProvider { get; }

        public ObjectDocument(T obj)
            : this(obj, null, null, null, null)
        {
        }

        public ObjectDocument(
            T obj,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            : this(obj, null, destination, items, contentProvider)
        {
        }

        public ObjectDocument(
            T obj,
            NormalizedPath source,
            NormalizedPath destination,
            IContentProvider contentProvider = null)
            : this(obj, source, destination, null, contentProvider)
        {
        }

        public ObjectDocument(
            T obj,
            NormalizedPath destination,
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
            NormalizedPath source,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            : this(Guid.NewGuid(), obj, source, destination, new Metadata(items), contentProvider)
        {
        }

        public ObjectDocument(
            T obj,
            NormalizedPath source,
            NormalizedPath destination,
            IMetadata metadata,
            IContentProvider contentProvider = null)
            : this(Guid.NewGuid(), obj, source, destination, metadata, contentProvider)
        {
        }

        private ObjectDocument(
            Guid id,
            T obj,
            NormalizedPath source,
            NormalizedPath destination,
            IMetadata metadata,
            IContentProvider contentProvider)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
            if (!source.IsAbsolute)
            {
                throw new ArgumentException("Document sources must be absolute", nameof(source));
            }
            if (!destination.IsRelative)
            {
                throw new ArgumentException("Document destinations must be relative to the output path", nameof(source));
            }

            Id = id;
            Object = obj;
            Source = source;
            Destination = destination;
            _metadata = metadata ?? new Metadata();
            ContentProvider = contentProvider ?? new NullContent();
        }

        public IDocument Clone(
            NormalizedPath source,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider) =>
            new ObjectDocument<T>(
                Id,
                Object,
                Source.IsNull ? source : Source,
                destination.IsNull ? Destination : destination,
                items == null ? _metadata : new Metadata(_metadata, items),
                contentProvider ?? ContentProvider);

        /// <inheritdoc />
        public override string ToString() => Source.IsNull ? string.Empty : Source.FullPath;

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

        // Enumerate the keys seperatly so we don't evaluate values
        public IEnumerable<string> Keys
        {
            get
            {
                HashSet<string> keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (string key in _metadata.Keys.Concat(PropertyMetadata<T>.For(Object).Keys))
                {
                    if (keys.Add(key))
                    {
                        yield return key;
                    }
                }
            }
        }

        public IEnumerable<object> Values => this.Select(x => x.Value);

        public bool TryGetRaw(string key, out object value) =>
            _metadata.TryGetRaw(key, out value) || PropertyMetadata<T>.For(Object).TryGetRaw(key, out value);

        public bool TryGetValue<TValue>(string key, out TValue value)
        {
            if (TryGetRaw(key, out object rawValue))
            {
                return TypeHelper.TryExpandAndConvert(rawValue, this, out value);
            }
            value = default;
            return false;
        }

        public bool TryGetValue(string key, out object value) => TryGetValue<object>(key, out value);

        // The Select ensures LINQ optimizations won't turn this into a recursive call to Count
        public int Count => this.Select(_ => (object)null).Count();

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
