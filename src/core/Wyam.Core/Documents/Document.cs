using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Core.Meta;
using Wyam.Core.Util;

namespace Wyam.Core.Documents
{
    // Because it's immutable, document metadata can still be accessed after disposal
    // Document source must be unique within the pipeline
    internal class Document : IDocument
    {
        private readonly SemaphoreSlim _streamMutex;
        private readonly MetadataStack _metadata;
        private readonly Stream _stream;
        private bool _disposeStream;
        private bool _disposed;

        internal Document(MetadataDictionary initialMetadata, Stream stream = null)
            : this(initialMetadata, null, stream, null, true)
        {
        }

        internal Document(MetadataDictionary initialMetadata, FilePath source, Stream stream, IEnumerable<KeyValuePair<string, object>> items, bool disposeStream)
            : this(Guid.NewGuid().ToString(), new MetadataStack(initialMetadata), source, stream, null, items, disposeStream)
        {
        }

        private Document(string id, MetadataStack metadata, FilePath source, Stream stream, SemaphoreSlim streamMutex, IEnumerable<KeyValuePair<string, object>> items, bool disposeStream)
        {
            if (source?.IsAbsolute == false)
            {
                throw new ArgumentException("Document sources must be absolute", nameof(source));
            }

            Id = id ?? throw new ArgumentNullException(nameof(id));
            Source = source;
            _metadata = items == null ? metadata : metadata.Clone(items);

            if (stream == null)
            {
                _stream = Stream.Null;
            }
            else
            {
                if (!stream.CanRead)
                {
                    throw new ArgumentException("Document stream must support reading.", nameof(stream));
                }

                if (!stream.CanSeek)
                {
                    _stream = new SeekableStream(stream, disposeStream);
                    _disposeStream = true;
                }
                else
                {
                    _stream = stream;
                    _disposeStream = disposeStream;
                }
            }
            _streamMutex = streamMutex ?? new SemaphoreSlim(1);
        }

        internal Document(Document sourceDocument, FilePath source, IEnumerable<KeyValuePair<string, object>> items = null)
            : this(
                sourceDocument.Id,
                sourceDocument._metadata,
                sourceDocument.Source ?? source,
                sourceDocument._stream,
                sourceDocument._streamMutex,
                items,
                sourceDocument._disposeStream)
        {
            sourceDocument.CheckDisposed();

            // Don't dispose the stream since the cloned document might be final and get passed to another pipeline, it'll take care of final disposal
            sourceDocument._disposeStream = false;
        }

        internal Document(
            Document sourceDocument,
            FilePath source,
            Stream stream,
            IEnumerable<KeyValuePair<string, object>> items = null,
            bool disposeStream = true)
            : this(
                sourceDocument.Id,
                sourceDocument._metadata,
                sourceDocument.Source ?? source,
                stream ?? sourceDocument._stream,
                stream == null ? sourceDocument._streamMutex : null,
                items,
                disposeStream)
        {
            sourceDocument.CheckDisposed();
        }

        internal Document(
            Document sourceDocument,
            Stream stream,
            IEnumerable<KeyValuePair<string, object>> items = null,
            bool disposeStream = true)
            : this(
                sourceDocument.Id,
                sourceDocument._metadata,
                sourceDocument.Source,
                stream ?? sourceDocument._stream,
                stream == null ? sourceDocument._streamMutex : null,
                items,
                disposeStream)
        {
            sourceDocument.CheckDisposed();
        }

        internal Document(Document sourceDocument, IEnumerable<KeyValuePair<string, object>> items)
            : this(
                sourceDocument.Id,
                sourceDocument._metadata,
                sourceDocument.Source,
                sourceDocument._stream,
                sourceDocument._streamMutex,
                items,
                sourceDocument._disposeStream)
        {
            sourceDocument.CheckDisposed();

            // Don't dispose the stream since the cloned document might be final and get passed to another pipeline, it'll take care of final disposal
            sourceDocument._disposeStream = false;
        }

        public FilePath Source { get; }

        public string SourceString() => Source?.ToString() ?? "[unknown source]";

        public string Id { get; }

        public IMetadata Metadata => _metadata;

        public string Content
        {
            get
            {
                CheckDisposed();
                _streamMutex.Wait();
                try
                {
                    _stream.Position = 0;
                    using (StreamReader reader = new StreamReader(_stream, Encoding.UTF8, true, 4096, true))
                    {
                        return reader.ReadToEnd();
                    }
                }
                finally
                {
                    _streamMutex.Release();
                }
            }
        }

        public Stream GetStream()
        {
            CheckDisposed();
            _streamMutex.Wait();
            _stream.Position = 0;
            return new BlockingStream(_stream, this);
        }

        internal void ReleaseStream() => _streamMutex.Release();

        public override string ToString() => _disposed ? string.Empty : Content;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (_disposeStream)
            {
                _stream.Dispose();
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
