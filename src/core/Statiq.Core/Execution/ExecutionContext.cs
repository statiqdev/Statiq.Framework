using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Statiq.Common;

namespace Statiq.Core
{
    internal class ExecutionContext : IExecutionContext
    {
        private readonly ExecutionContextData _contextData;
        private readonly ILogger _logger;
        private readonly string _logPrefix;

        internal ExecutionContext(ExecutionContextData contextData, IExecutionContext parent, IModule module, ImmutableArray<IDocument> inputs)
        {
            _contextData = contextData ?? throw new ArgumentNullException(nameof(contextData));
            _logger = contextData.Services.GetRequiredService<ILogger<ExecutionContext>>();
            _logPrefix = GetLogPrefix(parent, module, contextData.PipelinePhase);

            Parent = parent;
            Module = module ?? throw new ArgumentNullException(nameof(module));
            Inputs = inputs;

            IExecutionContext.Current = this;
        }

        private static string GetLogPrefix(IExecutionContext parent, IModule module, PipelinePhase pipelinePhase)
        {
            Stack<string> modules = new Stack<string>();
            modules.Push(module.GetType().Name);
            while (parent != null)
            {
                modules.Push(parent.Module.GetType().Name);
                parent = parent.Parent;
            }
            return $"{pipelinePhase.PipelineName}/{pipelinePhase.Phase} » {string.Join(" » ", modules)} » ";
        }

        /// <inheritdoc/>
        public IExecutionState ExecutionState => _contextData.Engine;

        /// <inheritdoc/>
        public Guid ExecutionId => _contextData.Engine.ExecutionId;

        /// <inheritdoc/>
        public INamespacesCollection Namespaces => _contextData.Engine.Namespaces;

        /// <inheritdoc/>
        public IReadOnlyPipeline Pipeline => _contextData.PipelinePhase.Pipeline;

        /// <inheritdoc/>
        public string PipelineName => _contextData.PipelinePhase.PipelineName;

        /// <inheritdoc/>
        public Phase Phase => _contextData.PipelinePhase.Phase;

        /// <inheritdoc/>
        public IReadOnlyEventCollection Events => _contextData.Engine.Events;

        /// <inheritdoc/>
        public IPipelineOutputs Outputs => _contextData.Outputs;

        /// <inheritdoc/>
        public IReadOnlyFileSystem FileSystem => _contextData.Engine.FileSystem;

        /// <inheritdoc/>
        public IReadOnlyConfigurationSettings Settings => _contextData.Engine.Settings;

        /// <inheritdoc/>
        public IReadOnlyShortcodeCollection Shortcodes => _contextData.Engine.Shortcodes;

        /// <inheritdoc/>
        public IReadOnlyApplicationState ApplicationState => _contextData.Engine.ApplicationState;

        /// <inheritdoc/>
        public ClassCatalog ClassCatalog => _contextData.Engine.ClassCatalog;

        /// <inheritdoc/>
        public IServiceProvider Services => _contextData.Services; // Expose the scoped provider inside the execution context

        /// <inheritdoc/>
        public bool SerialExecution => _contextData.Engine.SerialExecution;

        /// <inheritdoc/>
        public IMemoryStreamFactory MemoryStreamFactory => _contextData.Engine.MemoryStreamFactory;

        /// <inheritdoc/>
        public IScriptHelper ScriptHelper => _contextData.Engine.ScriptHelper;

        /// <inheritdoc/>
        public IReadOnlyPipelineCollection Pipelines => _contextData.Engine.Pipelines;

        /// <inheritdoc/>
        public CancellationToken CancellationToken => _contextData.Engine.CancellationToken;

        /// <inheritdoc/>
        public IExecutionContext Parent { get; }

        /// <inheritdoc/>
        public IModule Module { get; }

        /// <inheritdoc/>
        public ImmutableArray<IDocument> Inputs { get; }

        /// <inheritdoc/>
        public HttpClient CreateHttpClient() => _contextData.Engine.CreateHttpClient();

        /// <inheritdoc/>
        public HttpClient CreateHttpClient(HttpMessageHandler handler) => _contextData.Engine.CreateHttpClient(handler);

        /// <inheritdoc/>
        public Task<HttpResponseMessage> SendHttpRequestWithRetryAsync(Func<HttpRequestMessage> requestFactory) =>
            _contextData.Engine.SendHttpRequestWithRetryAsync(requestFactory);

        /// <inheritdoc/>
        public async Task<ImmutableArray<IDocument>> ExecuteModulesAsync(IEnumerable<IModule> modules, IEnumerable<IDocument> inputs) =>
            await Engine.ExecuteModulesAsync(_contextData, this, modules, inputs?.ToImmutableArray() ?? ImmutableArray<IDocument>.Empty, this);

        /// <inheritdoc/>
        public Task<Stream> GetContentStreamAsync(string content = null) =>
            _contextData.Engine.GetContentStreamAsync(content);

        /// <inheritdoc/>
        public IJavaScriptEnginePool GetJavaScriptEnginePool(
            Action<IJavaScriptEngine> initializer = null,
            int startEngines = 10,
            int maxEngines = 25,
            int maxUsagesPerEngine = 100,
            TimeSpan? engineTimeout = null) =>
            _contextData.Engine.GetJavaScriptEnginePool(initializer, startEngines, maxEngines, maxUsagesPerEngine, engineTimeout);

        // IDocumentFactory

        /// <inheritdoc />
        public IDocument CreateDocument(
            NormalizedPath source,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            _contextData.Engine.DocumentFactory.CreateDocument(source, destination, items, contentProvider);

        /// <inheritdoc />
        public TDocument CreateDocument<TDocument>(
            NormalizedPath source,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            _contextData.Engine.DocumentFactory.CreateDocument<TDocument>(source, destination, items, contentProvider);

        // IServiceProvider

        public object GetService(Type serviceType) => _contextData.Services.GetService(serviceType);

        // ILogger

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) =>
            _logger.Log(logLevel, eventId, state, exception, (s, e) => _logPrefix + formatter(s, e));

        public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

        public IDisposable BeginScope<TState>(TState state) => _logger.BeginScope(state);
    }
}
