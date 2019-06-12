using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Content;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.JavaScript;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Shortcodes;
using Wyam.Common.Util;
using Wyam.Testing.Configuration;
using Wyam.Testing.Documents;
using Wyam.Testing.IO;
using Wyam.Testing.Meta;
using Wyam.Testing.Shortcodes;

namespace Wyam.Testing.Execution
{
    /// <summary>
    /// An <see cref="IExecutionContext"/> that can be used for testing.
    /// </summary>
    public class TestExecutionContext : IExecutionContext, ITypeConversions
    {
        private readonly TestSettings _settings = new TestSettings();

        /// <inheritdoc/>
        public Guid ExecutionId { get; set; } = Guid.NewGuid();

        /// <inheritdoc/>
        public IReadOnlyCollection<byte[]> DynamicAssemblies { get; set; } = new List<byte[]>();

        /// <inheritdoc/>
        public IReadOnlyCollection<string> Namespaces { get; set; } = new List<string>();

        /// <inheritdoc/>
        public string PipelineName { get; set; }

        /// <inheritdoc/>
        public Phase Phase { get; set; } = Phase.Process;

        /// <inheritdoc/>
        public IModule Module { get; set; }

        /// <inheritdoc/>
        public IReadOnlyFileSystem FileSystem { get; set; } = new TestFileSystem();

        /// <inheritdoc/>
        public IDocumentCollection Documents { get; set; }

        /// <inheritdoc/>
        public IServiceProvider Services { get; set; } = new TestServiceProvider();

        /// <inheritdoc/>
        public string ApplicationInput { get; set; }

        /// <inheritdoc/>
        public ISettings Settings => _settings;

        IReadOnlySettings IExecutionContext.Settings => Settings;

        public IShortcodeCollection Shortcodes { get; set; } = new TestShortcodeCollection();

        IReadOnlyShortcodeCollection IExecutionContext.Shortcodes => Shortcodes;

        public IMemoryStreamFactory MemoryStreamFactory { get; set; } = new TestMemoryStreamFactory();

        /// <inheritdoc/>
        public Task<Stream> GetContentStreamAsync(string content = null) => Task.FromResult<Stream>(new ContentStream(content));

        private class ContentStream : DelegatingStream
        {
            public ContentStream(string content)
                : base(string.IsNullOrEmpty(content) ? new MemoryStream() : new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
            }
        }

        /// <inheritdoc/>
        public async Task<IContentProvider> GetContentProviderAsync(object content)
        {
            await Task.CompletedTask;

            switch (content)
            {
                case null:
                    return null;
                case IContentProvider contentProvider:
                    return contentProvider;
                case ContentStream contentStream:
                    return new Common.Content.StreamContent(MemoryStreamFactory, contentStream, false);
                case Stream stream:
                    return new Common.Content.StreamContent(MemoryStreamFactory, stream);
                case IFile file:
                    return new FileContent(file);
                case TestDocument document:
                    return document.ContentProvider;
            }

            string contentString = content as string ?? content.ToString();
            return string.IsNullOrEmpty(contentString)
                ? null
                : new Common.Content.StreamContent(MemoryStreamFactory, MemoryStreamFactory.GetStream(contentString));
        }

        /// <inheritdoc/>
        public IDocument GetDocument(
            IDocument originalDocument,
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> metadata,
            IContentProvider contentProvider = null)
        {
            TestDocument document = originalDocument == null
                ? new TestDocument(
                    source,
                    destination,
                    metadata,
                    contentProvider)
                : new TestDocument(
                    originalDocument.Source ?? source,
                    destination ?? originalDocument.Destination,
                    metadata == null ? originalDocument : originalDocument.Concat(metadata),
                    contentProvider == null ? ((TestDocument)originalDocument).ContentProvider : contentProvider);
            if (originalDocument != null)
            {
                document.Id = originalDocument.Id;
            }
            return document;
        }

        /// <inheritdoc/>
        public bool Untrack(IDocument document) => false;

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

        // Includes some initial common conversions
        public Dictionary<(Type Value, Type Result), Func<object, object>> TypeConversions { get; } = new Dictionary<(Type Value, Type Result), Func<object, object>>(DefaultTypeConversions);

        public static Dictionary<(Type Value, Type Result), Func<object, object>> DefaultTypeConversions { get; } =
            new Dictionary<(Type Value, Type Result), Func<object, object>>
            {
                { (typeof(string), typeof(bool)), x => bool.Parse((string)x) },
                { (typeof(string), typeof(FilePath)), x => new FilePath((string)x) },
                { (typeof(FilePath), typeof(string)), x => ((FilePath)x).FullPath },
                { (typeof(string), typeof(DirectoryPath)), x => new DirectoryPath((string)x) },
                { (typeof(DirectoryPath), typeof(string)), x => ((DirectoryPath)x).FullPath },
                { (typeof(string), typeof(Uri)), x => new Uri((string)x) },
                { (typeof(Uri), typeof(string)), x => ((Uri)x).ToString() }
            };

        public void AddTypeConversion<T, TResult>(Func<T, TResult> typeConversion) => TypeConversions.Add((typeof(T), typeof(TResult)), x => typeConversion((T)x));

        /// <inheritdoc/>
        public bool TryConvert<T>(object value, out T result)
        {
            // Check if there's a test-specific conversion
            if (TypeConversions.TryGetValue((value?.GetType() ?? typeof(object), typeof(T)), out Func<object, object> typeConversion))
            {
                result = (T)typeConversion(value);
                return true;
            }

            // Default conversion is just to cast
            if (value is T)
            {
                result = (T)value;
                return true;
            }

            result = default;
            return value == null;
        }

        /// <inheritdoc/>
        public async Task<ImmutableArray<IDocument>> ExecuteAsync(IEnumerable<IModule> modules, IEnumerable<IDocument> inputs)
        {
            if (modules == null)
            {
                return ImmutableArray<IDocument>.Empty;
            }
            foreach (IModule module in modules)
            {
                inputs = await module.ExecuteAsync(inputs.ToList(), this);
            }
            return inputs.ToImmutableArray();
        }

        public Func<IJavaScriptEngine> JsEngineFunc { get; set; } = () =>
            throw new NotImplementedException("JavaScript test engine not initialized. Wyam.Testing.JavaScript can be used to return a working JavaScript engine");

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
        public object GetRaw(string key) => _settings.GetRaw(key);

        /// <inheritdoc/>
        public bool TryGetValue<T>(string key, out T value) => _settings.TryGetValue<T>(key, out value);

        /// <inheritdoc />
        public bool TryGetValue(string key, out object value) => TryGetValue<object>(key, out value);

        /// <inheritdoc />
        public IMetadata GetMetadata(params string[] keys) => _settings.GetMetadata(keys);
    }
}
