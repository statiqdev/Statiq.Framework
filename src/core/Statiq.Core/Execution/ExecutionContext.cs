using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    internal class ExecutionContext : IExecutionContext
    {
        // Cache the HttpMessageHandler (the HttpClient is really just a thin wrapper around this)
        private static readonly HttpMessageHandler _httpMessageHandler = new HttpClientHandler();

        private readonly ExecutionContextData _contextData;

        /// <inheritdoc/>
        public Engine Engine => _contextData.Engine;

        /// <inheritdoc/>
        public Guid ExecutionId => _contextData.ExecutionId;

        /// <inheritdoc/>
        public IReadOnlyCollection<byte[]> DynamicAssemblies => _contextData.Engine.DynamicAssemblies;

        /// <inheritdoc/>
        public IReadOnlyCollection<string> Namespaces => _contextData.Engine.Namespaces;

        /// <inheritdoc/>
        public string PipelineName => _contextData.PipelinePhase.PipelineName;

        /// <inheritdoc/>
        public Phase Phase => _contextData.PipelinePhase.Phase;

        /// <inheritdoc/>
        public IPipelineOutputs Outputs => _contextData.Outputs;

        /// <inheritdoc/>
        public IReadOnlyFileSystem FileSystem => _contextData.Engine.FileSystem;

        /// <inheritdoc/>
        public IReadOnlySettings Settings => _contextData.Engine.Settings;

        /// <inheritdoc/>
        public IReadOnlyShortcodeCollection Shortcodes => _contextData.Engine.Shortcodes;

        /// <inheritdoc/>
        public IServiceProvider Services => _contextData.Services;

        /// <inheritdoc/>
        public string ApplicationInput => _contextData.Engine.ApplicationInput;

        /// <inheritdoc/>
        public DocumentFactory DocumentFactory => _contextData.Engine.DocumentFactory;

        /// <inheritdoc/>
        public IMemoryStreamFactory MemoryStreamFactory => _contextData.Engine.MemoryStreamFactory;

        /// <inheritdoc/>
        public CancellationToken CancellationToken => _contextData.CancellationToken;

        /// <inheritdoc/>
        public IExecutionContext Parent { get; }

        /// <inheritdoc/>
        public IModule Module { get; }

        /// <inheritdoc/>
        public ImmutableArray<IDocument> Inputs { get; }

        internal ExecutionContext(ExecutionContextData contextData, IExecutionContext parent, IModule module, ImmutableArray<IDocument> inputs)
        {
            _contextData = contextData ?? throw new ArgumentNullException(nameof(contextData));
            Parent = parent;
            Module = module ?? throw new ArgumentNullException(nameof(module));
            Inputs = inputs;
        }

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
            if (this.Bool(Common.Keys.UseStringContentFiles))
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
            return new ContentStream(new Common.StreamContent(MemoryStreamFactory, memoryStream), memoryStream, false);
        }

        /// <inheritdoc/>
        public async Task<ImmutableArray<IDocument>> ExecuteAsync(IEnumerable<IModule> modules, IEnumerable<IDocument> inputs) =>
            await Engine.ExecuteAsync(_contextData, this, modules, inputs?.ToImmutableArray() ?? ImmutableArray<IDocument>.Empty);

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
