using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Content;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Testing.Execution;
using Wyam.Testing.Meta;

namespace Wyam.Testing.Documents
{
    /// <summary>
    /// A simple document that stores metadata in a <c>Dictionary</c> without any built-in type conversion.
    /// Also no support for content at this time.
    /// </summary>
    public class TestDocument : IDocument, ITypeConversions
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
            ContentProvider = new StreamContent(memoryStreamFactory, memoryStreamFactory.GetStream(content));
        }

        public TestDocument(Stream content)
            : this()
        {
            TestMemoryStreamFactory memoryStreamFactory = new TestMemoryStreamFactory();
            ContentProvider = new StreamContent(memoryStreamFactory, content);
        }

        public TestDocument(IEnumerable<KeyValuePair<string, object>> metadata, string content)
            : this(metadata)
        {
            TestMemoryStreamFactory memoryStreamFactory = new TestMemoryStreamFactory();
            ContentProvider = new StreamContent(memoryStreamFactory, memoryStreamFactory.GetStream(content));
        }

        public TestDocument(IEnumerable<KeyValuePair<string, object>> metadata, Stream content)
            : this(metadata)
        {
            TestMemoryStreamFactory memoryStreamFactory = new TestMemoryStreamFactory();
            ContentProvider = new StreamContent(memoryStreamFactory, content);
        }

        public TestDocument(IContentProvider contentProvider)
            : this()
        {
            ContentProvider = contentProvider;
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

        /// <inhertdoc />
        public TestDocument(IEnumerable<KeyValuePair<string, object>> metadata, IContentProvider contentProvider)
            : this(metadata)
        {
            ContentProvider = contentProvider;
        }

        public void Add(KeyValuePair<string, object> item) => _metadata.Add(item);

        public void Add(string key, object value) => _metadata.Add(key, value);

        /// <inhertdoc />
        public IMetadata WithoutSettings => this;

        /// <inhertdoc />
        public bool ContainsKey(string key) => _metadata.ContainsKey(key);

        /// <inhertdoc />
        public object GetRaw(string key) => _metadata[key];

        /// <inhertdoc />
        public bool TryGetValue<T>(string key, out T value) => _metadata.TryGetValue<T>(key, out value);

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
        public FilePath Source { get; set; }

        /// <inhertdoc />
        public FilePath Destination { get; set; }

        /// <inhertdoc />
        public string SourceString() => Source?.FullPath;

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

        public Dictionary<(Type Value, Type Result), Func<object, object>> TypeConversions => _metadata.TypeConversions;

        public void AddTypeConversion<T, TResult>(Func<T, TResult> typeConversion) => _metadata.AddTypeConversion(typeConversion);
    }
}
