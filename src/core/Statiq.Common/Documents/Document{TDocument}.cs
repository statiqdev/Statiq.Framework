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
    /// This will first check document metadata for
    /// metadata items. Then it will check settings
    /// if a metadata item with the given key can't
    /// be found. Then it will check public properties.
    /// In other words, document types that derive from this
    /// class (including the default document type)
    /// will provide a unified metadata view over
    /// explicit metadata, settings, and public properties.
    /// </remarks>
    /// <typeparam name="TDocument">
    /// The type of this document class (for example, declare
    /// the base class of your custom document type as
    /// <c>public class MyDoc : Document&lt;MyDoc&gt;</c>.
    /// </typeparam>
    public abstract class Document<TDocument> : IDocument
        where TDocument : Document<TDocument>
    {
        // Keep track of document versions in the base since we might create more than one clone from the same source document
        private static readonly ConcurrentDictionary<string, int> _versions = new ConcurrentDictionary<string, int>();

        private static readonly Dictionary<string, IPropertyCallAdapter> PropertyMetadata = GetPropertyMetadata();

        private readonly IEngine _engine;

        protected Document(
            IEngine engine,
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            : this(
                engine,
                Guid.NewGuid().ToString(),
                source,
                destination,
                new Metadata(engine, items),
                contentProvider)
        {
            Trace.Verbose($"Created document with ID {Id}.{Version} and source {Source.ToDisplayString()}");
        }

        protected Document(
            TDocument sourceDocument,
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            : this(
                sourceDocument._engine,
                sourceDocument.Id,
                sourceDocument.Source ?? source,
                destination ?? sourceDocument.Destination,
                items == null ? sourceDocument.Metadata : new Metadata(sourceDocument._engine, sourceDocument.Metadata, items),
                contentProvider ?? sourceDocument.ContentProvider)
        {
            Trace.Verbose($"Created document with ID {Id}.{Version} and source {Source.ToDisplayString()} from version {sourceDocument.Version}");
        }

        private Document(
            IEngine engine,
            string id,
            FilePath source,
            FilePath destination,
            IMetadata metadata,
            IContentProvider contentProvider)
        {
            if (source?.IsAbsolute == false)
            {
                throw new ArgumentException("Document sources must be absolute", nameof(source));
            }

            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Version = _versions.AddOrUpdate(id, 0, (_, ver) => ver + 1);
            Source = source;
            Destination = GetRelativeDestination(destination, engine.FileSystem.OutputPath);
            Metadata = metadata;

            // Special case to set the content provider to null when cloning
            ContentProvider = contentProvider is NullContent ? null : contentProvider;
        }

        // Internal for testing
        internal static FilePath GetRelativeDestination(FilePath destination, DirectoryPath outputPath)
        {
            if (destination?.IsAbsolute == true
                && destination.Directory.Segments.StartsWith(outputPath.Segments))
            {
                return outputPath.GetRelativePath(destination);
            }
            return destination;
        }

        /// <summary>
        /// This clone method needs to be overridden in every document type to return a new instance
        /// of that document type using it's cloning constructor.
        /// </summary>
        /// <param name="source">The new source. If this document already contains a source, then it's used and this is ignored.</param>
        /// <param name="destination">The new destination or <c>null</c> to keep the existing destination.</param>
        /// <param name="items">New metadata items or <c>null</c> not to add any new metadata.</param>
        /// <param name="contentProvider">The new content provider or <c>null</c> to keep the existing content provider.</param>
        /// <returns>A new document of the same type as this document.</returns>
        public abstract TDocument Clone(
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null);

        /// <inheritdoc />
        IDocument IDocument.Clone(
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider) =>
            Clone(source, destination, items, contentProvider);

        /// <inheritdoc />
        public string Id { get; }

        /// <inheritdoc />
        public int Version { get; }

        /// <inheritdoc />
        public FilePath Source { get; }

        /// <inheritdoc />
        public FilePath Destination { get; }

        /// <inheritdoc />
        public IMetadata Metadata { get; }

        /// <inheritdoc />
        public IContentProvider ContentProvider { get; }

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

            // Include settings and public properties, which should be enough to capture full state
            foreach (KeyValuePair<string, object> item in this)
            {
                hash.Add(item.Key);
                hash.Add(item.Value);
            }

            return hash.ToHashCode();
        }

        // Property-based metadata (property call adapter pattern concept from https://stackoverflow.com/a/26733318/807064)

        private static Dictionary<string, IPropertyCallAdapter> GetPropertyMetadata()
        {
            Dictionary<string, IPropertyCallAdapter> propertyMetadata =
                new Dictionary<string, IPropertyCallAdapter>(StringComparer.OrdinalIgnoreCase);

            // Do a first pass for non-attribute properties
            // This ensures actual properties will get added first and take precedince over any attributes that define colliding names
            List<(DocumentMetadataAttribute, MethodInfo)> attributeProperties =
                new List<(DocumentMetadataAttribute Property, MethodInfo Getter)>();
            foreach ((PropertyInfo property, MethodInfo getter) in
                typeof(TDocument)
                    .GetProperties()
                    .Select(x => (x, x.GetGetMethod()))
                    .Where(x => x.Item2 != null && x.Item2.GetParameters().Length == 0))
            {
                // If there's an attribute, do this in a second pass
                DocumentMetadataAttribute attribute = property.GetCustomAttribute<DocumentMetadataAttribute>();
                if (attribute != null)
                {
                    // Only add the property for later processing if the new name isn't null
                    if (!string.IsNullOrEmpty(attribute.Name))
                    {
                        attributeProperties.Add((attribute, getter));
                    }
                }
                else
                {
                    // No attribute, so add this property
                    if (!propertyMetadata.ContainsKey(property.Name))
                    {
                        propertyMetadata.Add(property.Name, GetPropertyCallAdapter(getter));
                    }
                }
            }

            // Now that all the actual property names have been added, add ones from the attribute
            foreach ((DocumentMetadataAttribute attribute, MethodInfo getter) in attributeProperties)
            {
                if (!propertyMetadata.ContainsKey(attribute.Name))
                {
                    propertyMetadata.Add(attribute.Name, GetPropertyCallAdapter(getter));
                }
            }

            return propertyMetadata;
        }

        private static IPropertyCallAdapter GetPropertyCallAdapter(MethodInfo getter)
        {
            Type delegateType = typeof(Func<,>).MakeGenericType(typeof(TDocument), getter.ReturnType);
            Delegate getterDelegate = getter.CreateDelegate(delegateType);
            Type adapterType = typeof(PropertyCallAdapter<>).MakeGenericType(typeof(TDocument), getter.ReturnType);
            return Activator.CreateInstance(adapterType, getterDelegate) as IPropertyCallAdapter;
        }

        private interface IPropertyCallAdapter
        {
            object GetValue(TDocument instance);
        }

        private class PropertyCallAdapter<TResult> : IPropertyCallAdapter
        {
            private readonly Func<TDocument, TResult> _getter;

            public PropertyCallAdapter(Func<TDocument, TResult> getter)
            {
                _getter = getter;
            }

            public object GetValue(TDocument instance) => _getter.Invoke(instance);
        }

        // IMetadata

        public bool ContainsKey(string key) =>
            Metadata.ContainsKey(key)
            || _engine.Settings.ContainsKey(key)
            || PropertyMetadata.ContainsKey(key);

        public object this[string key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }
                if (!TryGetValue(key, out object value))
                {
                    throw new KeyNotFoundException("The key " + key + " was not found in the document, use Get() to provide a default value.");
                }
                return value;
            }
        }

        public IEnumerable<string> Keys => this.Select(x => x.Key);

        public IEnumerable<object> Values => this.Select(x => x.Value);

        public bool TryGetRaw(string key, out object value)
        {
            if (Metadata.TryGetRaw(key, out value))
            {
                return true;
            }

            if (_engine.Settings.TryGetRaw(key, out value))
            {
                return true;
            }

            if (PropertyMetadata.TryGetValue(key, out IPropertyCallAdapter adapter))
            {
                value = adapter.GetValue((TDocument)this);
                return true;
            }

            return false;
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            if (Metadata.TryGetValue<T>(key, out value))
            {
                return true;
            }

            if (_engine.Settings.TryGetValue<T>(key, out value))
            {
                return true;
            }

            if (PropertyMetadata.TryGetValue(key, out IPropertyCallAdapter adapter))
            {
                object raw = adapter.GetValue((TDocument)this);
                return _engine.TryConvert(raw, out value);
            }

            return false;
        }

        public bool TryGetValue(string key, out object value) => TryGetValue<object>(key, out value);

        public IMetadata GetMetadata(params string[] keys) =>
            new Metadata(_engine, this.Where(x => keys.Contains(x.Key, StringComparer.OrdinalIgnoreCase)));

#pragma warning disable RCS1077 // We want to count the enumerable items, not recursivly call this property
        public int Count => this.Count();
#pragma warning restore RCS1077

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            HashSet<string> keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (KeyValuePair<string, object> item in Metadata)
            {
                if (keys.Add(item.Key))
                {
                    yield return item;
                }
            }

            foreach (KeyValuePair<string, object> item in _engine.Settings)
            {
                if (keys.Add(item.Key))
                {
                    yield return item;
                }
            }

            foreach (KeyValuePair<string, IPropertyCallAdapter> item in PropertyMetadata)
            {
                if (keys.Add(item.Key))
                {
                    yield return new KeyValuePair<string, object>(item.Key, item.Value.GetValue((TDocument)this));
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
