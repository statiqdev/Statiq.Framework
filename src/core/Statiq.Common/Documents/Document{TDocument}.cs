using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A base class for custom document types.
    /// </summary>
    /// <remarks>
    /// Create a derived class to use a custom document type.
    /// </remarks>
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
        private bool _initialized;
        private IMetadata _metadata;
        private IMetadata _baseMetadata;
        private IContentProvider _contentProvider;
        private FilePath _destination;
        private FilePath _source;

        /// <summary>
        /// Every derived document type should implement an empty default constructor for the
        /// document factory to use. The empty constructor _should not_ call other constructors
        /// (otherwise the document will be incorrectly marked as fully initialized).
        /// If properties like <see cref="Source"/> or <see cref="Metadata"/> need to be initialized
        /// to default values, most provide protected setters that can be called until the document
        /// has been accessed or fully initialized by the factory (at which point the setters will throw).
        /// </summary>
        protected Document()
        {
            // Don't call the other constructors for the empty constructor because it's a special case
            // If we do, that'll trigger initialization, but the empty constructor is also used
            // by the document factory to instantiate a document _then_ initialize it explicitly
            // and we can't initialize twice or it'll throw
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
            : this(baseMetadata, source, destination, items == null ? null : new Metadata(items), contentProvider)
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

        // Initialization has to be performed separately from construction to maintain the illusion of immutability
        // since clone operations perform a member-wise clone by default (and possibly even not this since it can be
        // overridden) which results in a new document instance before we have the chance to set properties like
        // metadata, source, and destination in the constructor
        internal override IDocument Initialize(
            IMetadata baseMetadata,
            FilePath source,
            FilePath destination,
            IMetadata metadata,
            IContentProvider contentProvider)
        {
            CheckInitialized();
            if (source?.IsAbsolute == false)
            {
                throw new ArgumentException("Document sources must be absolute", nameof(source));
            }
            if (destination?.IsRelative == false)
            {
                throw new ArgumentException("Document destinations must be relative to the output path", nameof(destination));
            }

            _baseMetadata = baseMetadata;
            _metadata = metadata;
            _source = source;
            _destination = destination;
            _contentProvider = contentProvider ?? new NullContent();

            _initialized = true;
            return this;
        }

        /// <inheritdoc />
        public IDocument Clone(
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider)
        {
            _initialized = true;
            TDocument document = Clone();
            document._initialized = false;  // The newly cloned document would have copied over our value for initialized
            document.Id = Id;  // Make sure it's got the same ID in case implementation overrode Clone()
            return document.Initialize(
                _baseMetadata,
                _source ?? source,
                destination ?? _destination,
                items == null ? _metadata : new Metadata(_metadata, items),
                contentProvider ?? _contentProvider);
        }

        protected virtual TDocument Clone() => (TDocument)MemberwiseClone();

        private void CheckInitialized()
        {
            if (_initialized)
            {
                throw new InvalidOperationException($"Document with ID {Id} was previously initialized");
            }
        }

        /// <inheritdoc />
        [PropertyMetadata(null)]
        public Guid Id { get; private set; } = Guid.NewGuid();

        protected IMetadata BaseMetadata
        {
            get
            {
                _initialized = true;
                return _baseMetadata;
            }
            set
            {
                CheckInitialized();
                _baseMetadata = value;
            }
        }

        protected IMetadata Metadata
        {
            get
            {
                _initialized = true;
                return _metadata;
            }
            set
            {
                CheckInitialized();
                _metadata = value;
            }
        }

        /// <inheritdoc />
        public FilePath Source
        {
            get
            {
                _initialized = true;
                return _source;
            }
            protected set
            {
                CheckInitialized();
                _source = value;
            }
        }

        /// <inheritdoc />
        public FilePath Destination
        {
            get
            {
                _initialized = true;
                return _destination;
            }
            protected set
            {
                CheckInitialized();
                _destination = value;
            }
        }

        /// <inheritdoc />
        public IContentProvider ContentProvider
        {
            get
            {
                _initialized = true;
                return _contentProvider;
            }
            protected set
            {
                CheckInitialized();
                _contentProvider = value ?? new NullContent();
            }
        }

        // Allow overrides
        // TODO: Replace with base(IDocument).GetCacheHashCodeAsync() when available (base interface method call not in language yet)
        public virtual async Task<int> GetCacheHashCodeAsync()
        {
            HashCode hash = default;
            using (Stream stream = ((IDocument)this).GetContentStream())
            {
                hash.Add(await Crc32.CalculateAsync(stream));
            }

            // We exclude ContentProvider from hash as we already added CRC for content above.
            foreach (KeyValuePair<string, object> item in this
                .Where(x => x.Key != nameof(ContentProvider)))
            {
                hash.Add(item.Key);
                hash.Add(item.Value);
            }

            return hash.ToHashCode();
        }

        // Allow overrides
        // TODO: Replace with base(IDocument).ToDisplayString() when available (base interface method call not in language yet)
        public virtual string ToDisplayString() => Source?.ToDisplayString() ?? "unknown source";

        /// <inheritdoc />
        public override string ToString() => Source?.FullPath ?? string.Empty;

        // IMetadata

        public bool ContainsKey(string key) =>
            (Metadata?.ContainsKey(key) ?? false)
            || PropertyMetadata<TDocument>.For((TDocument)this).ContainsKey(key)
            || (BaseMetadata?.ContainsKey(key) ?? false);

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

        [PropertyMetadata(null)]
        public IEnumerable<string> Keys => this.Select(x => x.Key);

        [PropertyMetadata(null)]
        public IEnumerable<object> Values => this.Select(x => x.Value);

        public bool TryGetRaw(string key, out object value)
        {
            value = default;
            return (Metadata?.TryGetRaw(key, out value) ?? false)
                || PropertyMetadata<TDocument>.For((TDocument)this).TryGetRaw(key, out value)
                || (BaseMetadata?.TryGetRaw(key, out value) ?? false);
        }

        public bool TryGetValue<TValue>(string key, out TValue value)
        {
            value = default;
            return (Metadata?.TryGetValue(key, out value) ?? false)
                || PropertyMetadata<TDocument>.For((TDocument)this).TryGetValue(key, out value)
                || (BaseMetadata?.TryGetValue(key, out value) ?? false);
        }

        public bool TryGetValue(string key, out object value) => TryGetValue<object>(key, out value);

        public IMetadata GetMetadata(params string[] keys) =>
            new Metadata(this.Where(x => keys.Contains(x.Key, StringComparer.OrdinalIgnoreCase)));

        // We have to exclude properties that use the enumerator because they cause infinite recursion
        // The Select ensures LINQ optimizations won't turn this into a recursive call to Count
        [PropertyMetadata(null)]
        public int Count => this.Select(_ => (object)null).Count();

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            HashSet<string> keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (Metadata != null)
            {
                foreach (KeyValuePair<string, object> item in Metadata)
                {
                    if (keys.Add(item.Key))
                    {
                        yield return item;
                    }
                }
            }

            foreach (KeyValuePair<string, object> item in PropertyMetadata<TDocument>.For((TDocument)this))
            {
                if (keys.Add(item.Key))
                {
                    yield return item;
                }
            }

            if (BaseMetadata != null)
            {
                foreach (KeyValuePair<string, object> item in BaseMetadata)
                {
                    if (keys.Add(item.Key))
                    {
                        yield return item;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
