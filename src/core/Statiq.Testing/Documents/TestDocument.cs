using System.Collections.Generic;
using System.IO;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestDocument : Document<TestDocument>
    {
        public TestDocument()
        {
            Metadata = new TestMetadata();
        }

        public TestDocument(IContentProvider contentProvider)
            : this(null, null, null, null, contentProvider)
        {
        }

        public TestDocument(IEnumerable<KeyValuePair<string, object>> items, IContentProvider contentProvider = null)
            : this(null, null, null, items, contentProvider)
        {
        }

        public TestDocument(FilePath source, FilePath destination, IContentProvider contentProvider = null)
            : this(null, source, destination, null, contentProvider)
        {
        }

        public TestDocument(FilePath source, FilePath destination, IEnumerable<KeyValuePair<string, object>> items, IContentProvider contentProvider = null)
            : this(null, source, destination, items, contentProvider)
        {
        }

        public TestDocument(IMetadata baseMetadata, FilePath source, FilePath destination, IEnumerable<KeyValuePair<string, object>> items, IContentProvider contentProvider = null)
            : this(baseMetadata, source, destination, items == null ? null : new TestMetadata(items), contentProvider)
        {
        }

        public TestDocument(IMetadata baseMetadata, FilePath source, FilePath destination, IMetadata metadata, IContentProvider contentProvider = null)
            : base(baseMetadata, source, destination, metadata is TestMetadata ? metadata : new TestMetadata(metadata), contentProvider)
        {
            // All constructors lead here
        }

        // Special test constructors

        public TestDocument(string content)
            : this(GetContentProvider(content))
        {
        }

        public TestDocument(Stream content)
            : this(GetContentProvider(content))
        {
        }

        public TestDocument(IEnumerable<KeyValuePair<string, object>> metadata, string content)
            : this(metadata, GetContentProvider(content))
        {
        }

        public TestDocument(IEnumerable<KeyValuePair<string, object>> metadata, Stream content)
            : this(metadata, GetContentProvider(content))
        {
        }

        public TestDocument(FilePath source, FilePath destination, IEnumerable<KeyValuePair<string, object>> metadata, string content)
            : this(source, destination, metadata, GetContentProvider(content))
        {
        }

        public TestDocument(FilePath source, FilePath destination, IEnumerable<KeyValuePair<string, object>> metadata, Stream content)
            : this(source, destination, metadata, GetContentProvider(content))
        {
        }

        public TestDocument(FilePath source, FilePath destination, string content)
            : this(source, destination, GetContentProvider(content))
        {
        }

        public TestDocument(FilePath source, FilePath destination, Stream content)
            : this(source, destination, GetContentProvider(content))
        {
        }

        public TestDocument(FilePath path)
            : this(GetSourcePath(path), GetDestinationPath(path))
        {
        }

        public TestDocument(FilePath path, IEnumerable<KeyValuePair<string, object>> metadata)
            : this(GetSourcePath(path), GetDestinationPath(path), metadata)
        {
        }

        public TestDocument(FilePath path, IEnumerable<KeyValuePair<string, object>> metadata, string content)
            : this(GetSourcePath(path), GetDestinationPath(path), metadata, GetContentProvider(content))
        {
        }

        public TestDocument(FilePath path, IEnumerable<KeyValuePair<string, object>> metadata, Stream content)
            : this(GetSourcePath(path), GetDestinationPath(path), metadata, GetContentProvider(content))
        {
        }

        public TestDocument(FilePath path, IEnumerable<KeyValuePair<string, object>> metadata, IContentProvider contentProvider)
            : this(GetSourcePath(path), GetDestinationPath(path), metadata, contentProvider)
        {
        }

        public TestDocument(FilePath path, string content)
            : this(GetSourcePath(path), GetDestinationPath(path), GetContentProvider(content))
        {
        }

        public TestDocument(FilePath path, Stream content)
            : this(GetSourcePath(path), GetDestinationPath(path), GetContentProvider(content))
        {
        }

        public TestDocument(FilePath path, IContentProvider contentProvider)
            : this(GetSourcePath(path), GetDestinationPath(path), contentProvider)
        {
        }

        // Constructor helpers

        private static IContentProvider GetContentProvider(string content)
        {
            TestMemoryStreamFactory memoryStreamFactory = new TestMemoryStreamFactory();
            return content == null ? null : new StreamContent(memoryStreamFactory, memoryStreamFactory.GetStream(content));
        }

        private static IContentProvider GetContentProvider(Stream content)
        {
            TestMemoryStreamFactory memoryStreamFactory = new TestMemoryStreamFactory();
            return content == null ? null : new StreamContent(memoryStreamFactory, content);
        }

        private static FilePath GetSourcePath(FilePath path) =>
            path == null ? null : path.IsAbsolute ? path : new DirectoryPath("/input").CombineFile(path);

        private static FilePath GetDestinationPath(FilePath path)
        {
            if (path?.IsRelative != false)
            {
                return path;
            }
            path = new DirectoryPath("/input").GetRelativePath(path);
            return path.IsRelative ? path : null;
        }

        // Test helpers

        [PropertyMetadata(null)]
        public string Content => ((IDocument)this).GetStringAsync().Result;

        [PropertyMetadata(null)]
        public TestMetadata TestMetadata => (TestMetadata)Metadata;

        public void Add(KeyValuePair<string, object> item) => TestMetadata.Add(item);

        public void Add(string key, object value) => TestMetadata.Add(key, value);
    }
}
