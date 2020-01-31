using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
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

            TestLoggerProvider = new TestLoggerProvider(LogMessages);
            Services = new TestServiceProvider(
                serviceCollection =>
                {
                    serviceCollection.AddLogging();
                    serviceCollection.AddSingleton<ILoggerProvider>(TestLoggerProvider);
                    serviceCollection.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Trace);
                });
        }

        public TestLoggerProvider TestLoggerProvider { get; }

        public ConcurrentQueue<TestMessage> LogMessages { get; } = new ConcurrentQueue<TestMessage>();

        /// <inheritdoc />
        public Guid ExecutionId { get; set; } = Guid.Empty;

        /// <inheritdoc />
        public CancellationToken CancellationToken { get; set; }

        /// <inheritdoc />
        public ApplicationState ApplicationState { get; set; }

        /// <inheritdoc />
        IReadOnlyApplicationState IExecutionState.ApplicationState => ApplicationState;

        /// <inheritdoc />
        public TestConfigurationSettings Settings { get; set; } = new TestConfigurationSettings();

        /// <inheritdoc />
        IReadOnlyConfigurationSettings IExecutionState.Settings => Settings;

        /// <inheritdoc />
        public TestEventCollection Events { get; set; } = new TestEventCollection();

        /// <inheritdoc />
        IEventCollection IEngine.Events => Events;

        /// <inheritdoc />
        IReadOnlyEventCollection IExecutionState.Events => Events;

        /// <inheritdoc />
        public TestServiceProvider Services { get; set; }

        /// <inheritdoc />
        IServiceProvider IExecutionState.Services => Services;

        /// <inheritdoc />
        public TestFileSystem FileSystem { get; set; } = new TestFileSystem();

        /// <inheritdoc />
        IFileSystem IEngine.FileSystem => FileSystem;

        /// <inheritdoc />
        IReadOnlyFileSystem IExecutionState.FileSystem => FileSystem;

        /// <inheritdoc />
        public TestMemoryStreamFactory MemoryStreamFactory { get; set; } = new TestMemoryStreamFactory();

        /// <inheritdoc />
        IMemoryStreamFactory IExecutionState.MemoryStreamFactory => MemoryStreamFactory;

        /// <inheritdoc />
        public IPipelineCollection Pipelines => throw new NotImplementedException();

        /// <inheritdoc />
        public TestShortcodeCollection Shortcodes { get; set; } = new TestShortcodeCollection();

        /// <inheritdoc />
        IShortcodeCollection IEngine.Shortcodes => Shortcodes;

        /// <inheritdoc />
        IReadOnlyShortcodeCollection IExecutionState.Shortcodes => Shortcodes;

        /// <inheritdoc />
        public TestNamespacesCollection Namespaces { get; set; } = new TestNamespacesCollection();

        /// <inheritdoc />
        INamespacesCollection IExecutionState.Namespaces => Namespaces;

        /// <inheritdoc />
        public bool SerialExecution { get; set; }

        /// <inheritdoc/>
        public TestPipelineOutputs Outputs { get; set; } = new TestPipelineOutputs();

        /// <inheritdoc />
        IPipelineOutputs IExecutionState.Outputs => Outputs;

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
