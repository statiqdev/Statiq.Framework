using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Jint;
using JSPool;
using Microsoft.Extensions.DependencyInjection;
using Wyam.Common.Caching;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.JavaScript;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Shortcodes;
using Wyam.Common.Tracing;
using Wyam.Common.Util;
using Wyam.Core.Documents;
using Wyam.Core.JavaScript;
using Wyam.Core.Meta;
using Wyam.Core.Shortcodes;

namespace Wyam.Core.Execution
{
    internal class ExecutionContext : IExecutionContext, IDisposable
    {
        // Cache the HttpMessageHandler (the HttpClient is really just a thin wrapper around this)
        private static readonly HttpMessageHandler _httpMessageHandler = new HttpClientHandler();

        private readonly PipelinePhase _pipelinePhase;

        private bool _disposed;

        public Engine Engine { get; }

        public Guid ExecutionId { get; }

        public IReadOnlyCollection<byte[]> DynamicAssemblies => Engine.DynamicAssemblies;

        public IReadOnlyCollection<string> Namespaces => Engine.Namespaces;

        public string PipelineName => _pipelinePhase.PipelineName;

        public string PhaseName => _pipelinePhase.PhaseName;

        public IModule Module { get; }

        public IDocumentCollection Documents { get; }

        public IReadOnlyFileSystem FileSystem => Engine.FileSystem;

        public IReadOnlySettings Settings => Engine.Settings;

        public IReadOnlyShortcodeCollection Shortcodes => Engine.Shortcodes;

        public IExecutionCache ExecutionCache => Engine.ExecutionCacheManager.Get(Module, Settings);

        public IServiceProvider Services { get; }

        public string ApplicationInput => Engine.ApplicationInput;

        public ExecutionContext(Engine engine, Guid executionId, PipelinePhase pipelinePhase, IServiceProvider services)
        {
            Engine = engine ?? throw new ArgumentNullException(nameof(engine));
            ExecutionId = executionId;
            _pipelinePhase = pipelinePhase ?? throw new ArgumentNullException(nameof(pipelinePhase));
            Services = services ?? throw new ArgumentNullException(nameof(services));
            Documents = new DocumentCollection(pipelinePhase, engine);
        }

        private ExecutionContext(ExecutionContext original, IModule module)
        {
            Engine = original.Engine;
            ExecutionId = original.ExecutionId;
            _pipelinePhase = original._pipelinePhase;
            Services = original.Services;
            Module = module;
        }

        internal ExecutionContext Clone(IModule module) => new ExecutionContext(this, module);

        /// <summary>
        /// The context is disposed after use by each module to ensure modules aren't accessing stale data
        /// if they continue to create documents or perform other operations after the module is done
        /// executing. A disposed context can no longer be used.
        /// </summary>
        public void Dispose()
        {
            _disposed = true;
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ExecutionContext));
            }
        }

        public bool TryConvert<T>(object value, out T result) => TypeHelper.TryConvert(value, out result);

        public bool TryGetValue(string key, out object value) => TryGetValue<object>(key, out value);

        public async Task<Stream> GetContentStreamAsync(string content = null) => await Engine.ContentStreamFactory.GetStreamAsync(this, content);

        public HttpClient CreateHttpClient() => CreateHttpClient(_httpMessageHandler);

        public HttpClient CreateHttpClient(HttpMessageHandler handler) => new HttpClient(handler, false)
        {
            Timeout = TimeSpan.FromSeconds(60)
        };

        /// <inheritdoc/>
        public IDocument GetDocument(IDocument sourceDocument, FilePath source, Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true)
        {
            CheckDisposed();
            IDocument document = Engine.DocumentFactory.GetDocument(this, sourceDocument, source, stream, items, disposeStream);
            if (sourceDocument != null && sourceDocument.Source == null && source != null)
            {
                // Only add a new source if the source document didn't already contain one (otherwise the one it contains will be used)
                _pipelinePhase.AddDocumentSource(source);
            }
            _pipelinePhase.AddClonedDocument(document);
            return document;
        }

        public async Task<ImmutableArray<IDocument>> ExecuteAsync(IEnumerable<IModule> modules, IEnumerable<IDocument> inputs)
        {
            CheckDisposed();
            return await Engine.ExecuteAsync(this, modules, inputs?.ToImmutableArray() ?? ImmutableArray<IDocument>.Empty);
        }

        public IJavaScriptEnginePool GetJavaScriptEnginePool(
            Action<IJavaScriptEngine> initializer = null,
            int startEngines = 10,
            int maxEngines = 25,
            int maxUsagesPerEngine = 100,
            TimeSpan? engineTimeout = null) =>
            new JavaScriptEnginePool(
                initializer,
                startEngines,
                maxEngines,
                maxUsagesPerEngine,
                engineTimeout ?? TimeSpan.FromSeconds(5));

        public async Task<IShortcodeResult> GetShortcodeResultAsync(string content, IEnumerable<KeyValuePair<string, object>> metadata = null)
            => GetShortcodeResult(content == null ? null : await GetContentStreamAsync(content), metadata);

        public IShortcodeResult GetShortcodeResult(Stream content, IEnumerable<KeyValuePair<string, object>> metadata = null)
            => new ShortcodeResult(content, metadata);

        // IMetadata

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => Settings.GetEnumerator();

        public int Count => Settings.Count;

        public bool ContainsKey(string key) => Settings.ContainsKey(key);

        public object this[string key] => Settings[key];

        public IEnumerable<string> Keys => Settings.Keys;

        public IEnumerable<object> Values => Settings.Values;

        public object GetRaw(string key) => Settings.Get(key);

        public bool TryGetValue<T>(string key, out T value) => Settings.TryGetValue<T>(key, out value);

        public IMetadata GetMetadata(params string[] keys) => Settings.GetMetadata(keys);
    }
}
