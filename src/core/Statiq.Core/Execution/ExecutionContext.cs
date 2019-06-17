using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Net.Http;
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
using Statiq.Core.Meta;

namespace Statiq.Core.Execution
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

        public Phase Phase => _pipelinePhase.Phase;

        public IModule Module { get; }

        public IDocumentCollection Documents { get; }

        public IReadOnlyFileSystem FileSystem => Engine.FileSystem;

        public IReadOnlySettings Settings => Engine.Settings;

        public IReadOnlyShortcodeCollection Shortcodes => Engine.Shortcodes;

        public IServiceProvider Services { get; }

        public string ApplicationInput => Engine.ApplicationInput;

        public IMemoryStreamFactory MemoryStreamFactory => Engine.MemoryStreamManager;

        public ExecutionContext(Engine engine, Guid executionId, PipelinePhase pipelinePhase, IServiceProvider services)
        {
            Engine = engine ?? throw new ArgumentNullException(nameof(engine));
            ExecutionId = executionId;
            _pipelinePhase = pipelinePhase ?? throw new ArgumentNullException(nameof(pipelinePhase));
            Services = services ?? throw new ArgumentNullException(nameof(services));
            Documents = new DocumentCollection(engine.Documents, pipelinePhase, engine.Pipelines);
        }

        private ExecutionContext(ExecutionContext original, IModule module)
        {
            Engine = original.Engine;
            ExecutionId = original.ExecutionId;
            _pipelinePhase = original._pipelinePhase;
            Services = original.Services;
            Documents = original.Documents;
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

        public HttpClient CreateHttpClient() => CreateHttpClient(_httpMessageHandler);

        public HttpClient CreateHttpClient(HttpMessageHandler handler) => new HttpClient(handler, false)
        {
            Timeout = TimeSpan.FromSeconds(60)
        };

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
                return new ContentStream(new TempFileContent(tempFile), await tempFile.OpenAsync(), true);
            }

            // Otherwise get a memory stream from the pool and use that
            Stream memoryStream = MemoryStreamFactory.GetStream(content);
            return new ContentStream(new Common.Content.StreamContent(MemoryStreamFactory, memoryStream), memoryStream, false);
        }

        /// <inheritdoc/>
        public async Task<IContentProvider> GetContentProviderAsync(object content)
        {
            switch (content)
            {
                case null:
                    return null;
                case IContentProvider contentProvider:
                    return contentProvider;
                case ContentStream contentStream:
                    return contentStream.GetContentProvider();  // This will also dispose the writable stream
                case Stream stream:
                    return new Common.Content.StreamContent(MemoryStreamFactory, stream);
                case IFile file:
                    return new FileContent(file);
                case Document document:
                    return document.ContentProvider;
            }

            // This wasn't one of the known content types, so treat it as a string
            string contentString = content as string ?? content.ToString();

            if (string.IsNullOrEmpty(contentString))
            {
                return null;
            }

            if (this.Bool(Common.Meta.Keys.UseStringContentFiles))
            {
                // Use a temp file for strings
                IFile tempFile = await FileSystem.GetTempFileAsync();
                if (!string.IsNullOrEmpty(contentString))
                {
                    await tempFile.WriteAllTextAsync(contentString);
                }
                return new TempFileContent(tempFile);
            }

            // Otherwise get a memory stream from the pool and use that
            return new Common.Content.StreamContent(MemoryStreamFactory, MemoryStreamFactory.GetStream(contentString));
        }

        /// <inheritdoc/>
        public IDocument GetDocument(
            IDocument originalDocument,
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> metadata,
            IContentProvider contentProvider = null)
        {
            CheckDisposed();
            IDocument document = Engine.DocumentFactory.GetDocument(this, originalDocument, source, destination, metadata, contentProvider);
            if (originalDocument != null && originalDocument.Source == null && source != null)
            {
                // Only add a new source if the source document didn't already contain one (otherwise the one it contains will be used)
                _pipelinePhase.AddDocumentSource(source);
            }
            _pipelinePhase.AddClonedDocument(document);
            return document;
        }

        /// <inheritdoc/>
        public bool Untrack(IDocument document) => _pipelinePhase.Untrack(document);

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
