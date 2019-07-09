using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JavaScriptEngineSwitcher.Core;
using Statiq.Common.Configuration;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.IO;
using Statiq.Common.Meta;
using Statiq.Common.Modules;
using Statiq.Common.Shortcodes;
using Statiq.Common.Tracing;
using Statiq.Core.Configuration;
using Statiq.Core.Documents;
using Statiq.Core.IO;
using Statiq.Core.Shortcodes;
using Statiq.Core.Tracing;
using Statiq.Core.Util;

namespace Statiq.Core.Execution
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

        private readonly FileSystem _fileSystem = new FileSystem();
        private readonly Settings _settings = new Settings();
        private readonly ShortcodeCollection _shortcodes = new ShortcodeCollection();
        private readonly PipelineCollection _pipelines = new PipelineCollection();
        private readonly DiagnosticsTraceListener _diagnosticsTraceListener = new DiagnosticsTraceListener();

        // Gets initialized on first execute
        private PipelinePhase[] _phases;

        private bool _disposed;

        /// <summary>
        /// Creates the engine.
        /// </summary>
        public Engine()
        {
            System.Diagnostics.Trace.Listeners.Add(_diagnosticsTraceListener);
            DocumentFactory = new DocumentFactory(_settings);
        }

        /// <inheritdoc />
        public IFileSystem FileSystem => _fileSystem;

        /// <inheritdoc />
        public ISettings Settings => _settings;

        /// <inheritdoc />
        public IShortcodeCollection Shortcodes => _shortcodes;

        /// <inheritdoc />
        public IPipelineCollection Pipelines => _pipelines;

        internal ConcurrentDictionary<string, ImmutableArray<IDocument>> Documents { get; }
            = new ConcurrentDictionary<string, ImmutableArray<IDocument>>(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc />
        public INamespacesCollection Namespaces { get; } = new NamespaceCollection();

        /// <inheritdoc />
        public IRawAssemblyCollection DynamicAssemblies { get; } = new RawAssemblyCollection();

        /// <inheritdoc />
        public IMemoryStreamFactory MemoryStreamFactory { get; } = new MemoryStreamFactory();

        /// <inheritdoc />
        public string ApplicationInput { get; set; }

        /// <inheritdoc />
        public DocumentFactory DocumentFactory { get; }

        /// <summary>
        /// Deletes the output path and all files it contains.
        /// </summary>
        public async Task CleanOutputPathAsync()
        {
            try
            {
                Trace.Information("Cleaning output path: {0}", FileSystem.OutputPath);
                IDirectory outputDirectory = await FileSystem.GetOutputDirectoryAsync();
                if (await outputDirectory.GetExistsAsync())
                {
                    await outputDirectory.DeleteAsync(true);
                }
                Trace.Information("Cleaned output directory");
            }
            catch (Exception ex)
            {
                Trace.Warning("Error while cleaning output path: {0} - {1}", ex.GetType(), ex.Message);
            }
        }

        /// <summary>
        /// Deletes the temp path and all files it contains.
        /// </summary>
        public async Task CleanTempPathAsync()
        {
            try
            {
                Trace.Information("Cleaning temp path: {0}", FileSystem.TempPath);
                IDirectory tempDirectory = await FileSystem.GetTempDirectoryAsync();
                if (await tempDirectory.GetExistsAsync())
                {
                    await tempDirectory.DeleteAsync(true);
                }
                Trace.Information("Cleaned temp directory");
            }
            catch (Exception ex)
            {
                Trace.Warning("Error while cleaning temp path: {0} - {1}", ex.GetType(), ex.Message);
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
        public async Task ExecuteAsync(IServiceProvider serviceProvider, CancellationTokenSource cancellationTokenSource)
        {
            // Remove the synchronization context
            await default(SynchronizationContextRemover);

            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            CheckDisposed();

            Trace.Information($"Using {JsEngineSwitcher.Current.DefaultEngineName} as the JavaScript engine");

            // Make sure we've actually configured some pipelines
            if (_pipelines.Count == 0)
            {
                Trace.Error("No pipelines are configured.");
                return;
            }

            // Do a check for the same input/output path
            if (FileSystem.InputPaths.Any(x => x.Equals(FileSystem.OutputPath)))
            {
                Trace.Warning("The output path is also one of the input paths which can cause unexpected behavior and is usually not advised");
            }

            await CleanTempPathAsync();

            // Clean the output folder if requested
            if (Settings.Bool(Keys.CleanOutputPath))
            {
                await CleanOutputPathAsync();
            }

            // Create the pipeline phases
            if (_phases == null)
            {
                _phases = GetPipelinePhases(_pipelines);
            }

            // Start the timer
            Guid executionId = Guid.NewGuid();
            System.Diagnostics.Stopwatch engineStopwatch = System.Diagnostics.Stopwatch.StartNew();
            Trace.Information($"Executing {_pipelines.Count} pipelines (execution ID {executionId})");

            // Setup (clear the document collection)
            Documents.Clear();

            // Get phase tasks
            Task[] phaseTasks = null;
            try
            {
                // Get and execute all phases
                phaseTasks = GetPhaseTasks(executionId, serviceProvider, cancellationTokenSource);
                await Task.WhenAll(phaseTasks);
            }
            catch (Exception ex)
            {
                if (!(ex is OperationCanceledException))
                {
                    Trace.Critical("Error during execution");
                }
            }

            // Stop the timer
            engineStopwatch.Stop();
            Trace.Information($"Finished execution in {engineStopwatch.ElapsedMilliseconds} ms");
        }

        // The result array is sorted based on dependencies
        private static PipelinePhase[] GetPipelinePhases(PipelineCollection pipelines)
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
                    pipelinePhases.Input = new PipelinePhase(pipeline, name, Phase.Input, pipeline.InputModules);
                    pipelinePhases.Process = new PipelinePhase(pipeline, name, Phase.Process, pipeline.ProcessModules,  pipelinePhases.Input);
                    pipelinePhases.Transform = new PipelinePhase(pipeline, name, Phase.Transform, pipeline.TransformModules, pipelinePhases.Process);
                    pipelinePhases.Output = new PipelinePhase(pipeline, name, Phase.Output, pipeline.OutputModules, pipelinePhases.Transform);
                    phases.Add(name, pipelinePhases);
                    sortedPhases.Add(pipelinePhases);
                    return pipelinePhases;
                }

                if (visited.Add(name))
                {
                    // Visit dependencies if this isn't an isolated pipeline
                    List<PipelinePhase> processDependencies = new List<PipelinePhase>();
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

                    // Add the phases (by this time all dependencies should have been added)
                    pipelinePhases = new PipelinePhases(false);
                    pipelinePhases.Input = new PipelinePhase(pipeline, name, Phase.Input, pipeline.InputModules);
                    processDependencies.Insert(0, pipelinePhases.Input);  // Makes sure the process phase is also dependent on it's input phase
                    pipelinePhases.Process = new PipelinePhase(pipeline, name, Phase.Process, pipeline.ProcessModules, processDependencies.ToArray());
                    pipelinePhases.Transform = new PipelinePhase(pipeline, name, Phase.Transform, pipeline.TransformModules, pipelinePhases.Process);  // Transform dependencies will be added after all pipelines have been processed
                    pipelinePhases.Output = new PipelinePhase(pipeline, name, Phase.Output, pipeline.OutputModules, pipelinePhases.Transform);
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

        private Task[] GetPhaseTasks(Guid executionId, IServiceProvider serviceProvider, CancellationTokenSource cancellationTokenSource)
        {
            Dictionary<PipelinePhase, Task> phaseTasks = new Dictionary<PipelinePhase, Task>();
            foreach (PipelinePhase phase in _phases)
            {
                phaseTasks.Add(phase, GetPhaseTask(executionId, serviceProvider, phaseTasks, phase, cancellationTokenSource));
            }
            return phaseTasks.Values.ToArray();
        }

        private Task GetPhaseTask(Guid executionId, IServiceProvider serviceProvider, Dictionary<PipelinePhase, Task> phaseTasks, PipelinePhase phase, CancellationTokenSource cancellationTokenSource)
        {
            if (phase.Dependencies.Length == 0)
            {
                // This will immediately queue the input phase while we continue figuring out tasks, but that's okay
                return Task.Run(() => phase.ExecuteAsync(this, executionId, serviceProvider, cancellationTokenSource), cancellationTokenSource.Token);
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
                        Task.WaitAll(new Task[] { phase.ExecuteAsync(this, executionId, serviceProvider, cancellationTokenSource) }, cancellationTokenSource.Token);
                    }
                    else
                    {
                        // Otherwise, throw an exception so that this dependency is also skipped by it's dependents
                        string error = $"Skipping pipeline {phase.PipelineName}/{phase.Phase} due to dependency error";
                        Trace.Error(error);
                        throw new Exception(error);
                    }
                }, cancellationTokenSource.Token);
        }

        // This executes the specified modules with the specified input documents
        internal static async Task<ImmutableArray<IDocument>> ExecuteAsync(ExecutionContext context, IEnumerable<IModule> modules, ImmutableArray<IDocument> inputDocuments)
        {
            ImmutableArray<IDocument> resultDocuments = ImmutableArray<IDocument>.Empty;
            if (modules != null)
            {
                foreach (IModule module in modules.Where(x => x != null))
                {
                    string moduleName = module.GetType().Name;

                    try
                    {
                        // Check for cancellation
                        context.CancellationToken.ThrowIfCancellationRequested();

                        // Execute the module
                        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
                        Trace.Verbose("Executing module {0} with {1} input document(s)", moduleName, inputDocuments.Length);
                        ExecutionContext moduleContext = context.Clone(module);
                        IEnumerable<IDocument> moduleResult = await module.ExecuteAsync(inputDocuments, moduleContext);
                        resultDocuments = moduleResult?.Where(x => x != null).ToImmutableArray() ?? ImmutableArray<IDocument>.Empty;

                        // Trace results
                        stopwatch.Stop();
                        Trace.Verbose(
                            "Executed module {0} in {1} ms resulting in {2} output document(s)",
                            moduleName,
                            stopwatch.ElapsedMilliseconds,
                            resultDocuments.Length);
                        inputDocuments = resultDocuments;
                    }
                    catch (Exception ex)
                    {
                        if (!(ex is OperationCanceledException))
                        {
                            Trace.Error($"Error while executing module {moduleName}: {ex.Message}");
                        }
                        resultDocuments = ImmutableArray<IDocument>.Empty;
                        throw;
                    }
                }
            }
            return resultDocuments;
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
            CleanTempPathAsync().Wait();
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
