using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    /// <summary>
    /// Wraps the original <see cref="IAnalyzerContext"/> and provides the current document when a null document is used for adding a message.
    /// </summary>
    internal class DocumentAnalyzerContext : IAnalyzerContext
    {
        private readonly IAnalyzerContext _context;
        private readonly IDocument _document;

        internal DocumentAnalyzerContext(IAnalyzerContext context, IDocument document)
        {
            _context = context;
            _document = document;
        }

        // If the document is null, use the current document
        public void Add(IDocument document, string message) => _context.Add(document ?? _document, message);

        public IExecutionState ExecutionState => _context.ExecutionState;

        public string PipelineName => _context.PipelineName;

        public IReadOnlyPipeline Pipeline => _context.Pipeline;

        public Phase Phase => _context.Phase;

        public Guid ExecutionId => _context.ExecutionId;

        public CancellationToken CancellationToken => _context.CancellationToken;

        public IReadOnlyApplicationState ApplicationState => _context.ApplicationState;

        public ClassCatalog ClassCatalog => _context.ClassCatalog;

        public bool SerialExecution => _context.SerialExecution;

        public IReadOnlyEventCollection Events => _context.Events;

        public IReadOnlyFileSystem FileSystem => _context.FileSystem;

        public IReadOnlySettings Settings => _context.Settings;

        public IReadOnlyShortcodeCollection Shortcodes => _context.Shortcodes;

        public INamespacesCollection Namespaces => _context.Namespaces;

        public IMemoryStreamFactory MemoryStreamFactory => _context.MemoryStreamFactory;

        public IPipelineOutputs Outputs => _context.Outputs;

        public FilteredDocumentList<IDocument> OutputPages => _context.OutputPages;

        public IServiceProvider Services => _context.Services;

        public ILogger Logger => _context.Logger;

        public IScriptHelper ScriptHelper => _context.ScriptHelper;

        public IReadOnlyPipelineCollection Pipelines => _context.Pipelines;

        public IReadOnlyPipelineCollection ExecutingPipelines => _context.ExecutingPipelines;

        public IDocument CreateDocument(NormalizedPath source, NormalizedPath destination, IEnumerable<KeyValuePair<string, object>> items, IContentProvider contentProvider = null) =>
            _context.CreateDocument(source, destination, items, contentProvider);

        public TDocument CreateDocument<TDocument>(NormalizedPath source, NormalizedPath destination, IEnumerable<KeyValuePair<string, object>> items, IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            _context.CreateDocument<TDocument>(source, destination, items, contentProvider);

        public HttpClient CreateHttpClient() => _context.CreateHttpClient();

        public HttpClient CreateHttpClient(HttpMessageHandler handler) => _context.CreateHttpClient(handler);

        public Task<Stream> GetContentStreamAsync(string content = null) => _context.GetContentStreamAsync(content);

        public IJavaScriptEnginePool GetJavaScriptEnginePool(Action<IJavaScriptEngine> initializer = null, int startEngines = 10, int maxEngines = 25, int maxUsagesPerEngine = 100, TimeSpan? engineTimeout = null) =>
            _context.GetJavaScriptEnginePool(initializer, startEngines, maxEngines, maxUsagesPerEngine, engineTimeout);

        public Task<HttpResponseMessage> SendHttpRequestWithRetryAsync(Func<HttpRequestMessage> requestFactory) => _context.SendHttpRequestWithRetryAsync(requestFactory);
    }
}
