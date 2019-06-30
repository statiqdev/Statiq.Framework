using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Statiq.Common.Configuration;
using Statiq.Common.Content;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.IO;
using Statiq.Common.JavaScript;
using Statiq.Common.Meta;
using Statiq.Common.Modules;
using Statiq.Common.Shortcodes;
using Statiq.Core.Content;
using Statiq.Core.Documents;
using Statiq.Core.JavaScript;

namespace Statiq.Core.Execution
{
    internal class ExecutionContext : IExecutionContext
    {
        // Cache the HttpMessageHandler (the HttpClient is really just a thin wrapper around this)
        private static readonly HttpMessageHandler _httpMessageHandler = new HttpClientHandler();

        private readonly PipelinePhase _pipelinePhase;

        /// <inheritdoc/>
        public Engine Engine { get; }

        /// <inheritdoc/>
        public Guid ExecutionId { get; }

        /// <inheritdoc/>
        public IReadOnlyCollection<byte[]> DynamicAssemblies => Engine.DynamicAssemblies;

        /// <inheritdoc/>
        public IReadOnlyCollection<string> Namespaces => Engine.Namespaces;

        /// <inheritdoc/>
        public string PipelineName => _pipelinePhase.PipelineName;

        /// <inheritdoc/>
        public Phase Phase => _pipelinePhase.Phase;

        /// <inheritdoc/>
        public IModule Module { get; }

        /// <inheritdoc/>
        public IDocumentCollection Documents { get; }

        /// <inheritdoc/>
        public IReadOnlyFileSystem FileSystem => Engine.FileSystem;

        /// <inheritdoc/>
        public IReadOnlySettings Settings => Engine.Settings;

        /// <inheritdoc/>
        public IReadOnlyShortcodeCollection Shortcodes => Engine.Shortcodes;

        /// <inheritdoc/>
        public IServiceProvider Services { get; }

        /// <inheritdoc/>
        public string ApplicationInput => Engine.ApplicationInput;

        /// <inheritdoc/>
        public DocumentFactory DocumentFactory => Engine.DocumentFactory;

        /// <inheritdoc/>
        public IMemoryStreamFactory MemoryStreamFactory => Engine.MemoryStreamFactory;

        /// <inheritdoc/>
        public CancellationToken CancellationToken { get; }

        public ExecutionContext(Engine engine, Guid executionId, PipelinePhase pipelinePhase, IServiceProvider services, CancellationToken cancellationToken)
        {
            Engine = engine ?? throw new ArgumentNullException(nameof(engine));
            ExecutionId = executionId;
            _pipelinePhase = pipelinePhase ?? throw new ArgumentNullException(nameof(pipelinePhase));
            Services = services ?? throw new ArgumentNullException(nameof(services));
            Documents = new DocumentCollection(engine.Documents, pipelinePhase, engine.Pipelines);
            CancellationToken = cancellationToken;
        }

        private ExecutionContext(ExecutionContext original, IModule module)
        {
            Engine = original.Engine;
            ExecutionId = original.ExecutionId;
            _pipelinePhase = original._pipelinePhase;
            Services = original.Services;
            Documents = original.Documents;
            Module = module;
            CancellationToken = original.CancellationToken;
        }

        internal ExecutionContext Clone(IModule module) => new ExecutionContext(this, module);

        /// <inheritdoc/>
        public HttpClient CreateHttpClient() => CreateHttpClient(_httpMessageHandler);

        /// <inheritdoc/>
        public HttpClient CreateHttpClient(HttpMessageHandler handler)
        {
            HttpClient client = new HttpClient(handler, false)
            {
                Timeout = TimeSpan.FromSeconds(60)
            };
            client.DefaultRequestHeaders.Add("User-Agent", "Statiq");
            return client;
        }

        /// <inheritdoc/>
        public async Task<Stream> GetContentStreamAsync(string content = null)
        {
            if (this.Bool(Common.Meta.Keys.UseStringContentFiles))
            {
                // Use a temp file for strings
                IFile tempFile = await FileSystem.GetTempFileAsync();
                if (!string.IsNullOrEmpty(content))
                {
                    await tempFile.WriteAllTextAsync(content);
                }
                return new ContentStream(new FileContent(tempFile), await tempFile.OpenAsync(), true);
            }

            // Otherwise get a memory stream from the pool and use that
            Stream memoryStream = MemoryStreamFactory.GetStream(content);
            return new ContentStream(new Common.Content.StreamContent(MemoryStreamFactory, memoryStream), memoryStream, false);
        }

        /// <inheritdoc/>
        public async Task<ImmutableArray<IDocument>> ExecuteAsync(IEnumerable<IModule> modules, IEnumerable<IDocument> inputs) =>
            await Engine.ExecuteAsync(this, modules, inputs?.ToImmutableArray() ?? ImmutableArray<IDocument>.Empty);

        /// <inheritdoc/>
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

        // IMetadata

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => Settings.GetEnumerator();

        /// <inheritdoc/>
        public int Count => Settings.Count;

        /// <inheritdoc/>
        public bool ContainsKey(string key) => Settings.ContainsKey(key);

        /// <inheritdoc/>
        public object this[string key] => Settings[key];

        /// <inheritdoc/>
        public IEnumerable<string> Keys => Settings.Keys;

        /// <inheritdoc/>
        public IEnumerable<object> Values => Settings.Values;

        /// <inheritdoc/>
        public bool TryGetRaw(string key, out object value) => Settings.TryGetRaw(key, out value);

        /// <inheritdoc/>
        public bool TryGetValue(string key, out object value) => TryGetValue<object>(key, out value);

        /// <inheritdoc/>
        public bool TryGetValue<TValue>(string key, out TValue value) => Settings.TryGetValue(key, out value);

        /// <inheritdoc/>
        public IMetadata GetMetadata(params string[] keys) => Settings.GetMetadata(keys);
    }
}
