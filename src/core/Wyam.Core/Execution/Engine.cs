using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JavaScriptEngineSwitcher.Core;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Shortcodes;
using Wyam.Common.Tracing;
using Wyam.Core.Configuration;
using Wyam.Core.Documents;
using Wyam.Core.IO;
using Wyam.Core.Shortcodes;
using Wyam.Core.Tracing;
using Wyam.Core.Util;

namespace Wyam.Core.Execution
{
    /// <summary>
    /// The engine is the primary entry point for the generation process.
    /// </summary>
    public class Engine : IEngine, IDisposable
    {
        /// <summary>
        /// Gets the version of Wyam currently being used.
        /// </summary>
        public static string Version
        {
            get
            {
                if (!(Attribute.GetCustomAttribute(typeof(Engine).Assembly, typeof(AssemblyInformationalVersionAttribute)) is AssemblyInformationalVersionAttribute versionAttribute))
                {
                    throw new Exception("Something went terribly wrong, could not determine Wyam version");
                }
                return versionAttribute.InformationalVersion;
            }
        }

        private readonly FileSystem _fileSystem = new FileSystem();
        private readonly Settings _settings = new Settings();
        private readonly ShortcodeCollection _shortcodes = new ShortcodeCollection();
        private readonly PipelineCollection _pipelines = new PipelineCollection();
        private readonly DiagnosticsTraceListener _diagnosticsTraceListener = new DiagnosticsTraceListener();

        private IDocumentFactory _documentFactory;

        // Gets initialized on first execute
        private PipelinePhase[] _phases;

        private bool _disposed;

        /// <summary>
        /// Creates the engine.
        /// </summary>
        public Engine()
        {
            System.Diagnostics.Trace.Listeners.Add(_diagnosticsTraceListener);
            _documentFactory = new DocumentFactory(_settings);
        }

        /// <summary>
        /// Gets the file system.
        /// </summary>
        public IFileSystem FileSystem => _fileSystem;

        /// <summary>
        /// Gets the settings.
        /// </summary>
        public ISettings Settings => _settings;

        /// <summary>
        /// Gets the shortcodes.
        /// </summary>
        public IShortcodeCollection Shortcodes => _shortcodes;

        /// <summary>
        /// Gets the pipelines.
        /// </summary>
        public IPipelineCollection Pipelines => _pipelines;

        internal ConcurrentDictionary<string, ImmutableArray<IDocument>> Documents { get; }
            = new ConcurrentDictionary<string, ImmutableArray<IDocument>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the namespaces that should be brought in scope by modules that support dynamic compilation.
        /// </summary>
        public INamespacesCollection Namespaces { get; } = new NamespaceCollection();

        /// <summary>
        /// Gets a collection of all the raw assemblies that should be referenced by modules
        /// that support dynamic compilation (such as configuration assemblies).
        /// </summary>
        public IRawAssemblyCollection DynamicAssemblies { get; } = new RawAssemblyCollection();

        /// <inheritdoc />
        public IMemoryStreamFactory MemoryStreamManager { get; } = new MemoryStreamFactory();

        /// <summary>
        /// Gets or sets the application input.
        /// </summary>
        public string ApplicationInput { get; set; }

        /// <summary>
        /// Gets or sets the document factory.
        /// </summary>
        public IDocumentFactory DocumentFactory
        {
            get
            {
                return _documentFactory;
            }

            set
            {
                _documentFactory = value ?? throw new ArgumentNullException(nameof(DocumentFactory));
            }
        }

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
        public async Task ExecuteAsync(IServiceProvider serviceProvider)
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

            try
            {
                System.Diagnostics.Stopwatch engineStopwatch = System.Diagnostics.Stopwatch.StartNew();
                Trace.Information("Executing {0} pipelines", _pipelines.Count);

                // Setup (clear the document collection)
                Documents.Clear();

                // Get and execute all phases
                Guid executionId = Guid.NewGuid();
                Task[] phaseTasks = GetPhaseTasks(executionId, serviceProvider);
                await Task.WhenAll(phaseTasks);

                // Clean up (dispose documents)
                // Note that disposing the documents immediately after engine execution will ensure write streams get flushed and released
                // but will also mean that callers (and tests) can't access documents and document content after the engine finishes
                // Easiest way to access content after engine execution is to add a final Meta module and copy content to metadata
                foreach (PipelinePhase phase in _phases)
                {
                    phase.ResetClonedDocuments();
                }

                engineStopwatch.Stop();
                Trace.Information($"Finished execution in {engineStopwatch.ElapsedMilliseconds} ms");
            }
            catch (Exception ex)
            {
                Trace.Critical("Exception during execution: {0}", ex.ToString());
                throw;
            }
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

            // Make a pass through non-isolated render phases to set dependencies to all non-isolated process phases
            foreach (PipelinePhases pipelinePhases in phases.Values.Where(x => !x.Isolated))
            {
                pipelinePhases.Render.Dependencies =
                    pipelinePhases.Render.Dependencies
                        .Concat(phases.Values.Where(x => x != pipelinePhases && !x.Isolated).Select(x => x.Process))
                        .ToArray();
            }

            return sortedPhases
                .Select(x => x.Read)
                .Concat(sortedPhases.Select(x => x.Process))
                .Concat(sortedPhases.Select(x => x.Render))
                .Concat(sortedPhases.Select(x => x.Write))
                .ToArray();

            // Returns the process phases (if not isolated)
            PipelinePhases Visit(string name, IPipeline pipeline)
            {
                PipelinePhases pipelinePhases;

                if (pipeline.Isolated)
                {
                    // This is an isolated pipeline so just add the phases in a chain
                    pipelinePhases = new PipelinePhases(true);
                    pipelinePhases.Read = new PipelinePhase(pipeline, name, Phase.Read, pipeline.ReadModules);
                    pipelinePhases.Process = new PipelinePhase(pipeline, name, Phase.Process, pipeline.ProcessModules,  pipelinePhases.Read);
                    pipelinePhases.Render = new PipelinePhase(pipeline, name, Phase.Render, pipeline.RenderModules, pipelinePhases.Process);
                    pipelinePhases.Write = new PipelinePhase(pipeline, name, Phase.Write, pipeline.WriteModules, pipelinePhases.Render);
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
                    pipelinePhases.Read = new PipelinePhase(pipeline, name, Phase.Read, pipeline.ReadModules);
                    processDependencies.Insert(0, pipelinePhases.Read);  // Makes sure the process phase is also dependent on it's read phase
                    pipelinePhases.Process = new PipelinePhase(pipeline, name, Phase.Process, pipeline.ProcessModules, processDependencies.ToArray());
                    pipelinePhases.Render = new PipelinePhase(pipeline, name, Phase.Render, pipeline.RenderModules, pipelinePhases.Process);  // Render dependencies will be added after all pipelines have been processed
                    pipelinePhases.Write = new PipelinePhase(pipeline, name, Phase.Write, pipeline.WriteModules, pipelinePhases.Render);
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

        private Task[] GetPhaseTasks(Guid executionId, IServiceProvider serviceProvider)
        {
            Dictionary<PipelinePhase, Task> phaseTasks = new Dictionary<PipelinePhase, Task>();
            foreach (PipelinePhase phase in _phases)
            {
                phaseTasks.Add(phase, GetTask(phase));
            }
            return phaseTasks.Values.ToArray();

            Task GetTask(PipelinePhase taskPhase)
            {
                if (taskPhase.Dependencies.Length == 0)
                {
                    // This will immediatly queue the read phase while we continue figuring out tasks, but that's okay
                    return Task.Run(() => taskPhase.ExecuteAsync(this, executionId, serviceProvider, ImmutableArray<IDocument>.Empty));
                }

                // We have to explicitly wait the execution task in the continuation function
                // (the continuation task doesn't wait for the tasks it continues)
                return Task.Factory.ContinueWhenAll(
                    taskPhase.Dependencies.Select(x => phaseTasks[x]).ToArray(),
                    _ => Task.WaitAll(taskPhase.ExecuteAsync(this, executionId, serviceProvider, taskPhase.Dependencies[0].OutputDocuments)));
            }
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
                    System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
                    Trace.Verbose("Executing module {0} with {1} input document(s)", moduleName, inputDocuments.Length);

                    try
                    {
                        // Execute the module
                        using (ExecutionContext moduleContext = context.Clone(module))
                        {
                            IEnumerable<IDocument> moduleResult = await module.ExecuteAsync(inputDocuments, moduleContext);
                            resultDocuments = moduleResult?.Where(x => x != null).ToImmutableArray() ?? ImmutableArray<IDocument>.Empty;
                        }

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
                        Trace.Error($"Error while executing module {moduleName}: {ex.Message}");
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
            public PipelinePhase Read { get; set; }
            public PipelinePhase Process { get; set; }
            public PipelinePhase Render { get; set; }
            public PipelinePhase Write { get; set; }
        }
    }
}
