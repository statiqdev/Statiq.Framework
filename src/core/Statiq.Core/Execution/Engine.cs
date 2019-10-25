using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
            Settings = new ReadOnlyConfigurationSettings(configuration ?? new ConfigurationRoot(Array.Empty<IConfigurationProvider>()));
            _serviceScope = GetServiceScope(serviceCollection);
            _logger = Services.GetRequiredService<ILogger<Engine>>();
            DocumentFactory = new DocumentFactory(Settings);
            _diagnosticsTraceListener = new DiagnosticsTraceListener(_logger);
            System.Diagnostics.Trace.Listeners.Add(_diagnosticsTraceListener);
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

            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider.CreateScope();
        }

        /// <inheritdoc />
        public IServiceProvider Services => _serviceScope.ServiceProvider;

        /// <inheritdoc />
        public ApplicationState ApplicationState { get; }

        /// <inheritdoc />
        public IReadOnlyConfigurationSettings Settings { get; }

        /// <inheritdoc />
        public IEventCollection Events { get; } = new EventCollection();

        /// <inheritdoc />
        public IFileSystem FileSystem { get; } = new FileSystem();

        /// <inheritdoc />
        public IShortcodeCollection Shortcodes { get; } = new ShortcodeCollection();

        /// <inheritdoc />
        public IPipelineCollection Pipelines => _pipelines;

        /// <inheritdoc />
        public INamespacesCollection Namespaces { get; } = new NamespaceCollection();

        /// <inheritdoc />
        public IMemoryStreamFactory MemoryStreamFactory { get; } = new MemoryStreamFactory();

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
        /// Executes pipelines with <see cref="ExecutionPolicy.Default"/> and <see cref="ExecutionPolicy.Always"/> policies.
        /// </summary>
        /// <param name="cancellationTokenSource">
        /// A cancellation token source that can be used to cancel the execution.
        /// </param>
        /// <returns>The output documents from each executed pipeline.</returns>
        public Task<IPipelineOutputs> ExecuteAsync(CancellationTokenSource cancellationTokenSource) =>
            ExecuteAsync(null, true, cancellationTokenSource);

        /// <summary>
        /// Executes the specified pipelines and pipelines with <see cref="ExecutionPolicy.Always"/> policies.
        /// </summary>
        /// <param name="pipelines">
        /// The pipelines to execute or <c>null</c> to execute pipelines with
        /// <see cref="ExecutionPolicy.Default"/> and <see cref="ExecutionPolicy.Always"/> policies.
        /// To only execute pipelines with <see cref="ExecutionPolicy.Always"/> provide a zero-length array.
        /// </param>
        /// <param name="cancellationTokenSource">
        /// A cancellation token source that can be used to cancel the execution.
        /// </param>
        /// <returns>The output documents from each executed pipeline.</returns>
        public Task<IPipelineOutputs> ExecuteAsync(string[] pipelines, CancellationTokenSource cancellationTokenSource) =>
            ExecuteAsync(pipelines, false, cancellationTokenSource);

        /// <summary>
        /// Executes the specified pipelines and pipelines with <see cref="ExecutionPolicy.Always"/> policies.
        /// </summary>
        /// <param name="pipelines">
        /// The pipelines to execute or <c>null</c>/empty to only execute pipelines with the <see cref="ExecutionPolicy.Always"/> policy.
        /// </param>
        /// <param name="defaultPipelines">
        /// <c>true</c> to run the default pipelines in addition to the pipelines specified
        /// or <c>false</c> to only run the specified pipelines.
        /// </param>
        /// <param name="cancellationTokenSource">
        /// A cancellation token source that can be used to cancel the execution.
        /// </param>
        /// <returns>The output documents from each executed pipeline.</returns>
        public async Task<IPipelineOutputs> ExecuteAsync(string[] pipelines, bool defaultPipelines, CancellationTokenSource cancellationTokenSource)
        {
            // Setup
            await default(SynchronizationContextRemover);
            CheckDisposed();
            Guid executionId = Guid.NewGuid();
            ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
            PipelineOutputs outputs = new PipelineOutputs(phaseResults);

            // Create the pipeline phases (this also validates the pipeline graph)
            // Also add the service-based pipelines as late as possible so other services have been configured
            if (_phases == null)
            {
                AddServicePipelines();
                _phases = GetPipelinePhases(_pipelines, _logger);
            }

            // Verify pipelines
            HashSet<string> executingPipelines = GetExecutingPipelines(pipelines, defaultPipelines);
            if (executingPipelines.Count == 0)
            {
                _logger.LogWarning("No pipelines are configured or specified for execution.");
                return outputs;
            }

            // Log
            _logger.LogInformation($"Executing {executingPipelines.Count} pipelines ({string.Join(", ", executingPipelines.OrderBy(x => x))})");
            _logger.LogDebug($"Execution ID {executionId}");
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Raise before event
            await Events.RaiseAsync(new BeforeEngineExecution(this, executionId));

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

            // Get phase tasks
            Task[] phaseTasks = null;
            try
            {
                // Get and execute all phases
                phaseTasks = GetPhaseTasks(executionId, executingPipelines, phaseResults, cancellationTokenSource);
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
            await Events.RaiseAsync(new AfterEngineExecution(this, executionId, outputs, stopwatch.ElapsedMilliseconds));

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
            return outputs;
        }

        // Internal for testing
        internal HashSet<string> GetExecutingPipelines(string[] pipelines, bool defaultPipelines)
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
                if (kvp.Value.ExecutionPolicy == ExecutionPolicy.Always
                    || (defaultPipelines && kvp.Value.ExecutionPolicy == ExecutionPolicy.Default)
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

        /// <summary>
        /// Adds pipelines from the DI container to the pipeline collection.
        /// </summary>
        private void AddServicePipelines()
        {
            foreach (IPipeline pipeline in Services.GetServices<IPipeline>())
            {
                Pipelines.Add(pipeline);
            }
        }

        // The result array is sorted based on dependencies
        // Internal for testing
        internal static PipelinePhase[] GetPipelinePhases(IPipelineCollection pipelines, ILogger logger)
        {
            // Perform a topological sort to create phases down the dependency tree
            Dictionary<string, PipelinePhases> phases = new Dictionary<string, PipelinePhases>(StringComparer.OrdinalIgnoreCase);
            List<PipelinePhases> sortedPhases = new List<PipelinePhases>();
            HashSet<string> visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, IPipeline> pipelineEntry in pipelines)
            {
                Visit(pipelineEntry.Key, pipelineEntry.Value);
            }

            // Make a pass through non-isolated transform phases to set dependencies to all non-isolated process phases
            foreach (PipelinePhases pipelinePhases in phases.Values.Where(x => !x.Pipeline.Isolated))
            {
                pipelinePhases.Transform.Dependencies =
                    pipelinePhases.Transform.Dependencies
                        .Concat(phases.Values.Where(x => x != pipelinePhases && !x.Pipeline.Isolated).Select(x => x.Process))
                        .ToArray();
            }

            // Make a pass through deployment pipeline output phases to set dependencies to all non-deployment output phases
            foreach (PipelinePhases pipelinePhases in phases.Values.Where(x => x.Pipeline.Deployment))
            {
                pipelinePhases.Output.Dependencies =
                    pipelinePhases.Output.Dependencies
                        .Concat(phases.Values.Where(x => x != pipelinePhases && !x.Pipeline.Deployment).Select(x => x.Output))
                        .ToArray();
            }

            return sortedPhases
                .Select(x => x.Input)
                .Concat(sortedPhases.Select(x => x.Process))
                .Concat(sortedPhases.Select(x => x.Transform))
                .Concat(sortedPhases.Select(x => x.Output))
                .ToArray();

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
                    phases.Add(name, pipelinePhases);
                    sortedPhases.Add(pipelinePhases);
                    return pipelinePhases;
                }

                if (visited.Add(name))
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
                    pipelinePhases.Output = new PipelinePhase(pipeline, name, Phase.Output, pipeline.OutputModules, logger, pipelinePhases.Transform);
                    phases.Add(name, pipelinePhases);
                    sortedPhases.Add(pipelinePhases);
                }
                else if (!phases.TryGetValue(name, out pipelinePhases))
                {
                    throw new PipelineException($"Pipeline cyclical dependency detected involving {name}");
                }

                return pipelinePhases;
            }
        }

        private Task[] GetPhaseTasks(
            Guid executionId,
            HashSet<string> executingPipelines,
            ConcurrentDictionary<string, PhaseResult[]> phaseResults,
            CancellationTokenSource cancellationTokenSource)
        {
            Dictionary<PipelinePhase, Task> phaseTasks = new Dictionary<PipelinePhase, Task>();
            foreach (PipelinePhase phase in _phases.Where(x => executingPipelines.Contains(x.PipelineName)))
            {
                Task phaseTask = GetPhaseTaskAsync(
                    executionId,
                    phaseResults,
                    phaseTasks,
                    phase,
                    cancellationTokenSource);
                if (SerialExecution)
                {
                    // If we're running serially, immediately wait for this phase task before getting the next one
                    phaseTask.Wait(cancellationTokenSource.Token);
                }
                phaseTasks.Add(phase, phaseTask);
            }
            return phaseTasks.Values.ToArray();
        }

        private Task GetPhaseTaskAsync(
            Guid executionId,
            ConcurrentDictionary<string, PhaseResult[]> phaseResults,
            Dictionary<PipelinePhase, Task> phaseTasks,
            PipelinePhase phase,
            CancellationTokenSource cancellationTokenSource)
        {
            if (phase.Dependencies.Length == 0)
            {
                // This will immediately queue the input phase while we continue figuring out tasks, but that's okay
                return Task.Run(
                    () => phase.ExecuteAsync(this, executionId, phaseResults, cancellationTokenSource),
                    cancellationTokenSource.Token);
            }

            // We have to explicitly wait the execution task in the continuation function
            // (the continuation task doesn't wait for the tasks it continues)
            return Task.Factory.ContinueWhenAll(
                phase.Dependencies.Select(x => phaseTasks.TryGetValue(x, out Task dependencyTask) ? dependencyTask : null).Where(x => x != null).ToArray(),
                dependencies =>
                {
                    // Only run the dependent task if all the dependencies successfully completed
                    if (dependencies.All(x => x.IsCompletedSuccessfully))
                    {
                        Task.WaitAll(
                            new Task[] { phase.ExecuteAsync(this, executionId, phaseResults, cancellationTokenSource) },
                            cancellationTokenSource.Token);
                    }
                    else
                    {
                        // Otherwise, throw an exception so that this dependency is also skipped by it's dependents
                        string error = $"{phase.PipelineName}/{phase.Phase} » Skipping pipeline due to dependency error";
                        _logger.LogError(error);
                        throw new ExecutionException(error);
                    }
                }, cancellationTokenSource.Token);
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
                        contextData.CancellationToken.ThrowIfCancellationRequested();

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
