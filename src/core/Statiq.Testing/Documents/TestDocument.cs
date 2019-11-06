using System.Collections.Generic;
using System.IO;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestDocument : Document<TestDocument>
    {
        // The empty constructor should not call the base constructor
        // Otherwise the document will be initialized after empty construction and the factory will initialize it again
        public TestDocument()
        {
            Metadata = new TestMetadata();
            ContentProvider = new NullContent();
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

        public TestDocument(string content, string mediaType = null)
            : this(GetContentProvider(content, mediaType))
        {
        }

        public TestDocument(Stream content, string mediaType = null)
            : this(GetContentProvider(content, mediaType))
        {
        }

        public TestDocument(IEnumerable<KeyValuePair<string, object>> metadata, string content, string mediaType = null)
            : this(metadata, GetContentProvider(content, mediaType))
        {
        }

        public TestDocument(IEnumerable<KeyValuePair<string, object>> metadata, Stream content, string mediaType = null)
            : this(metadata, GetContentProvider(content, mediaType))
        {
        }

        public TestDocument(FilePath source, FilePath destination, IEnumerable<KeyValuePair<string, object>> metadata, string content, string mediaType = null)
            : this(source, destination, metadata, GetContentProvider(content, mediaType))
        {
        }

        public TestDocument(FilePath source, FilePath destination, IEnumerable<KeyValuePair<string, object>> metadata, Stream content, string mediaType = null)
            : this(source, destination, metadata, GetContentProvider(content, mediaType))
        {
        }

        public TestDocument(FilePath source, FilePath destination, string content, string mediaType = null)
            : this(source, destination, GetContentProvider(content, mediaType))
        {
        }

        public TestDocument(FilePath source, FilePath destination, Stream content, string mediaType = null)
            : this(source, destination, GetContentProvider(content, mediaType))
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

        public TestDocument(FilePath path, IEnumerable<KeyValuePair<string, object>> metadata, string content, string mediaType = null)
            : this(GetSourcePath(path), GetDestinationPath(path), metadata, GetContentProvider(content, mediaType))
        {
        }

        public TestDocument(FilePath path, IEnumerable<KeyValuePair<string, object>> metadata, Stream content, string mediaType = null)
            : this(GetSourcePath(path), GetDestinationPath(path), metadata, GetContentProvider(content, mediaType))
        {
        }

        public TestDocument(FilePath path, IEnumerable<KeyValuePair<string, object>> metadata, IContentProvider contentProvider)
            : this(GetSourcePath(path), GetDestinationPath(path), metadata, contentProvider)
        {
        }

        public TestDocument(FilePath path, string content, string mediaType = null)
            : this(GetSourcePath(path), GetDestinationPath(path), GetContentProvider(content, mediaType))
        {
        }

        public TestDocument(FilePath path, Stream content, string mediaType = null)
            : this(GetSourcePath(path), GetDestinationPath(path), GetContentProvider(content, mediaType))
        {
        }

        public TestDocument(FilePath path, IContentProvider contentProvider)
            : this(GetSourcePath(path), GetDestinationPath(path), contentProvider)
        {
        }

        // Constructor helpers

        private static IContentProvider GetContentProvider(string content, string mediaType)
        {
            TestMemoryStreamFactory memoryStreamFactory = new TestMemoryStreamFactory();
            return content == null ? (IContentProvider)new NullContent() : new StreamContent(memoryStreamFactory, memoryStreamFactory.GetStream(content), mediaType);
        }

        private static IContentProvider GetContentProvider(Stream content, string mediaType)
        {
            TestMemoryStreamFactory memoryStreamFactory = new TestMemoryStreamFactory();
            return content == null ? (IContentProvider)new NullContent() : new StreamContent(memoryStreamFactory, content, mediaType);
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
        public string Content => ((IDocument)this).GetContentStringAsync().Result;

        [PropertyMetadata(null)]
        public TestMetadata TestMetadata => (TestMetadata)Metadata;

        public void Add(KeyValuePair<string, object> item) => TestMetadata.Add(item);

        public void Add(string key, object value) => TestMetadata.Add(key, value);
    }
}
