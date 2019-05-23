using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using ConcurrentCollections;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.Tracing;
using Microsoft.Extensions.DependencyInjection;
using Wyam.Core.Modules.Control;

namespace Wyam.Core.Execution
{
    /// <summary>
    /// This contains the pipeline execution context and other data
    /// needed to execute a pipeline and cache it's results.
    /// </summary>
    internal class PipelinePhase : IDisposable
    {
        private readonly IList<IModule> _modules;
        private readonly ConcurrentHashSet<FilePath> _documentSources = new ConcurrentHashSet<FilePath>();
        private ConcurrentHashSet<IDocument> _clonedDocuments = new ConcurrentHashSet<IDocument>();
        private bool _disposed;

        public PipelinePhase(IPipeline pipeline, string pipelineName, Phase phase, IList<IModule> modules, params PipelinePhase[] dependencies)
        {
            Pipeline = pipeline;
            PipelineName = pipelineName;
            Phase = phase;
            _modules = modules ?? new List<IModule>();
            Dependencies = dependencies ?? Array.Empty<PipelinePhase>();
        }

        public IPipeline Pipeline { get; }

        public string PipelineName { get; }

        public Phase Phase { get; }

        /// <summary>
        /// Contains direct dependencies for this pipeline and phase.
        /// The first dependency should contain the input documents for this phase.
        /// </summary>
        public PipelinePhase[] Dependencies { get; set; }

        /// <summary>
        /// Holds the output documents from the previous execution of this phase.
        /// </summary>
        public ImmutableArray<IDocument> OutputDocuments { get; private set; } = ImmutableArray<IDocument>.Empty;

        /// <summary>
        /// The first dependency always holds the input documents for this phase.
        /// </summary>
        /// <returns>The input documents for this phase.</returns>
        private ImmutableArray<IDocument> GetInputDocuments() => Dependencies.Length == 0 ? ImmutableArray<IDocument>.Empty : Dependencies[0].OutputDocuments;

        // This is the main execute method called by the engine
        public async Task ExecuteAsync(
            Engine engine,
            Guid executionId,
            IServiceProvider serviceProvider)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(PipelinePhase));
            }

            if (_modules.Count == 0)
            {
                Trace.Verbose($"Pipeline {PipelineName}/{Phase} contains no modules, skipping");
                OutputDocuments = GetInputDocuments();
                return;
            }

            // Setup for pipeline execution
            _documentSources.Clear();
            ResetClonedDocuments();

            System.Diagnostics.Stopwatch pipelineStopwatch = System.Diagnostics.Stopwatch.StartNew();
            Trace.Verbose($"Executing pipeline {PipelineName}/{Phase} with {_modules.Count} module(s)");
            try
            {
                // Execute all modules in the pipeline with a new DI scope per phase
                IServiceScopeFactory serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
                using (IServiceScope serviceScope = serviceScopeFactory.CreateScope())
                {
                    using (ExecutionContext context = new ExecutionContext(engine, executionId, this, serviceScope.ServiceProvider))
                    {
                        OutputDocuments = await Engine.ExecuteAsync(context, _modules, GetInputDocuments());
                        pipelineStopwatch.Stop();
                        Trace.Information($"Executed pipeline {PipelineName}/{Phase} in {pipelineStopwatch.ElapsedMilliseconds} ms resulting in {OutputDocuments.Length} output document(s)");
                    }
                }
            }
            catch (Exception)
            {
                Trace.Error($"Error while executing pipeline {PipelineName}/{Phase}");
                OutputDocuments = ImmutableArray<IDocument>.Empty;
                throw;
            }

            // Store the result documents, but only if this is the Process phase of a non-isolated pipeline
            if (!Pipeline.Isolated && Phase == Phase.Process)
            {
                engine.Documents.AddOrUpdate(
                    PipelineName,
                    OutputDocuments,
                    (_, __) => OutputDocuments);
            }

            // Dispose documents that aren't part of the final collection for this pipeline,
            // but don't dispose any documents that are referenced directly or indirectly from the final ones
            HashSet<IDocument> flattenedResultDocuments = new HashSet<IDocument>();
            foreach (IDocument outputDocument in OutputDocuments)
            {
                outputDocument.Flatten(flattenedResultDocuments);
            }
            Parallel.ForEach(_clonedDocuments.Where(x => !flattenedResultDocuments.Contains(x)), x => x.Dispose());

            // Track remaining outputs *tracked by this phase* to dispose them after overall execution is done
            _clonedDocuments = new ConcurrentHashSet<IDocument>(flattenedResultDocuments.Where(x => _clonedDocuments.Contains(x)));
        }

        public void AddClonedDocument(IDocument document) => _clonedDocuments.Add(document);

        public void AddDocumentSource(FilePath source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (!_documentSources.Add(source))
            {
                throw new ArgumentException("Document source must be unique within the pipeline: " + source);
            }
        }

        /// <summary>
        /// Removes a document from disposal tracking, making it the responsibility of the caller to dispose the document.
        /// This method should only be used when you want the module to take over document lifetime (such as caching between executions).
        /// Note that a prior module might have otherwise removed the document from tracking in which case this method will return
        /// <c>false</c> and the caller should not attempt to dispose the document.
        /// </summary>
        /// <param name="document">The document to stop tracking.</param>
        /// <returns><c>true</c> if the document was being tracked and the caller should now be responsible for it, <c>false</c> otherwise.</returns>
        public bool Untrack(IDocument document) => _clonedDocuments.TryRemove(document);

        /// <summary>
        /// Disposes all remaining cloned documents from this phase.
        /// Called at the end of execution after all pipelines have run.
        /// </summary>
        public void ResetClonedDocuments()
        {
            Parallel.ForEach(_clonedDocuments, x => x.Dispose());
            _clonedDocuments = new ConcurrentHashSet<IDocument>();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Pipeline));
            }
            _disposed = true;

            // Clean up the documents
            ResetClonedDocuments();

            // Clean up the modules
            DisposeModules(_modules);
        }

        private static void DisposeModules(IEnumerable<IModule> modules)
        {
            foreach (IModule module in modules)
            {
                (module as IDisposable)?.Dispose();
                if (module is IEnumerable<IModule> childModules)
                {
                    DisposeModules(childModules);
                }
            }
        }
    }
}
