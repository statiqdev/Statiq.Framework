using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Statiq.Common;

namespace Statiq.Testing
{
    /// <summary>
    /// An <see cref="IExecutionContext"/> that can be used for testing.
    /// </summary>
    public class TestExecutionContext : IExecutionContext
    {
        private readonly TestSettings _settings = new TestSettings();
        private readonly DocumentFactory _documentFactory;
        private readonly ILogger _logger;

        public TestExecutionContext()
            : this((IEnumerable<IDocument>)null)
        {
        }

        public TestExecutionContext(params IDocument[] inputs)
            : this((IEnumerable<IDocument>)inputs)
        {
        }

        public TestExecutionContext(IEnumerable<IDocument> inputs)
        {
            _documentFactory = new DocumentFactory(_settings);
            _documentFactory.SetDefaultDocumentType<TestDocument>();
            if (inputs != null)
            {
                SetInputs(inputs);
            }

            TestLoggerProvider = new TestLoggerProvider(LogMessages);
            Services = new TestServiceProvider(
                serviceCollection =>
                {
                    serviceCollection.AddLogging();
                    serviceCollection.AddSingleton<ILoggerProvider>(TestLoggerProvider);
                    serviceCollection.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Trace);
                });
            _logger = TestLoggerProvider.CreateLogger(null);
        }

        public IServiceProvider Services { get; set; }

        public TestLoggerProvider TestLoggerProvider { get; }

        public ConcurrentQueue<TestMessage> LogMessages { get; } = new ConcurrentQueue<TestMessage>();

        public IShortcodeCollection Shortcodes { get; set; } = new TestShortcodeCollection();

        public ISettings Settings => _settings;

        // IExecutionContext

        /// <inheritdoc/>
        public Guid ExecutionId { get; set; } = Guid.NewGuid();

        /// <inheritdoc/>
        public IRawAssemblyCollection DynamicAssemblies { get; set; } = new TestRawAssemblyCollection();

        /// <inheritdoc/>
        public INamespacesCollection Namespaces { get; set; } = new TestNamespacesCollection();

        /// <inheritdoc/>
        public string PipelineName { get; set; }

        /// <inheritdoc/>
        public Phase Phase { get; set; } = Phase.Process;

        /// <inheritdoc />
        public IReadOnlyEventCollection Events { get; set; } = new TestEventCollection();

        /// <inheritdoc/>
        public IReadOnlyFileSystem FileSystem { get; set; } = new TestFileSystem();

        /// <inheritdoc/>
        public IPipelineOutputs Outputs { get; set; }

        /// <inheritdoc/>
        public IReadOnlyApplicationState ApplicationState { get; set; }

        /// <inheritdoc/>
        public bool SerialExecution { get; set; }

        /// <inheritdoc/>
        IReadOnlySettings IExecutionContext.Settings => Settings;

        /// <inheritdoc/>
        IReadOnlyShortcodeCollection IExecutionContext.Shortcodes => Shortcodes;

        /// <inheritdoc/>
        public IMemoryStreamFactory MemoryStreamFactory { get; set; } = new TestMemoryStreamFactory();

        /// <inheritdoc/>
        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;

        /// <inheritdoc/>
        public IExecutionContext Parent { get; set; }

        /// <inheritdoc/>
        public IModule Module { get; set; }

        /// <inheritdoc/>
        public ImmutableArray<IDocument> Inputs { get; set; } = ImmutableArray<IDocument>.Empty;

        public void SetInputs(IEnumerable<IDocument> inputs) =>
            Inputs = inputs?.Where(x => x != null).ToImmutableArray() ?? ImmutableArray<IDocument>.Empty;

        public void SetInputs(params IDocument[] inputs) =>
            SetInputs((IEnumerable<IDocument>)inputs);

        /// <inheritdoc/>
        public Task<Stream> GetContentStreamAsync(string content = null) => Task.FromResult<Stream>(new TestContentStream(this, content));

        private class TestContentStream : DelegatingStream, IContentProviderFactory
        {
            private readonly TestExecutionContext _context;

            public TestContentStream(TestExecutionContext context, string content)
                : base(string.IsNullOrEmpty(content) ? new MemoryStream() : new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                _context = context;
            }

            public IContentProvider GetContentProvider() => new Common.StreamContent(_context.MemoryStreamFactory, this);
        }

        /// <inheritdoc/>
        public HttpClient CreateHttpClient() =>
            new HttpClient(new TestHttpMessageHandler(HttpResponseFunc, null));

        /// <inheritdoc/>
        public HttpClient CreateHttpClient(HttpMessageHandler handler) =>
            new HttpClient(new TestHttpMessageHandler(HttpResponseFunc, handler));

        /// <summary>
        /// A message handler that should be used to register <see cref="HttpResponseMessage"/>
        /// instances for a given request.
        /// </summary>
        public Func<HttpRequestMessage, HttpMessageHandler, HttpResponseMessage> HttpResponseFunc { get; set; }
            = (_, __) => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(string.Empty)
            };

        /// <inheritdoc/>
        public async Task<ImmutableArray<IDocument>> ExecuteModulesAsync(IEnumerable<IModule> modules, IEnumerable<IDocument> inputs)
        {
            if (modules == null)
            {
                return ImmutableArray<IDocument>.Empty;
            }
            foreach (IModule module in modules)
            {
                // We need a new context for each module so just do a member-wise clone of this one and set module and documents
                TestExecutionContext moduleContext = (TestExecutionContext)MemberwiseClone();
                moduleContext.SetInputs(inputs);
                moduleContext.Module = module;
                moduleContext.Parent = this;
                inputs = await module.ExecuteAsync(moduleContext);
            }
            return inputs?.Where(x => x != null).ToImmutableArray() ?? ImmutableArray<IDocument>.Empty;
        }

        public Func<IJavaScriptEngine> JsEngineFunc { get; set; } = () =>
            throw new NotImplementedException("JavaScript test engine not initialized. Statiq.Testing.JavaScript can be used to return a working JavaScript engine");

        /// <inheritdoc/>
        public IJavaScriptEnginePool GetJavaScriptEnginePool(
            Action<IJavaScriptEngine> initializer = null,
            int startEngines = 10,
            int maxEngines = 25,
            int maxUsagesPerEngine = 100,
            TimeSpan? engineTimeout = null) =>
            new TestJsEnginePool(JsEngineFunc, initializer);

        private class TestJsEnginePool : IJavaScriptEnginePool
        {
            private readonly Func<IJavaScriptEngine> _engineFunc;
            private readonly Action<IJavaScriptEngine> _initializer;

            public TestJsEnginePool(Func<IJavaScriptEngine> engineFunc, Action<IJavaScriptEngine> initializer)
            {
                _engineFunc = engineFunc;
                _initializer = initializer;
            }

            public IJavaScriptEngine GetEngine(TimeSpan? timeout = null)
            {
                IJavaScriptEngine engine = _engineFunc();
                _initializer?.Invoke(engine);
                return engine;
            }

            public void Dispose()
            {
            }

            public void RecycleEngine(IJavaScriptEngine engine)
            {
                throw new NotImplementedException();
            }

            public void RecycleAllEngines()
            {
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc />
        public void SetDefaultDocumentType<TDocument>()
            where TDocument : FactoryDocument, IDocument, new() =>
            _documentFactory.SetDefaultDocumentType<TDocument>();

        // IDocumentFactory

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

        // IServiceProvider

        public object GetService(Type serviceType) => Services.GetService(serviceType);

        // ILogger

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) =>
            _logger.Log(logLevel, eventId, state, exception, formatter);

        public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

        public IDisposable BeginScope<TState>(TState state) => _logger.BeginScope(state);
    }
}
