using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Core.Execution;

namespace Wyam.Core.Documents
{
    internal class DocumentCollection : IDocumentCollection
    {
        private readonly ConcurrentDictionary<string, ImmutableArray<IDocument>> _documents;
        private readonly PipelinePhase _pipelinePhase;
        private readonly IPipelineCollection _pipelines;

        public DocumentCollection(ConcurrentDictionary<string, ImmutableArray<IDocument>> documents, PipelinePhase pipelinePhase, IPipelineCollection pipelines)
        {
            _documents = documents;
            _pipelinePhase = pipelinePhase;
            _pipelines = pipelines;
        }

        public IEnumerator<IDocument> GetEnumerator()
        {
            Check();
            return GetDocumentsFromDependencies().SelectMany(x => x.Value).Distinct().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IReadOnlyDictionary<string, IEnumerable<IDocument>> ByPipeline()
        {
            Check();
            return GetDocumentsFromDependencies().ToDictionary(x => x.Key, x => x.Value.Distinct());
        }

        public IEnumerable<IDocument> FromPipeline(string pipeline)
        {
            Check(pipeline);
            return _documents[pipeline].Distinct();
        }

        public IEnumerable<IDocument> ExceptPipeline(string pipeline)
        {
            Check(pipeline, false);
            return GetDocumentsFromDependencies().Where(x => x.Key != pipeline).SelectMany(x => x.Value).Distinct();
        }

        public IEnumerable<IDocument> this[string pipline] => FromPipeline(pipline);

        // If in the process phase only get documents from dependencies (including transient ones)
        private IEnumerable<KeyValuePair<string, ImmutableArray<IDocument>>> GetDocumentsFromDependencies()
        {
            if (_pipelinePhase.Phase == Phase.Render)
            {
                return _documents;
            }

            HashSet<string> transientDependencies = GatherProcessPhaseDependencies(_pipelinePhase);
            return _documents.Where(x => transientDependencies.Contains(x.Key));
        }

        private HashSet<string> GatherProcessPhaseDependencies(PipelinePhase phase, HashSet<string> transientDependencies = null)
        {
            if (transientDependencies == null)
            {
                transientDependencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
            foreach (PipelinePhase dependency in phase.Dependencies.Where(x => x.Phase == Phase.Process))
            {
                transientDependencies.Add(dependency.PipelineName);
                GatherProcessPhaseDependencies(dependency, transientDependencies);
            }
            return transientDependencies;
        }

        private void Check(string pipeline, bool phaseChecks = true)
        {
            Check();
            if (string.IsNullOrEmpty(pipeline))
            {
                throw new ArgumentException(nameof(pipeline));
            }
            if (!_pipelines.ContainsKey(pipeline))
            {
                throw new KeyNotFoundException($"The pipeline {pipeline} could not be found");
            }
            if (_pipelines[pipeline].Isolated)
            {
                throw new InvalidOperationException($"Cannot access documents in isolated pipeline {pipeline}");
            }
            if (phaseChecks && _pipelinePhase.Phase == Phase.Process)
            {
                if (pipeline.Equals(_pipelinePhase.PipelineName, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"Cannot access documents from currently executing pipeline {pipeline} while in the {nameof(Phase.Process)} phase");
                }
                if (!GatherProcessPhaseDependencies(_pipelinePhase).Contains(pipeline))
                {
                    throw new InvalidOperationException($"Cannot access documents from pipeline {pipeline} without a dependency while in the {nameof(Phase.Process)} phase");
                }
            }
        }

        private void Check()
        {
            if (_pipelines[_pipelinePhase.PipelineName].Isolated)
            {
                throw new InvalidOperationException($"Cannot access documents from inside isolated pipeline {_pipelinePhase.PipelineName}");
            }
            if (_pipelinePhase.Phase != Phase.Process && _pipelinePhase.Phase != Phase.Render)
            {
                throw new InvalidOperationException($"Cannot access the document collection during the {_pipelinePhase.Phase} phase");
            }
        }
    }
}
