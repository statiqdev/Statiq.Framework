using System;
using System.Collections.Generic;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestEngine : IEngine
    {
        public TestEngine()
        {
            _documentFactory = new DocumentFactory(_settings);
            _documentFactory.SetDefaultDocumentType<TestDocument>();
        }

        private readonly TestSettings _settings = new TestSettings();

        public ISettings Settings => _settings;

        public IFileSystem FileSystem { get; set; } = new TestFileSystem();

        public IMemoryStreamFactory MemoryStreamFactory { get; set; } = new TestMemoryStreamFactory();

        public string ApplicationInput { get; set; }

        public IPipelineCollection Pipelines => throw new NotImplementedException();

        public IShortcodeCollection Shortcodes => new TestShortcodeCollection();

        public INamespacesCollection Namespaces => throw new NotImplementedException();

        public IRawAssemblyCollection DynamicAssemblies => throw new NotImplementedException();

        private readonly DocumentFactory _documentFactory;

        /// <inheritdoc />
        public void SetDefaultDocumentType<TDocument>()
            where TDocument : FactoryDocument, IDocument, new() =>
            _documentFactory.SetDefaultDocumentType<TDocument>();

        /// <inheritdoc />
        public IDocument CreateDocument(
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            _documentFactory.CreateDocument(source, destination, items, contentProvider);

        /// <inheritdoc />
        public TDocument CreateDocument<TDocument>(
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            _documentFactory.CreateDocument<TDocument>(source, destination, items, contentProvider);
    }
}
