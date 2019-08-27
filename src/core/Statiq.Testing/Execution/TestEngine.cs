using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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

        /// <inheritdoc />
        public IServiceProvider Services { get; set; } =
            new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();

        private readonly TestSettings _settings = new TestSettings();

        /// <inheritdoc />
        public ISettings Settings => _settings;

        /// <inheritdoc />
        public IFileSystem FileSystem { get; set; } = new TestFileSystem();

        /// <inheritdoc />
        public IMemoryStreamFactory MemoryStreamFactory { get; set; } = new TestMemoryStreamFactory();

        /// <inheritdoc />
        public string ApplicationInput { get; set; }

        /// <inheritdoc />
        public IPipelineCollection Pipelines => throw new NotImplementedException();

        /// <inheritdoc />
        public IShortcodeCollection Shortcodes => new TestShortcodeCollection();

        /// <inheritdoc />
        public INamespacesCollection Namespaces => throw new NotImplementedException();

        /// <inheritdoc />
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
