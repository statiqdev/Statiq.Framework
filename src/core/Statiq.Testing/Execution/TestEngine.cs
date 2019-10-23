using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Extensions.Configuration;
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
            _documentFactory = new DocumentFactory(Settings);
            _documentFactory.SetDefaultDocumentType<TestDocument>();
        }

        public TestConfigurationSettings Settings { get; } = new TestConfigurationSettings();

        /// <inheritdoc />
        public ApplicationState ApplicationState { get; set; }

        /// <inheritdoc />
        IReadOnlyConfigurationSettings IEngine.Settings => Settings;

        /// <inheritdoc />
        public IEventCollection Events { get; set; } = new TestEventCollection();

        /// <inheritdoc />
        public IServiceProvider Services { get; set; } = new TestServiceProvider();

        /// <inheritdoc />
        public IFileSystem FileSystem { get; set; } = new TestFileSystem();

        /// <inheritdoc />
        public IMemoryStreamFactory MemoryStreamFactory { get; set; } = new TestMemoryStreamFactory();

        /// <inheritdoc />
        public IPipelineCollection Pipelines => throw new NotImplementedException();

        /// <inheritdoc />
        public IShortcodeCollection Shortcodes => new TestShortcodeCollection();

        /// <inheritdoc />
        public INamespacesCollection Namespaces { get; set; } = new TestNamespacesCollection();

        /// <inheritdoc />
        public IRawAssemblyCollection DynamicAssemblies { get; set; } = new TestRawAssemblyCollection();

        /// <inheritdoc />
        public bool SerialExecution { get; set; }

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
