using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        public TestExecutionContext()
        {
            _documentFactory = new DocumentFactory(_settings);
            _documentFactory.SetDefaultDocumentType<TestDocument>();
            Inputs = ImmutableArray<IDocument>.Empty;
        }

        public TestExecutionContext(IEnumerable<IDocument> inputs)
        {
            _documentFactory = new DocumentFactory(_settings);
            _documentFactory.SetDefaultDocumentType<TestDocument>();
            SetInputs(inputs);
        }

        public TestExecutionContext(params IDocument[] inputs)
            : this((IEnumerable<IDocument>)inputs)
        {
        }

        /// <inheritdoc/>
        public Guid ExecutionId { get; set; } = Guid.NewGuid();

        /// <inheritdoc/>
        public IReadOnlyCollection<byte[]> DynamicAssemblies { get; set; } = new List<byte[]>();

        /// <inheritdoc/>
        public IReadOnlyCollection<string> Namespaces { get; set; } =
            typeof(IEngine).Assembly.GetTypes()
                .Where(x => x.IsPublic)
                .Select(x => x.Namespace)
                .Distinct()
                .Concat(new[]
                {
                    "System",
                    "System.Threading.Tasks",
                    "System.Collections.Generic",
                    "System.Linq"
                })
                .ToList();

        /// <inheritdoc/>
        public string PipelineName { get; set; }

        /// <inheritdoc/>
        public Phase Phase { get; set; } = Phase.Process;

        /// <inheritdoc/>
        public IReadOnlyFileSystem FileSystem { get; set; } = new TestFileSystem();

        /// <inheritdoc/>
        public IPipelineOutputs Outputs { get; set; }

        /// <inheritdoc/>
        public IServiceProvider Services { get; set; } = new TestServiceProvider();

        /// <inheritdoc/>
        public string ApplicationInput { get; set; }

        /// <inheritdoc/>
        public ISettings Settings => _settings;

        /// <inheritdoc/>
        IReadOnlySettings IExecutionContext.Settings => Settings;

        /// <inheritdoc/>
        public IShortcodeCollection Shortcodes { get; set; } = new TestShortcodeCollection();

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
        public ImmutableArray<IDocument> Inputs { get; set; }

        /// <inheritdoc/>
        public ILogger Logger { get; set; } = NullLogger.Instance;

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

        // IMetadata

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _settings.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_settings).GetEnumerator();
        }

        /// <inheritdoc/>
        public int Count => _settings.Count;

        /// <inheritdoc/>
        public bool ContainsKey(string key) => _settings.ContainsKey(key);

        /// <inheritdoc/>
        public object this[string key] => _settings[key];

        /// <inheritdoc/>
        public IEnumerable<string> Keys => _settings.Keys;

        /// <inheritdoc/>
        public IEnumerable<object> Values => _settings.Values;

        /// <inheritdoc/>
        public bool TryGetRaw(string key, out object value) => _settings.TryGetRaw(key, out value);

        /// <inheritdoc/>
        public bool TryGetValue<TValue>(string key, out TValue value) => _settings.TryGetValue<TValue>(key, out value);

        /// <inheritdoc />
        public bool TryGetValue(string key, out object value) => TryGetValue<object>(key, out value);

        /// <inheritdoc />
        public IMetadata GetMetadata(params string[] keys) => _settings.GetMetadata(keys);
    }
}
