using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    internal class AnalyzerContext : IAnalyzerContext
    {
        // Cache analyzer overrides per document
        private static readonly ConcurrentCache<IDocument, IReadOnlyDictionary<string, LogLevel>> _overrideCache =
            new ConcurrentCache<IDocument, IReadOnlyDictionary<string, LogLevel>>();

        private readonly Engine _engine;
        private readonly PipelinePhase _pipelinePhase;
        private readonly string _analyzerName;
        private readonly LogLevel _logLevel;
        private readonly ConcurrentBag<AnalyzerResult> _results;

        internal AnalyzerContext(Engine engine, PipelinePhase pipelinePhase, KeyValuePair<string, IAnalyzer> analyzerItem, ConcurrentBag<AnalyzerResult> results)
        {
            _engine = engine;
            _pipelinePhase = pipelinePhase;
            _analyzerName = analyzerItem.Key;
            _logLevel = analyzerItem.Value.LogLevel;
            _results = results;
        }

        /// <inheritdoc/>
        public void Add(IDocument document, string message)
        {
            // Check if the document contains an override for this analyzer
            LogLevel logLevel = _logLevel;
            if (document is object && document.ContainsKey(Keys.Analyzers))
            {
                IReadOnlyDictionary<string, LogLevel> overrides = _overrideCache.GetOrAdd(
                    document,
                    doc => SettingsParser.Parse(doc.GetList(Keys.Analyzers, Array.Empty<string>())).ToDictionary(
                        x => x.Key,
                        x =>
                        {
                            if (x.Value.Equals("true", StringComparison.OrdinalIgnoreCase))
                            {
                                throw new Exception($"Analyzer override log level must be provided as \"[analyzer]=[log level]\" for {_analyzerName} in document {document.ToDisplayString()}");
                            }
                            if (!Enum.TryParse(x.Value, out LogLevel documentLevel))
                            {
                                throw new Exception($"Analyzer override log level {x.Value} for {_analyzerName} in document {document.ToDisplayString()} is invalid");
                            }
                            return documentLevel;
                        },
                        StringComparer.OrdinalIgnoreCase));
                if (overrides.TryGetValue(_analyzerName, out LogLevel overrideLevel) || overrides.TryGetValue("All", out overrideLevel))
                {
                    logLevel = overrideLevel;
                }
            }

            // Log the message
            if (logLevel != LogLevel.None)
            {
                _results.Add(new AnalyzerResult(_analyzerName, logLevel, document, message));
            }
        }

        /// <inheritdoc/>
        public IReadOnlyPipeline Pipeline => _pipelinePhase.Pipeline;

        /// <inheritdoc/>
        public string PipelineName => _pipelinePhase.PipelineName;

        /// <inheritdoc/>
        public Phase Phase => _pipelinePhase.Phase;

        /// <inheritdoc/>
        public IExecutionState ExecutionState => _engine;

        /// <inheritdoc/>
        public Guid ExecutionId => _engine.ExecutionId;

        /// <inheritdoc/>
        public CancellationToken CancellationToken => _engine.CancellationToken;

        /// <inheritdoc/>
        public IReadOnlyApplicationState ApplicationState => _engine.ApplicationState;

        /// <inheritdoc/>
        public ClassCatalog ClassCatalog => _engine.ClassCatalog;

        /// <inheritdoc/>
        public bool SerialExecution => _engine.SerialExecution;

        /// <inheritdoc/>
        public IReadOnlyEventCollection Events => _engine.Events;

        /// <inheritdoc/>
        public IReadOnlyFileSystem FileSystem => _engine.FileSystem;

        /// <inheritdoc/>
        public IReadOnlySettings Settings => _engine.Settings;

        /// <inheritdoc/>
        public IReadOnlyShortcodeCollection Shortcodes => _engine.Shortcodes;

        /// <inheritdoc/>
        public INamespacesCollection Namespaces => _engine.Namespaces;

        /// <inheritdoc/>
        public IMemoryStreamFactory MemoryStreamFactory => _engine.MemoryStreamFactory;

        /// <inheritdoc/>
        public IPipelineOutputs Outputs => _engine.Outputs;

        /// <inheritdoc/>
        public FilteredDocumentList<IDocument> OutputPages => _engine.OutputPages;

        /// <inheritdoc/>
        public IServiceProvider Services => _engine.Services;

        /// <inheritdoc/>
        public ILogger Logger => _engine.Logger;

        /// <inheritdoc/>
        public IScriptHelper ScriptHelper => _engine.ScriptHelper;

        /// <inheritdoc/>
        public IReadOnlyPipelineCollection Pipelines => _engine.Pipelines;

        /// <inheritdoc/>
        public IReadOnlyPipelineCollection ExecutingPipelines => _engine.ExecutingPipelines;

        /// <inheritdoc/>
        public Task<Stream> GetContentStreamAsync(string content = null) => _engine.GetContentStreamAsync(content);

        /// <inheritdoc/>
        public HttpClient CreateHttpClient() => _engine.CreateHttpClient();

        /// <inheritdoc/>
        public HttpClient CreateHttpClient(HttpMessageHandler handler) => _engine.CreateHttpClient(handler);

        /// <inheritdoc/>
        public async Task<HttpResponseMessage> SendHttpRequestWithRetryAsync(Func<HttpRequestMessage> requestFactory) =>
            await _engine.SendHttpRequestWithRetryAsync(requestFactory);

        /// <inheritdoc/>
        public IJavaScriptEnginePool GetJavaScriptEnginePool(
            Action<IJavaScriptEngine> initializer = null,
            int startEngines = 10,
            int maxEngines = 25,
            int maxUsagesPerEngine = 100,
            TimeSpan? engineTimeout = null) =>
            _engine.GetJavaScriptEnginePool(initializer, startEngines, maxEngines, maxUsagesPerEngine, engineTimeout);

        /// <inheritdoc/>
        public IDocument CreateDocument(
            NormalizedPath source,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            _engine.CreateDocument(source, destination, items, contentProvider);

        /// <inheritdoc/>
        public TDocument CreateDocument<TDocument>(
            NormalizedPath source,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            _engine.CreateDocument<TDocument>(source, destination, items, contentProvider);
    }
}
