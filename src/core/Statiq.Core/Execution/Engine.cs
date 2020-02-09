using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JavaScriptEngineSwitcher.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// The engine is the primary entry point for the generation process.
    /// </summary>
    public class Engine : IEngine, IDisposable
    {
        /// <summary>
        /// Gets the version of Statiq currently being used.
        /// </summary>
        public static string Version
        {
            get
            {
                if (!(Attribute.GetCustomAttribute(typeof(Engine).Assembly, typeof(AssemblyInformationalVersionAttribute)) is AssemblyInformationalVersionAttribute versionAttribute))
                {
                    throw new InvalidOperationException("Something went terribly wrong, could not determine Statiq version");
                }
                return versionAttribute.InformationalVersion;
            }
        }

        // Cache the HttpMessageHandler (the HttpClient is really just a thin wrapper around this)
        private static readonly HttpMessageHandler _httpMessageHandler = new HttpClientHandler();

        private readonly PipelineCollection _pipelines;
        private readonly DiagnosticsTraceListener _diagnosticsTraceListener;
        private readonly IServiceScope _serviceScope;
        private readonly ILogger _logger;

        // Gets initialized on first execute and reset when the pipeline collection changes
        private PipelinePhase[] _phases;

        private bool _disposed;

        /// <summary>
        /// Creates an engine with empty application state, configuration, and services.
        /// </summary>
        public Engine()
            : this(null, null, null)
        {
        }

        /// <summary>
        /// Creates an engine with the specified application state and service provider.
        /// </summary>
        /// <param name="applicationState">The state of the application (or <c>null</c> for an empty application state).</param>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="serviceCollection">The service collection (or <c>null</c> for an empty default service collection).</param>
        public Engine(ApplicationState applicationState, IConfiguration configuration, IServiceCollection serviceCollection)
        {
            _pipelines = new PipelineCollection(this);
            ApplicationState = applicationState ?? new ApplicationState(null, null, null);
            ScriptHelper = new ScriptHelper(this);
            Settings = new ReadOnlyConfigurationSettings(configuration ?? new ConfigurationRoot(Array.Empty<IConfigurationProvider>()), this);
            _serviceScope = GetServiceScope(serviceCollection);
            _logger = Services.GetRequiredService<ILogger<Engine>>();
            DocumentFactory = new DocumentFactory(this, Settings);
            _diagnosticsTraceListener = new DiagnosticsTraceListener(_logger);
            System.Diagnostics.Trace.Listeners.Add(_diagnosticsTraceListener);

            // Add the service-based pipelines as late as possible so other services have been configured
            AddServicePipelines();
        }

        /// <summary>
        /// Creates a service scope by registering engine
        /// types to the provided service collection or creating a new one.
        /// </summary>
        /// <remarks>
        /// Creates a new top-level scope so that transient services can be disposed with the engine
        /// See https://stackoverflow.com/questions/43244316/iserviceprovider-garbage-collection-disposal
        /// And https://github.com/aspnet/DependencyInjection/issues/456
        /// </remarks>
        /// <param name="serviceCollection">The service collection to create a scope for.</param>
        /// <returns>A built service scope (and provider).</returns>
        private IServiceScope GetServiceScope(IServiceCollection serviceCollection)
        {
            serviceCollection ??= new ServiceCollection();

            serviceCollection.AddLogging();
            serviceCollection.AddOptions();
            serviceCollection.AddSingleton<ApplicationState>(ApplicationState);
            serviceCollection.AddSingleton<IReadOnlyEventCollection>(Events);
            serviceCollection.AddSingleton<IReadOnlyFileSystem>(FileSystem);
            serviceCollection.AddSingleton<IReadOnlyConfigurationSettings>(Settings);
            serviceCollection.AddSingleton<IConfiguration>(Settings.Configuration);
            serviceCollection.AddSingleton<IReadOnlyShortcodeCollection>(Shortcodes);
            serviceCollection.AddSingleton<IMemoryStreamFactory>(MemoryStreamFactory);
            serviceCollection.AddSingleton<INamespacesCollection>(Namespaces);
            serviceCollection.AddSingleton<IScriptHelper>(ScriptHelper);

            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider.CreateScope();
        }

        /// <summary>
        /// Adds pipelines from the DI container to the pipeline collection.
        /// </summary>
        private void AddServicePipelines()
        {
            foreach (IPipeline pipeline in Services.GetServices<IPipeline>())
            {
                Pipelines.AddIfNonExisting(pipeline);
            }
        }

        /// <inheritdoc />
        public Guid ExecutionId { get; private set; } = Guid.Empty;

        /// <inheritdoc />
        public CancellationToken CancellationToken { get; private set; }

        /// <inheritdoc />
        public IServiceProvider Services => _serviceScope.ServiceProvider;

        /// <inheritdoc />
        public ApplicationState ApplicationState { get; }

        /// <inheritdoc />
        IReadOnlyApplicationState IExecutionState.ApplicationState => ApplicationState;

        /// <inheritdoc />
        public IReadOnlyConfigurationSettings Settings { get; }

        /// <inheritdoc />
        public IEventCollection Events { get; } = new EventCollection();

        /// <inheritdoc />
        IReadOnlyEventCollection IExecutionState.Events => Events;

        /// <inheritdoc />
        public IFileSystem FileSystem { get; } = new FileSystem();

        /// <inheritdoc />
        IReadOnlyFileSystem IExecutionState.FileSystem => FileSystem;

        /// <inheritdoc />
        public IShortcodeCollection Shortcodes { get; } = new ShortcodeCollection();

        /// <inheritdoc />
        IReadOnlyShortcodeCollection IExecutionState.Shortcodes => Shortcodes;

        /// <inheritdoc />
        public IPipelineCollection Pipelines => _pipelines;

        /// <inheritdoc />
        public INamespacesCollection Namespaces { get; } = new NamespaceCollection();

        /// <inheritdoc />
        public IMemoryStreamFactory MemoryStreamFactory { get; } = new MemoryStreamFactory();

        /// <inheritdoc />
        public IScriptHelper ScriptHelper { get; }

        /// <inheritdoc />
        public IExecutionContext CurrentContext => ExecutionContext.Current;

        /// <inheritdoc />
        public IPipelineOutputs Outputs { get; private set; }

        /// <inheritdoc />
        public bool SerialExecution { get; set; }

        internal DocumentFactory DocumentFactory { get; }

        internal void ResetPipelinePhases() => _phases = null;

        /// <inheritdoc />
        public void SetDefaultDocumentType<TDocument>()
            where TDocument : FactoryDocument, IDocument, new() =>
            DocumentFactory.SetDefaultDocumentType<TDocument>();

        /// <inheritdoc />
        public IDocument CreateDocument(
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            DocumentFactory.CreateDocument(source, destination, items, contentProvider);

        /// <inheritdoc />
        public TDocument CreateDocument<TDocument>(
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            DocumentFactory.CreateDocument<TDocument>(source, destination, items, contentProvider);

        /// <summary>
        /// Deletes the output path and all files it contains.
        /// </summary>
        public void CleanOutputPath()
        {
            try
            {
                _logger.LogDebug($"Cleaning output directory: {FileSystem.OutputPath}...");
                IDirectory outputDirectory = FileSystem.GetOutputDirectory();
                if (outputDirectory.Exists)
                {
                    outputDirectory.Delete(true);
                }
                _logger.LogInformation($"Cleaned output directory: {FileSystem.OutputPath}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Error while cleaning output directory: {0} - {1}", ex.GetType(), ex.Message);
            }
        }

        /// <summary>
        /// Deletes the temp path and all files it contains.
        /// </summary>
        public void CleanTempPath()
        {
            try
            {
                _logger.LogDebug($"Cleaning temp directory: {FileSystem.TempPath}...");
                IDirectory tempDirectory = FileSystem.GetTempDirectory();
                if (tempDirectory.Exists)
                {
                    tempDirectory.Delete(true);
                }
                tempDirectory.Create();
                _logger.LogInformation($"Cleaned temp directory: {FileSystem.TempPath}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Error while cleaning temp directory: {0} - {1}", ex.GetType(), ex.Message);
            }
        }

        /// <summary>
        /// Executes pipelines with <see cref="ExecutionPolicy.Normal"/> and <see cref="ExecutionPolicy.Always"/> policies.
        /// </summary>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used to cancel the execution.
        /// </param>
        /// <returns>The output documents from each executed pipeline.</returns>
        public Task<IPipelineOutputs> ExecuteAsync(CancellationToken cancellationToken = default) =>
            ExecuteAsync(null, true, cancellationToken);

        /// <summary>
        /// Executes the specified pipelines and pipelines with <see cref="ExecutionPolicy.Always"/> policies.
        /// </summary>
        /// <param name="pipelines">
        /// The pipelines to execute or <c>null</c>/empty to only execute pipelines with the <see cref="ExecutionPolicy.Always"/> policy.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used to cancel the execution.
        /// </param>
        /// <returns>The output documents from each executed pipeline.</returns>
        public Task<IPipelineOutputs> ExecuteAsync(string[] pipelines, CancellationToken cancellationToken = default) =>
            ExecuteAsync(pipelines, false, cancellationToken);

        /// <summary>
        /// Executes the specified pipelines and pipelines with <see cref="ExecutionPolicy.Always"/> policies.
        /// </summary>
        /// <param name="pipelines">
        /// The pipelines to execute or <c>null</c>/empty to only execute pipelines with the <see cref="ExecutionPolicy.Always"/> policy.
        /// </param>
        /// <param name="normalPipelines">
        /// <c>true</c> to run pipelines with the <see cref="ExecutionPolicy.Normal"/> policy in addition
        /// to the pipelines specified or <c>false</c> to only run the specified pipelines.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used to cancel the execution.
        /// </param>
        /// <returns>The output documents from each executed pipeline.</returns>
        public async Task<IPipelineOutputs> ExecuteAsync(string[] pipelines, bool normalPipelines, CancellationToken cancellationToken = default)
        {
            // Setup
            await default(SynchronizationContextRemover);
            CheckDisposed();

            // Make sure only one execution is running
            if (ExecutionId != Guid.Empty)
            {
                throw new ExecutionException($"Execution with ID {ExecutionId} is already executing, only one execution can be run at once");
            }
            ExecutionId = Guid.NewGuid();
            CancellationToken = cancellationToken;
            try
            {
                // Create the phase results for this execution
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                Outputs = new PipelineOutputs(phaseResults);

                // Create the pipeline phases (this also validates the pipeline graph)
                if (_phases == null)
                {
                    _phases = GetPipelinePhases(_pipelines, _logger);
                }

                // Verify pipelines
                HashSet<string> executingPipelines = GetExecutingPipelines(pipelines, normalPipelines);
                if (executingPipelines.Count == 0)
                {
                    _logger.LogWarning("No pipelines are configured or specified for execution.");
                    return Outputs;
                }

                // Log
                _logger.LogInformation($"Executing {executingPipelines.Count} pipelines ({string.Join(", ", executingPipelines.OrderBy(x => x))})");
                _logger.LogDebug($"Execution ID {ExecutionId}");
                System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

                // Raise before event
                await Events.RaiseAsync(new BeforeEngineExecution(this, ExecutionId));

                // Do a check for the same input/output path
                if (FileSystem.InputPaths.Any(x => x.Equals(FileSystem.OutputPath)))
                {
                    _logger.LogWarning("The output path is also one of the input paths which can cause unexpected behavior and is usually not advised");
                }

                // Clean paths
                CleanTempPath();
                if (Settings.GetBool(Keys.CleanOutputPath))
                {
                    CleanOutputPath();
                }

                // Get and run all phase tasks
                Task[] phaseTasks = null;
                try
                {
                    // Get and execute all phases
                    phaseTasks = GetPhaseTasks(executingPipelines, phaseResults);
                    await Task.WhenAll(phaseTasks);
                }
                catch (Exception ex)
                {
                    if (!(ex is OperationCanceledException))
                    {
                        _logger.LogCritical("Error during execution");
                    }
                    throw;
                }
                finally
                {
                    stopwatch.Stop();
                }

                // Raise after event
                await Events.RaiseAsync(new AfterEngineExecution(this, ExecutionId, Outputs, stopwatch.ElapsedMilliseconds));

                // Log execution summary table
                _logger.LogInformation(
                    "Execution summary: (number of output documents per pipeline and phase)"
                    + Environment.NewLine
                    + Environment.NewLine
                    + phaseResults
                        .OrderBy(x => x.Key)
                        .ToStringTable(
                            new[]
                            {
                                "Pipeline",
                                nameof(Phase.Input),
                                nameof(Phase.Process),
                                nameof(Phase.Transform),
                                nameof(Phase.Output),
                                "Total Time"
                            },
                            x => x.Key,
                            x => GetPhaseResultTableString(x.Value[(int)Phase.Input]),
                            x => GetPhaseResultTableString(x.Value[(int)Phase.Process]),
                            x => GetPhaseResultTableString(x.Value[(int)Phase.Transform]),
                            x => GetPhaseResultTableString(x.Value[(int)Phase.Output]),
                            x =>
                                ((x.Value[(int)Phase.Input]?.ElapsedMilliseconds ?? 0)
                                + (x.Value[(int)Phase.Process]?.ElapsedMilliseconds ?? 0)
                                + (x.Value[(int)Phase.Transform]?.ElapsedMilliseconds ?? 0)
                                + (x.Value[(int)Phase.Output]?.ElapsedMilliseconds ?? 0)).ToString()
                                + " ms"));

                static string GetPhaseResultTableString(PhaseResult phaseResult) =>
                    phaseResult == null
                        ? string.Empty
                        : $"{phaseResult.Outputs.Length} ({phaseResult.ElapsedMilliseconds} ms)";

                // Clean up
                _logger.LogInformation($"Finished execution in {stopwatch.ElapsedMilliseconds} ms");
                return Outputs;
            }
            finally
            {
                ExecutionId = Guid.Empty;
                CancellationToken = default;
            }
        }

        // Internal for testing
        internal HashSet<string> GetExecutingPipelines(string[] pipelines, bool normalPipelines)
        {
            // Validate
            if (pipelines != null)
            {
                foreach (string pipeline in pipelines)
                {
                    if (!_pipelines.ContainsKey(pipeline))
                    {
                        throw new ArgumentException($"Pipeline {pipeline} does not exist", nameof(pipelines));
                    }
                }
            }

            HashSet<string> executing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, IPipeline> kvp in _pipelines)
            {
                ExecutionPolicy effectivePolicy = GetEffectiveExecutionPolicy(kvp.Value);
                if (effectivePolicy == ExecutionPolicy.Always
                    || (normalPipelines && effectivePolicy == ExecutionPolicy.Normal)
                    || (pipelines?.Any(x => x.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase)) == true))
                {
                    AddPipelineAndDependencies(kvp.Key);
                }
            }

            void AddPipelineAndDependencies(string pipelineName)
            {
                if (executing.Add(pipelineName))
                {
                    foreach (string dependency in _pipelines[pipelineName].Dependencies)
                    {
                        AddPipelineAndDependencies(dependency);
                    }
                }
            }

            return executing;
        }

        private static ExecutionPolicy GetEffectiveExecutionPolicy(IPipeline pipeline) =>
            pipeline.ExecutionPolicy == ExecutionPolicy.Default
                ? pipeline.Deployment ? ExecutionPolicy.Manual : ExecutionPolicy.Normal
                : pipeline.ExecutionPolicy;

        // The result array is sorted based on dependencies
        // Internal for testing
        internal static PipelinePhase[] GetPipelinePhases(IPipelineCollection pipelines, ILogger logger)
        {
            // Perform a topological sort to create phases down the dependency tree
            Dictionary<string, PipelinePhases> phasesByPipeline = new Dictionary<string, PipelinePhases>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> visitedPipelines = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, IPipeline> pipelineEntry in pipelines)
            {
                Visit(pipelineEntry.Key, pipelineEntry.Value);
            }

            // Make a pass through non-isolated transform phases to set dependencies to all non-isolated process phases
            foreach (PipelinePhases pipelinePhases in phasesByPipeline.Values.Where(x => !x.Pipeline.Isolated))
            {
                pipelinePhases.Transform.Dependencies =
                    pipelinePhases.Transform.Dependencies
                        .Concat(phasesByPipeline.Values.Where(x => x != pipelinePhases && !x.Pipeline.Isolated).Select(x => x.Process))
                        .ToArray();
            }

            // Make a pass through deployment pipeline output phases to set dependencies to all non-deployment output phases
            foreach (PipelinePhases pipelinePhases in phasesByPipeline.Values.Where(x => x.Pipeline.Deployment))
            {
                pipelinePhases.Output.Dependencies =
                    pipelinePhases.Output.Dependencies
                        .Concat(phasesByPipeline.Values.Where(x => x != pipelinePhases && !x.Pipeline.Deployment).Select(x => x.Output))
                        .ToArray();
            }

            // Perform another topological sort for phases based on their dependencies which is important
            // because tasks will be created in the order of the result array and we need tasks for
            // dependencies to be available for the continuation before the task that depends on them
            HashSet<PipelinePhase> visitedPhases = new HashSet<PipelinePhase>();
            Queue<PipelinePhase> sortedPhases = new Queue<PipelinePhase>();
            foreach (PipelinePhases pipelinePhases in phasesByPipeline.Values)
            {
                SortPhases(pipelinePhases.Input);
                SortPhases(pipelinePhases.Process);
                SortPhases(pipelinePhases.Transform);
                SortPhases(pipelinePhases.Output);
            }
            return sortedPhases.ToArray();

            // Returns the process phases (if not isolated)
            PipelinePhases Visit(string name, IPipeline pipeline)
            {
                PipelinePhases pipelinePhases;

                if (pipeline.Isolated)
                {
                    // Sanity check
                    if (pipeline.Dependencies?.Count > 0)
                    {
                        throw new PipelineException($"Isolated pipeline {name} can not have dependencies");
                    }

                    // This is an isolated pipeline so just add the phases in a chain
                    pipelinePhases = new PipelinePhases(pipeline);
                    pipelinePhases.Input = new PipelinePhase(pipeline, name, Phase.Input, pipeline.InputModules, logger);
                    pipelinePhases.Process = new PipelinePhase(pipeline, name, Phase.Process, pipeline.ProcessModules, logger, pipelinePhases.Input);
                    pipelinePhases.Transform = new PipelinePhase(pipeline, name, Phase.Transform, pipeline.TransformModules, logger, pipelinePhases.Process);
                    pipelinePhases.Output = new PipelinePhase(pipeline, name, Phase.Output, pipeline.OutputModules, logger, pipelinePhases.Transform);
                    phasesByPipeline.Add(name, pipelinePhases);
                    return pipelinePhases;
                }

                if (visitedPipelines.Add(name))
                {
                    // Visit dependencies if this isn't an isolated pipeline
                    List<PipelinePhase> processDependencies = new List<PipelinePhase>();
                    if (pipeline.Dependencies != null)
                    {
                        foreach (string dependencyName in pipeline.Dependencies)
                        {
                            if (!pipelines.TryGetValue(dependencyName, out IPipeline dependency))
                            {
                                throw new PipelineException($"Could not find pipeline dependency {dependencyName} of {name}");
                            }
                            if (!dependency.Isolated)
                            {
                                // Only add the phase dependency if the dependency is not isolated
                                processDependencies.Add(Visit(dependencyName, dependency).Process);
                            }
                        }
                    }

                    // Add the phases (by this time all dependencies should have been added)
                    pipelinePhases = new PipelinePhases(pipeline);
                    pipelinePhases.Input = new PipelinePhase(pipeline, name, Phase.Input, pipeline.InputModules, logger);
                    processDependencies.Insert(0, pipelinePhases.Input);  // Makes sure the process phase is also dependent on it's input phase
                    pipelinePhases.Process = new PipelinePhase(pipeline, name, Phase.Process, pipeline.ProcessModules, logger, processDependencies.ToArray());
                    pipelinePhases.Transform = new PipelinePhase(pipeline, name, Phase.Transform, pipeline.TransformModules, logger, pipelinePhases.Process);  // Transform dependencies will be added after all pipelines have been processed
                    pipelinePhases.Output = new PipelinePhase(pipeline, name, Phase.Output, pipeline.OutputModules, logger, pipelinePhases.Transform);  // Output dependencies for deployment pipelines will be added after all pipelines have been processed
                    phasesByPipeline.Add(name, pipelinePhases);
                }
                else if (!phasesByPipeline.TryGetValue(name, out pipelinePhases))
                {
                    throw new PipelineException($"Pipeline cyclical dependency detected involving {name}");
                }

                return pipelinePhases;
            }

            // Simple topological sort without cycle checking (we already checked for cycles)
            void SortPhases(PipelinePhase phase)
            {
                if (visitedPhases.Add(phase))
                {
                    foreach (PipelinePhase dependency in phase.Dependencies)
                    {
                        SortPhases(dependency);
                    }
                    sortedPhases.Enqueue(phase);
                }
            }
        }

        private Task[] GetPhaseTasks(
            HashSet<string> executingPipelines,
            ConcurrentDictionary<string, PhaseResult[]> phaseResults)
        {
            Dictionary<PipelinePhase, Task> phaseTasks = new Dictionary<PipelinePhase, Task>();
            foreach (PipelinePhase phase in _phases.Where(x => executingPipelines.Contains(x.PipelineName)))
            {
                Task phaseTask = GetPhaseTaskAsync(
                    phaseResults,
                    phaseTasks,
                    phase);
                if (SerialExecution)
                {
                    // If we're running serially, immediately wait for this phase task before getting the next one
                    phaseTask.Wait(CancellationToken);
                }
                phaseTasks.Add(phase, phaseTask);
            }
            return phaseTasks.Values.ToArray();
        }

        private Task GetPhaseTaskAsync(
            ConcurrentDictionary<string, PhaseResult[]> phaseResults,
            Dictionary<PipelinePhase, Task> phaseTasks,
            PipelinePhase phase)
        {
            // Only the input phase won't have dependencies, all other phases at least depend on the previous phase of their pipeline
            if (phase.Dependencies.Length == 0)
            {
                // This will immediately queue the input phase while we continue figuring out tasks, but that's okay
                return Task.Run(() => phase.ExecuteAsync(this, phaseResults), CancellationToken);
            }

            // We have to explicitly wait the execution task in the continuation function
            // (the continuation task doesn't wait for the tasks it continues)
            // Note that we need to check if each dependency actually has a task since some pipelines might not be in this execution
            // For example, the transform phase of every pipeline depends on the process phase of every other pipeline, including manual ones
            return Task.Factory.ContinueWhenAll(
                phase.Dependencies.Select(x => phaseTasks
                    .TryGetValue(x, out Task dependencyTask) ? dependencyTask : null)
                    .Where(x => x != null)
                    .ToArray(),
                dependencies =>
                {
                    // Only run the dependent task if all the dependencies successfully completed
                    if (dependencies.All(x => x.IsCompletedSuccessfully))
                    {
                        Task.WaitAll(new Task[] { phase.ExecuteAsync(this, phaseResults) }, CancellationToken);
                    }
                    else
                    {
                        // Otherwise, throw an exception so that this dependency is also skipped by it's dependents
                        string error = $"{phase.PipelineName}/{phase.Phase} » Skipping pipeline due to dependency error";
                        _logger.LogError(error);
                        throw new ExecutionException(error);
                    }
                }, CancellationToken);
        }

        // This executes the specified modules with the specified input documents
        internal static async Task<ImmutableArray<IDocument>> ExecuteModulesAsync(
            ExecutionContextData contextData,
            IExecutionContext parent,
            IEnumerable<IModule> modules,
            ImmutableArray<IDocument> inputs,
            ILogger logger)
        {
            ImmutableArray<IDocument> outputs = ImmutableArray<IDocument>.Empty;
            if (modules != null)
            {
                foreach (IModule module in modules.Where(x => x != null))
                {
                    string moduleName = module.GetType().Name;

                    try
                    {
                        // Check for cancellation
                        contextData.Engine.CancellationToken.ThrowIfCancellationRequested();

                        // Get the context
                        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
                        ExecutionContext moduleContext = new ExecutionContext(contextData, parent, module, inputs);
                        moduleContext.LogDebug($"Starting module execution... ({inputs.Length} input document(s))");

                        // Raise the before event and use overridden results if provided
                        BeforeModuleExecution beforeEvent = new BeforeModuleExecution(moduleContext);
                        bool raised = await contextData.Engine.Events.RaiseAsync(beforeEvent);
                        if (raised && beforeEvent.OverriddenOutputs != null)
                        {
                            outputs = beforeEvent.OverriddenOutputs.ToImmutableDocumentArray();
                        }
                        else
                        {
                            // Execute the module
                            IEnumerable<IDocument> moduleResult = await (module.ExecuteAsync(moduleContext) ?? Task.FromResult<IEnumerable<IDocument>>(null));  // Handle a null Task return
                            outputs = moduleResult.ToImmutableDocumentArray();
                        }
                        stopwatch.Stop();

                        // Raise the after event
                        AfterModuleExecution afterEvent = new AfterModuleExecution(moduleContext, outputs, stopwatch.ElapsedMilliseconds);
                        raised = await contextData.Engine.Events.RaiseAsync(afterEvent);
                        if (raised && afterEvent.OverriddenOutputs != null)
                        {
                            outputs = afterEvent.OverriddenOutputs.ToImmutableDocumentArray();
                        }

                        // Log results
                        moduleContext.LogDebug($"Finished module execution ({outputs.Length} output document(s), {stopwatch.ElapsedMilliseconds} ms)");
                        inputs = outputs;
                    }
                    catch (Exception ex)
                    {
                        if (!(ex is OperationCanceledException))
                        {
                            logger.LogError($"Error while executing module {moduleName} in {contextData.PipelinePhase.PipelineName}/{contextData.PipelinePhase.Phase}: {ex.Message}");
                        }
                        outputs = ImmutableArray<IDocument>.Empty;
                        throw;
                    }
                }
            }
            return outputs;
        }

        /// <inheritdoc />
        public async Task<Stream> GetContentStreamAsync(string content = null)
        {
            if (Settings.GetBool(Keys.UseStringContentFiles))
            {
                // Use a temp file for strings
                IFile tempFile = FileSystem.GetTempFile();
                if (!string.IsNullOrEmpty(content))
                {
                    await tempFile.WriteAllTextAsync(content);
                }
                return new ContentStream(x => new FileContent(tempFile, x), tempFile.Open(), true);
            }

            // Otherwise get a memory stream from the pool and use that
            Stream memoryStream = MemoryStreamFactory.GetStream(content);
            return new ContentStream(x => new Common.StreamContent(MemoryStreamFactory, memoryStream, x), memoryStream, false);
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

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (_phases != null)
            {
                foreach (PipelinePhase phase in _phases)
                {
                    phase.Dispose();
                }
            }

            foreach (IPipeline pipeline in _pipelines.Values)
            {
                if (pipeline is IDisposable disposablePipeline)
                {
                    disposablePipeline.Dispose();
                }
            }

            System.Diagnostics.Trace.Listeners.Remove(_diagnosticsTraceListener);
            CleanTempPath();
            _serviceScope.Dispose();
            _disposed = true;
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Engine));
            }
        }

        private class PipelinePhases
        {
            public PipelinePhases(IPipeline pipeline)
            {
                Pipeline = pipeline;
            }

            public IPipeline Pipeline { get; }
            public PipelinePhase Input { get; set; }
            public PipelinePhase Process { get; set; }
            public PipelinePhase Transform { get; set; }
            public PipelinePhase Output { get; set; }
        }
    }
}
