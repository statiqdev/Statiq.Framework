using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JavaScriptEngineSwitcher.Core;
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
                    throw new Exception("Something went terribly wrong, could not determine Statiq version");
                }
                return versionAttribute.InformationalVersion;
            }
        }

        private readonly PipelineCollection _pipelines = new PipelineCollection();
        private readonly DiagnosticsTraceListener _diagnosticsTraceListener;
        private readonly ILogger _logger;

        // Gets initialized on first execute
        private PipelinePhase[] _phases;

        private bool _disposed;

        /// <summary>
        /// Creates an engine.
        /// </summary>
        public Engine()
            : this(null, null)
        {
        }

        /// <summary>
        /// Creates an engine with the specified application state .
        /// </summary>
        /// <param name="applicationState">The state of the application (or <c>null</c> for an empty application state).</param>
        public Engine(ApplicationState applicationState)
            : this(applicationState, null)
        {
        }

        /// <summary>
        /// Creates an engine with the specified service provider.
        /// </summary>
        /// <param name="serviceCollection">The service collection (or <c>null</c> for an empty default service collection).</param>
        public Engine(IServiceCollection serviceCollection)
            : this(null, serviceCollection)
        {
        }

        /// <summary>
        /// Creates an engine with the specified application state and service provider.
        /// </summary>
        /// <param name="applicationState">The state of the application (or <c>null</c> for an empty application state).</param>
        /// <param name="serviceCollection">The service collection (or <c>null</c> for an empty default service collection).</param>
        public Engine(ApplicationState applicationState, IServiceCollection serviceCollection)
        {
            ApplicationState = applicationState ?? new ApplicationState(null, null, null);
            Services = GetServiceProvider(serviceCollection);
            _logger = Services.GetRequiredService<ILogger<Engine>>();
            DocumentFactory = new DocumentFactory(Settings);
            _diagnosticsTraceListener = new DiagnosticsTraceListener(_logger);
            System.Diagnostics.Trace.Listeners.Add(_diagnosticsTraceListener);
        }

        /// <summary>
        /// Creates a service provider by registering engine
        /// types to the provided service collection or creating a new one.
        /// </summary>
        /// <param name="serviceCollection">The service collection to create a provider for.</param>
        /// <returns>A built service provider.</returns>
        private IServiceProvider GetServiceProvider(IServiceCollection serviceCollection)
        {
            serviceCollection ??= new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.AddSingleton<ApplicationState>(ApplicationState);
            serviceCollection.AddSingleton<IReadOnlyEventCollection>(Events);
            serviceCollection.AddSingleton<IReadOnlyFileSystem>(FileSystem);
            serviceCollection.AddSingleton<IReadOnlySettings>(Settings);
            serviceCollection.AddSingleton<IReadOnlyShortcodeCollection>(Shortcodes);
            serviceCollection.AddSingleton<IMemoryStreamFactory>(MemoryStreamFactory);
            serviceCollection.AddSingleton<INamespacesCollection>(Namespaces);
            serviceCollection.AddSingleton<IRawAssemblyCollection>(DynamicAssemblies);
            return serviceCollection.BuildServiceProvider();
        }

        /// <inheritdoc />
        public IServiceProvider Services { get; }

        /// <inheritdoc />
        public ApplicationState ApplicationState { get; }

        /// <inheritdoc />
        public IEventCollection Events { get; } = new EventCollection();

        /// <inheritdoc />
        public IFileSystem FileSystem { get; } = new FileSystem();

        /// <inheritdoc />
        public ISettings Settings { get; } = new Settings();

        /// <inheritdoc />
        public IShortcodeCollection Shortcodes { get; } = new ShortcodeCollection();

        /// <inheritdoc />
        public IPipelineCollection Pipelines => _pipelines;

        /// <inheritdoc />
        public INamespacesCollection Namespaces { get; } = new NamespaceCollection();

        /// <inheritdoc />
        public IRawAssemblyCollection DynamicAssemblies { get; } = new RawAssemblyCollection();

        /// <inheritdoc />
        public IMemoryStreamFactory MemoryStreamFactory { get; } = new MemoryStreamFactory();

        internal DocumentFactory DocumentFactory { get; }

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
                _logger.LogInformation("Cleaning output path: {0}", FileSystem.OutputPath);
                IDirectory outputDirectory = FileSystem.GetOutputDirectory();
                if (outputDirectory.Exists)
                {
                    outputDirectory.Delete(true);
                }
                _logger.LogInformation("Cleaned output directory");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Error while cleaning output path: {0} - {1}", ex.GetType(), ex.Message);
            }
        }

        /// <summary>
        /// Deletes the temp path and all files it contains.
        /// </summary>
        public void CleanTempPath()
        {
            try
            {
                _logger.LogInformation("Cleaning temp path: {0}", FileSystem.TempPath);
                IDirectory tempDirectory = FileSystem.GetTempDirectory();
                if (tempDirectory.Exists)
                {
                    tempDirectory.Delete(true);
                }
                _logger.LogInformation("Cleaned temp directory");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Error while cleaning temp path: {0} - {1}", ex.GetType(), ex.Message);
            }
        }

        /// <summary>
        /// Resets the JavaScript Engine pool and clears the JavaScript Engine Switcher
        /// to an empty list of engine factories and default engine. Useful on configuration
        /// change where we might have a new configuration.
        /// </summary>
        public static void ResetJsEngines()
        {
            JsEngineSwitcher.Current.EngineFactories.Clear();
            JsEngineSwitcher.Current.DefaultEngineName = string.Empty;
        }

        /// <summary>
        /// Executes the engine. This is the primary method that kicks off generation.
        /// </summary>
        public async Task<IPipelineOutputs> ExecuteAsync(CancellationTokenSource cancellationTokenSource)
        {
            // Setup
            await default(SynchronizationContextRemover);
            CheckDisposed();
            Guid executionId = Guid.NewGuid();
            ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
            PipelineOutputs outputs = new PipelineOutputs(phaseResults);
            _logger.LogInformation($"Executing {_pipelines.Count} pipelines (execution ID {executionId})");
            _logger.LogInformation($"Using {JsEngineSwitcher.Current.DefaultEngineName} as the JavaScript engine");
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Raise before event
            await Events.RaiseAsync(new BeforeEngineExecution(this, executionId));

            // Make sure we've actually configured some pipelines
            if (_pipelines.Count == 0)
            {
                _logger.LogWarning("No pipelines are configured.");
                return outputs;
            }

            // Do a check for the same input/output path
            if (FileSystem.InputPaths.Any(x => x.Equals(FileSystem.OutputPath)))
            {
                _logger.LogWarning("The output path is also one of the input paths which can cause unexpected behavior and is usually not advised");
            }

            CleanTempPath();

            // Clean the output folder if requested
            if (Settings.GetBool(Keys.CleanOutputPath))
            {
                CleanOutputPath();
            }

            // Create the pipeline phases
            if (_phases == null)
            {
                _phases = GetPipelinePhases(_pipelines, _logger);
            }

            // Get phase tasks
            Task[] phaseTasks = null;
            try
            {
                // Get and execute all phases
                phaseTasks = GetPhaseTasks(executionId, phaseResults, cancellationTokenSource);
                await Task.WhenAll(phaseTasks);
            }
            catch (Exception ex)
            {
                if (!(ex is OperationCanceledException))
                {
                    _logger.LogCritical("Error during execution");
                }
            }
            stopwatch.Stop();

            // Raise after event
            await Events.RaiseAsync(new AfterEngineExecution(this, executionId, outputs, stopwatch.ElapsedMilliseconds));

            // Log execution summary table
            _logger.LogInformation(
                "Execution summary (number of output documents per pipeline and phase):"
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

        // The result array is sorted based on dependencies
        private static PipelinePhase[] GetPipelinePhases(PipelineCollection pipelines, ILogger logger)
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
            foreach (PipelinePhases pipelinePhases in phases.Values.Where(x => !x.Isolated))
            {
                pipelinePhases.Transform.Dependencies =
                    pipelinePhases.Transform.Dependencies
                        .Concat(phases.Values.Where(x => x != pipelinePhases && !x.Isolated).Select(x => x.Process))
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
                    // This is an isolated pipeline so just add the phases in a chain
                    pipelinePhases = new PipelinePhases(true);
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
                                throw new Exception($"Could not find pipeline dependency {dependencyName} of {name}");
                            }
                            if (dependency.Isolated)
                            {
                                throw new Exception($"Pipeline {name} has dependency on isolated pipeline {dependencyName}");
                            }
                            processDependencies.Add(Visit(dependencyName, dependency).Process);
                        }
                    }

                    // Add the phases (by this time all dependencies should have been added)
                    pipelinePhases = new PipelinePhases(false);
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
                    throw new Exception($"Pipeline cyclical dependency detected involving {name}");
                }

                return pipelinePhases;
            }
        }

        private Task[] GetPhaseTasks(
            Guid executionId,
            ConcurrentDictionary<string, PhaseResult[]> phaseResults,
            CancellationTokenSource cancellationTokenSource)
        {
            Dictionary<PipelinePhase, Task> phaseTasks = new Dictionary<PipelinePhase, Task>();
            foreach (PipelinePhase phase in _phases)
            {
                phaseTasks.Add(phase, GetPhaseTaskAsync(executionId, phaseResults, phaseTasks, phase, cancellationTokenSource));
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
                return Task.Run(() => phase.ExecuteAsync(this, executionId, phaseResults, cancellationTokenSource), cancellationTokenSource.Token);
            }

            // We have to explicitly wait the execution task in the continuation function
            // (the continuation task doesn't wait for the tasks it continues)
            return Task.Factory.ContinueWhenAll(
                phase.Dependencies.Select(x => phaseTasks[x]).ToArray(),
                dependencies =>
                {
                    // Only run the dependent task if all the dependencies successfully completed
                    if (dependencies.All(x => x.IsCompletedSuccessfully))
                    {
                        Task.WaitAll(new Task[] { phase.ExecuteAsync(this, executionId, phaseResults, cancellationTokenSource) }, cancellationTokenSource.Token);
                    }
                    else
                    {
                        // Otherwise, throw an exception so that this dependency is also skipped by it's dependents
                        string error = $"Skipping pipeline {phase.PipelineName}/{phase.Phase} due to dependency error";
                        _logger.LogError(error);
                        throw new Exception(error);
                    }
                }, cancellationTokenSource.Token);
        }

        // This executes the specified modules with the specified input documents
        internal static async Task<ImmutableArray<IDocument>> ExecuteModulesAsync(ExecutionContextData contextData, IExecutionContext parent, IEnumerable<IModule> modules, ImmutableArray<IDocument> inputs, ILogger logger)
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
                        logger.LogDebug("Executing module {0} with {1} input document(s)", moduleName, inputs.Length);
                        ExecutionContext moduleContext = new ExecutionContext(contextData, parent, module, inputs);

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
                        logger.LogDebug(
                            "Executed module {0} in {1} ms resulting in {2} output document(s)",
                            moduleName,
                            stopwatch.ElapsedMilliseconds,
                            outputs.Length);
                        inputs = outputs;
                    }
                    catch (Exception ex)
                    {
                        if (!(ex is OperationCanceledException))
                        {
                            logger.LogError($"Error while executing module {moduleName}: {ex.Message}");
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
            public PipelinePhases(bool isolated)
            {
                Isolated = isolated;
            }

            public bool Isolated { get; }
            public PipelinePhase Input { get; set; }
            public PipelinePhase Process { get; set; }
            public PipelinePhase Transform { get; set; }
            public PipelinePhase Output { get; set; }
        }
    }
}
