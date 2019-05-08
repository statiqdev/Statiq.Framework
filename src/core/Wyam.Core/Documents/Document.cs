using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Wyam.Common.Content;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Core.Meta;

namespace Wyam.Core.Documents
{
    // Because it's immutable, document metadata can still be accessed after disposal
    // Document source must be unique within the pipeline
    internal class Document : IDocument
    {
        private static readonly Dictionary<IContentProvider, int> _contentProviderReferenceCount = new Dictionary<IContentProvider, int>();
        private static readonly object _contentProviderReferenceCountLock = new object();

        private readonly MetadataStack _metadata;
        private readonly IContentProvider _contentProvider;
        private bool _disposed;

        internal Document(
            MetadataDictionary initialMetadata,
            IContentProvider contentProvider = null)
            : this(initialMetadata, null, null, contentProvider, null)
        {
        }

        internal Document(
            MetadataDictionary initialMetadata,
            FilePath source,
            FilePath destination,
            IContentProvider contentProvider,
            IEnumerable<KeyValuePair<string, object>> items)
            : this(
                  Guid.NewGuid().ToString(),
                  new MetadataStack(initialMetadata),
                  source,
                  destination,
                  contentProvider,
                  items)
        {
        }

        internal Document(
            Document sourceDocument,
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items = null)
            : this(
                sourceDocument.Id,
                sourceDocument._metadata,
                sourceDocument.Source ?? source,
                destination ?? sourceDocument.Destination,
                sourceDocument._contentProvider,
                items)
        {
            sourceDocument.CheckDisposed();
        }

        internal Document(
            Document sourceDocument,
            FilePath source,
            FilePath destination,
            IContentProvider contentProvider,
            IEnumerable<KeyValuePair<string, object>> items = null)
            : this(
                sourceDocument.Id,
                sourceDocument._metadata,
                sourceDocument.Source ?? source,
                destination ?? sourceDocument.Destination,
                contentProvider ?? sourceDocument._contentProvider,
                items)
        {
            sourceDocument.CheckDisposed();
        }

        internal Document(
            Document sourceDocument,
            IContentProvider contentProvider,
            IEnumerable<KeyValuePair<string, object>> items = null)
            : this(
                sourceDocument.Id,
                sourceDocument._metadata,
                sourceDocument.Source,
                sourceDocument.Destination,
                contentProvider ?? sourceDocument._contentProvider,
                items)
        {
            sourceDocument.CheckDisposed();
        }

        internal Document(Document sourceDocument, IEnumerable<KeyValuePair<string, object>> items)
            : this(
                sourceDocument.Id,
                sourceDocument._metadata,
                sourceDocument.Source,
                sourceDocument.Destination,
                sourceDocument._contentProvider,
                items)
        {
            sourceDocument.CheckDisposed();
        }

        private Document(
            string id,
            MetadataStack metadata,
            FilePath source,
            FilePath destination,
            IContentProvider contentProvider,
            IEnumerable<KeyValuePair<string, object>> items)
        {
            if (source?.IsAbsolute == false)
            {
                throw new ArgumentException("Document sources must be absolute", nameof(source));
            }

            Id = id ?? throw new ArgumentNullException(nameof(id));
            Source = source;
            Destination = destination;
            _metadata = items == null ? metadata : metadata.Clone(items);
            _contentProvider = contentProvider;

            if (_contentProvider != null)
            {
                lock (_contentProviderReferenceCountLock)
                {
                    if (_contentProviderReferenceCount.TryGetValue(_contentProvider, out int count))
                    {
                        _contentProviderReferenceCount[_contentProvider] = count + 1;
                    }
                    else
                    {
                        _contentProviderReferenceCount.Add(contentProvider, 1);
                    }
                }
            }
        }

        public FilePath Source { get; }

        public FilePath Destination { get; }

        public string SourceString() => Source?.ToString() ?? "[unknown source]";

        public string Id { get; }

        public IMetadata Metadata => _metadata;

        public async Task<string> GetStringAsync()
        {
            CheckDisposed();
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

        public async Task<Stream> GetStreamAsync()
        {
            CheckDisposed();
            return _contentProvider == null ? Stream.Null : await _contentProvider.GetStreamAsync();
        }

        internal IContentProvider ContentProvider
        {
            get
            {
                CheckDisposed();
                return _contentProvider;
            }
        }

        public bool HasContent
        {
            get
            {
                CheckDisposed();
                return _contentProvider != null;
            }
        }

        public override string ToString() => _disposed ? string.Empty : Source?.FullPath ?? string.Empty;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (_contentProvider != null)
            {
                int count;
                lock (_contentProviderReferenceCountLock)
                {
                    if (!_contentProviderReferenceCount.TryGetValue(_contentProvider, out count))
                    {
                        throw new InvalidOperationException("Unexepected document content provider reference count missing");
                    }
                    count--;
                    if (count == 0)
                    {
                        _contentProviderReferenceCount.Remove(_contentProvider);
                    }
                    else
                    {
                        _contentProviderReferenceCount[_contentProvider] = count;
                    }
                }

                // Dispose the content provider outside the lock
                if (count == 0)
                {
                    _contentProvider.Dispose();
                }
            }

            _disposed = true;
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(
                    nameof(Document),
                    $"Attempted to access disposed document with ID {Id} and source {SourceString()}");
            }
        }

        public IMetadata WithoutSettings => new MetadataStack(_metadata.Stack.Reverse().Skip(1));

        // IMetadata

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _metadata.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool ContainsKey(string key) => _metadata.ContainsKey(key);

        public object this[string key] => _metadata[key];

        public IEnumerable<string> Keys => _metadata.Keys;

        public IEnumerable<object> Values => _metadata.Values;

        public object GetRaw(string key) => _metadata.GetRaw(key);

        public bool TryGetValue<T>(string key, out T value) => _metadata.TryGetValue<T>(key, out value);

        public bool TryGetValue(string key, out object value) => TryGetValue<object>(key, out value);

        public IMetadata GetMetadata(params string[] keys) => _metadata.GetMetadata(keys);

        public int Count => _metadata.Count;
    }
}
