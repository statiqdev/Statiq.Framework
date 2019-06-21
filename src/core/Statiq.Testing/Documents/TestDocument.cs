using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common.Content;
using Statiq.Common.Documents;
using Statiq.Common.IO;
using Statiq.Common.Meta;
using Statiq.Testing.Execution;
using Statiq.Testing.Meta;

namespace Statiq.Testing.Documents
{
    /// <summary>
    /// A simple document that stores metadata in a <c>Dictionary</c> without any built-in type conversion.
    /// Also no support for content at this time.
    /// </summary>
    public class TestDocument : IDocument
    {
        private readonly TestMetadata _metadata = new TestMetadata();

        public TestDocument()
        {
            Id = Guid.NewGuid().ToString();
        }

        public TestDocument(string content)
            : this()
        {
            TestMemoryStreamFactory memoryStreamFactory = new TestMemoryStreamFactory();
            ContentProvider = content == null ? null : new StreamContent(memoryStreamFactory, memoryStreamFactory.GetStream(content));
        }

        public TestDocument(Stream content)
            : this()
        {
            TestMemoryStreamFactory memoryStreamFactory = new TestMemoryStreamFactory();
            ContentProvider = content == null ? null : new StreamContent(memoryStreamFactory, content);
        }

        public TestDocument(IEnumerable<KeyValuePair<string, object>> metadata, string content)
            : this(metadata)
        {
            TestMemoryStreamFactory memoryStreamFactory = new TestMemoryStreamFactory();
            ContentProvider = content == null ? null : new StreamContent(memoryStreamFactory, memoryStreamFactory.GetStream(content));
        }

        public TestDocument(IEnumerable<KeyValuePair<string, object>> metadata, Stream content)
            : this(metadata)
        {
            TestMemoryStreamFactory memoryStreamFactory = new TestMemoryStreamFactory();
            ContentProvider = content == null ? null : new StreamContent(memoryStreamFactory, content);
        }

        public TestDocument(IContentProvider contentProvider)
            : this()
        {
            ContentProvider = contentProvider is NullContent ? null : contentProvider;
        }

        public TestDocument(IEnumerable<KeyValuePair<string, object>> metadata)
            : this()
        {
            if (metadata != null)
            {
                foreach (KeyValuePair<string, object> item in metadata)
                {
                    _metadata[item.Key] = item.Value;
                }
            }
        }

        public TestDocument(IEnumerable<KeyValuePair<string, object>> metadata, IContentProvider contentProvider)
            : this(metadata)
        {
            ContentProvider = contentProvider is NullContent ? null : contentProvider;
        }

        public TestDocument(FilePath source, FilePath destination)
        {
            Source = source;
            Destination = destination;
        }

        public TestDocument(FilePath source, FilePath destination, IEnumerable<KeyValuePair<string, object>> metadata)
            : this(metadata)
        {
            Source = source;
            Destination = destination;
        }

        public TestDocument(FilePath source, FilePath destination, IEnumerable<KeyValuePair<string, object>> metadata, string content)
            : this(metadata)
        {
            Source = source;
            Destination = destination;
            TestMemoryStreamFactory memoryStreamFactory = new TestMemoryStreamFactory();
            ContentProvider = content == null ? null : new StreamContent(memoryStreamFactory, memoryStreamFactory.GetStream(content));
        }

        public TestDocument(FilePath source, FilePath destination, IEnumerable<KeyValuePair<string, object>> metadata, Stream content)
            : this(metadata)
        {
            Source = source;
            Destination = destination;
            TestMemoryStreamFactory memoryStreamFactory = new TestMemoryStreamFactory();
            ContentProvider = content == null ? null : new StreamContent(memoryStreamFactory, content);
        }

        public TestDocument(FilePath source, FilePath destination, IEnumerable<KeyValuePair<string, object>> metadata, IContentProvider contentProvider)
            : this(metadata)
        {
            Source = source;
            Destination = destination;
            ContentProvider = contentProvider is NullContent ? null : contentProvider;
        }

        public TestDocument(FilePath source, FilePath destination, string content)
            : this()
        {
            Source = source;
            Destination = destination;
            TestMemoryStreamFactory memoryStreamFactory = new TestMemoryStreamFactory();
            ContentProvider = new StreamContent(memoryStreamFactory, memoryStreamFactory.GetStream(content));
        }

        public TestDocument(FilePath source, FilePath destination, Stream content)
            : this()
        {
            Source = source;
            Destination = destination;
            TestMemoryStreamFactory memoryStreamFactory = new TestMemoryStreamFactory();
            ContentProvider = new StreamContent(memoryStreamFactory, content);
        }

        public TestDocument(FilePath source, FilePath destination, IContentProvider contentProvider)
            : this()
        {
            Source = source;
            Destination = destination;
            ContentProvider = contentProvider is NullContent ? null : contentProvider;
        }

        public TestDocument(FilePath path)
        {
            if (path.IsAbsolute)
            {
                Source = path;
                Destination = path.FullPath.StartsWith("/input") ? new FilePath(path.FullPath.Substring(6)) : null;
            }
            else
            {
                Source = new DirectoryPath("/input").CombineFile(path);
                Destination = path;
            }
        }

        public TestDocument(FilePath path, IEnumerable<KeyValuePair<string, object>> metadata)
            : this(metadata)
        {
            if (path.IsAbsolute)
            {
                Source = path;
                Destination = path.FullPath.StartsWith("/input") ? new FilePath(path.FullPath.Substring(6)) : null;
            }
            else
            {
                Source = new DirectoryPath("/input").CombineFile(path);
                Destination = path;
            }
        }

        public TestDocument(FilePath path, IEnumerable<KeyValuePair<string, object>> metadata, string content)
            : this(metadata)
        {
            if (path.IsAbsolute)
            {
                Source = path;
                Destination = path.FullPath.StartsWith("/input") ? new FilePath(path.FullPath.Substring(6)) : null;
            }
            else
            {
                Source = new DirectoryPath("/input").CombineFile(path);
                Destination = path;
            }

            TestMemoryStreamFactory memoryStreamFactory = new TestMemoryStreamFactory();
            ContentProvider = content == null ? null : new StreamContent(memoryStreamFactory, memoryStreamFactory.GetStream(content));
        }

        public TestDocument(FilePath path, IEnumerable<KeyValuePair<string, object>> metadata, Stream content)
            : this(metadata)
        {
            if (path.IsAbsolute)
            {
                Source = path;
                Destination = path.FullPath.StartsWith("/input") ? new FilePath(path.FullPath.Substring(6)) : null;
            }
            else
            {
                Source = new DirectoryPath("/input").CombineFile(path);
                Destination = path;
            }

            TestMemoryStreamFactory memoryStreamFactory = new TestMemoryStreamFactory();
            ContentProvider = content == null ? null : new StreamContent(memoryStreamFactory, content);
        }

        public TestDocument(FilePath path, IEnumerable<KeyValuePair<string, object>> metadata, IContentProvider contentProvider)
            : this(metadata)
        {
            if (path.IsAbsolute)
            {
                Source = path;
                Destination = path.FullPath.StartsWith("/input") ? new FilePath(path.FullPath.Substring(6)) : null;
            }
            else
            {
                Source = new DirectoryPath("/input").CombineFile(path);
                Destination = path;
            }

            ContentProvider = contentProvider is NullContent ? null : contentProvider;
        }

        public TestDocument(FilePath path, string content)
            : this()
        {
            if (path.IsAbsolute)
            {
                Source = path;
                Destination = path.FullPath.StartsWith("/input") ? new FilePath(path.FullPath.Substring(6)) : null;
            }
            else
            {
                Source = new DirectoryPath("/input").CombineFile(path);
                Destination = path;
            }

            TestMemoryStreamFactory memoryStreamFactory = new TestMemoryStreamFactory();
            ContentProvider = new StreamContent(memoryStreamFactory, memoryStreamFactory.GetStream(content));
        }

        public TestDocument(FilePath path, Stream content)
            : this()
        {
            if (path.IsAbsolute)
            {
                Source = path;
                Destination = path.FullPath.StartsWith("/input") ? new FilePath(path.FullPath.Substring(6)) : null;
            }
            else
            {
                Source = new DirectoryPath("/input").CombineFile(path);
                Destination = path;
            }

            TestMemoryStreamFactory memoryStreamFactory = new TestMemoryStreamFactory();
            ContentProvider = new StreamContent(memoryStreamFactory, content);
        }

        public TestDocument(FilePath path, IContentProvider contentProvider)
            : this()
        {
            if (path.IsAbsolute)
            {
                Source = path;
            }
            else
            {
                Source = new DirectoryPath("/input").CombineFile(path);
                Destination = path;
            }

            ContentProvider = contentProvider is NullContent ? null : contentProvider;
        }

        public IDocument Clone(FilePath source, FilePath destination, IEnumerable<KeyValuePair<string, object>> items, IContentProvider contentProvider = null) =>
            new TestDocument(
                Source ?? source,
                destination ?? Destination,
                items == null ? this : this.Concat(items),
                contentProvider ?? ContentProvider)
                {
                    Id = Id
                };

        public void Add(KeyValuePair<string, object> item) => _metadata.Add(item);

        public void Add(string key, object value) => _metadata.Add(key, value);

        /// <inhertdoc />
        public IMetadata WithoutSettings => this;

        public int CacheHashCode { get; set; }

        /// <inhertdoc />
        public Task<int> GetCacheHashCodeAsync() => Task.FromResult(CacheHashCode);

        /// <inhertdoc />
        public bool ContainsKey(string key) => _metadata.ContainsKey(key);

        /// <inhertdoc />
        public bool TryGetRaw(string key, out object value) => _metadata.TryGetRaw(key, out value);

        /// <inhertdoc />
        public bool TryGetValue<TValue>(string key, out TValue value) => _metadata.TryGetValue<TValue>(key, out value);

        /// <inheritdoc />
        public bool TryGetValue(string key, out object value) => TryGetValue<object>(key, out value);

        /// <inheritdoc />
        public IMetadata GetMetadata(params string[] keys) => _metadata.GetMetadata(keys);

        /// <inhertdoc />
        public object this[string key] => _metadata[key];

        /// <inhertdoc />
        public IEnumerable<string> Keys => _metadata.Keys;

        /// <inhertdoc />
        public IEnumerable<object> Values => _metadata.Values;

        /// <inhertdoc />
        public string Id { get; set; }

        /// <inhertdoc />
        public int Version { get; set; }

        /// <inhertdoc />
        public FilePath Source { get; set; }

        /// <inhertdoc />
        public FilePath Destination { get; set; }

        /// <inhertdoc />
        public IContentProvider ContentProvider { get; set; }

        /// <inhertdoc />
        public bool HasContent => ContentProvider != null;

        /// <inhertdoc />
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

        public async Task<Stream> GetStreamAsync() =>
            ContentProvider == null ? Stream.Null : await ContentProvider.GetStreamAsync();

        public string Content => GetStringAsync().Result;

        /// <inhertdoc />
        public IMetadata Metadata => this;

        /// <inhertdoc />
        public void Dispose()
        {
        }

        /// <inhertdoc />
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _metadata.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_metadata).GetEnumerator();
        }

        /// <inhertdoc />
        public int Count => _metadata.Count;
    }
}
