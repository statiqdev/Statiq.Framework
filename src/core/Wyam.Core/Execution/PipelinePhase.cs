using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using ConcurrentCollections;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.Tracing;
using Wyam.Common.Util;
using Wyam.Core.Caching;
using Wyam.Core.Documents;
using Wyam.Core.Meta;
using Microsoft.Extensions.DependencyInjection;

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
        private ConcurrentBag<IDocument> _clonedDocuments = new ConcurrentBag<IDocument>();
        private bool _disposed;

        public PipelinePhase(IPipeline pipeline, string pipelineName, string phaseName, IList<IModule> modules, params PipelinePhase[] dependencies)
        {
            Pipeline = pipeline;
            PipelineName = pipelineName;
            PhaseName = phaseName;
            _modules = modules ?? new List<IModule>();
            Dependencies = dependencies ?? Array.Empty<PipelinePhase>();
        }

        public IPipeline Pipeline { get; }

        public string PipelineName { get; }

        public string PhaseName { get; }

        /// <summary>
        /// The first dependency should contain the input documents for this phase.
        /// </summary>
        public PipelinePhase[] Dependencies { get; set; }

        /// <summary>
        /// Holds the output documents from the previous execution of this phase.
        /// </summary>
        public ImmutableArray<IDocument> OutputDocuments { get; private set; } = ImmutableArray<IDocument>.Empty;

        // This is the main execute method called by the engine
        public async Task ExecuteAsync(
            Engine engine,
            Guid executionId,
            IServiceProvider serviceProvider,
            ImmutableArray<IDocument> inputDocuments)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(PipelinePhase));
            }

            if (_modules.Count == 0)
            {
                Trace.Information($"{PipelineName}/{PhaseName} contains no modules, skipping");
                OutputDocuments = inputDocuments;
                return;
            }

            // Setup for pipeline execution
            _documentSources.Clear();
            ResetClonedDocuments();

            System.Diagnostics.Stopwatch pipelineStopwatch = System.Diagnostics.Stopwatch.StartNew();
            using (Trace.WithIndent().Information($"Executing {PipelineName}/{PhaseName} with {_modules.Count} module(s)"))
            {
                try
                {
                    // Execute all modules in the pipeline with a new DI scope per phase
                    IServiceScopeFactory serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
                    using (IServiceScope serviceScope = serviceScopeFactory.CreateScope())
                    {
                        using (ExecutionContext context = new ExecutionContext(engine, executionId, this, serviceScope.ServiceProvider))
                        {
                            OutputDocuments = await Engine.ExecuteAsync(context, _modules, inputDocuments);
                            pipelineStopwatch.Stop();
                            Trace.Information($"Executed {PipelineName}/{PhaseName} in {pipelineStopwatch.ElapsedMilliseconds} ms resulting in {OutputDocuments.Length} output document(s)");
                        }
                    }
                }
                catch (Exception)
                {
                    Trace.Error($"Error while executing {PipelineName}/{PhaseName}");
                    throw;
                }
            }

            // Store the result documents, but only if this is the Process phase of a non-isolated pipeline
            if (!Pipeline.Isolated && PhaseName.Equals(nameof(IPipeline.Process), StringComparison.OrdinalIgnoreCase))
            {
                engine.Documents.AddOrUpdate(
                    PipelineName,
                    OutputDocuments,
                    (_, __) => OutputDocuments);
            }

            // Dispose documents that aren't part of the final collection for this pipeline,
            // but don't dispose any documents that are referenced directly or indirectly from the final ones
            HashSet<IDocument> flattenedResultDocuments = new HashSet<IDocument>();
            FlattenResultDocuments(OutputDocuments, flattenedResultDocuments);
            Parallel.ForEach(_clonedDocuments.Where(x => !flattenedResultDocuments.Contains(x)), x => x.Dispose());
            _clonedDocuments = new ConcurrentBag<IDocument>(flattenedResultDocuments);
        }

        private void FlattenResultDocuments(IEnumerable<IDocument> documents, HashSet<IDocument> flattenedResultDocuments)
        {
            foreach (IDocument document in documents)
            {
                if (document == null || !flattenedResultDocuments.Add(document))
                {
                    continue;
                }

                FlattenResultDocuments(
                    document.Keys.SelectMany(x =>
                    {
                        object value = document.GetRaw(x);
                        IEnumerable<IDocument> children = value as IEnumerable<IDocument>;
                        if (children == null && value is IDocument)
                        {
                            children = new[] { (IDocument)value };
                        }
                        return children ?? Enumerable.Empty<IDocument>();
                    }),
                    flattenedResultDocuments);
            }
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

        public void ResetClonedDocuments()
        {
            Parallel.ForEach(_clonedDocuments, x => x.Dispose());
            _clonedDocuments = new ConcurrentBag<IDocument>();
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
