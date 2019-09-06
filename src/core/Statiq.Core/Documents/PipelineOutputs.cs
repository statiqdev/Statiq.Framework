using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Statiq.Common;

namespace Statiq.Core
{
    internal class PipelineOutputs : IPipelineOutputs
    {
        private readonly ConcurrentDictionary<string, ImmutableArray<IDocument>> _documents;
        private readonly PipelinePhase _pipelinePhase;
        private readonly IPipelineCollection _pipelines;

        // Cache the dependency calculation
        private KeyValuePair<string, ImmutableArray<IDocument>>[] _cachedDependencies;

        public PipelineOutputs(ConcurrentDictionary<string, ImmutableArray<IDocument>> documents, PipelinePhase pipelinePhase, IPipelineCollection pipelines)
        {
            _documents = documents;
            _pipelinePhase = pipelinePhase;
            _pipelines = pipelines;
        }

        public IEnumerator<IDocument> GetEnumerator()
        {
            Check();
            return GetDocumentsFromDependencies().SelectMany(x => x.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IReadOnlyDictionary<string, ImmutableArray<IDocument>> ByPipeline()
        {
            Check();
            return GetDocumentsFromDependencies().ToDictionary(x => x.Key, x => x.Value);
        }

        public ImmutableArray<IDocument> FromPipeline(string pipeline)
        {
            Check(pipeline);
            return _documents[pipeline];
        }

        public IEnumerable<IDocument> ExceptPipeline(string pipeline)
        {
            Check(pipeline, false);
            return GetDocumentsFromDependencies().Where(x => x.Key != pipeline).SelectMany(x => x.Value);
        }

        // If in the process phase only get documents from dependencies (including transient ones)
        private IEnumerable<KeyValuePair<string, ImmutableArray<IDocument>>> GetDocumentsFromDependencies()
        {
            if (_pipelinePhase.Phase == Phase.Transform)
            {
                return _documents;
            }

            if (_cachedDependencies != null)
            {
                return _cachedDependencies;
            }

            HashSet<string> transientDependencies = GatherProcessPhaseDependencies(_pipelinePhase);
            _cachedDependencies = _documents.Where(x => transientDependencies.Contains(x.Key)).ToArray();
            return _cachedDependencies;
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
            if (_pipelinePhase.Phase != Phase.Process && _pipelinePhase.Phase != Phase.Transform)
            {
                throw new InvalidOperationException($"Cannot access the document collection during the {_pipelinePhase.Phase} phase");
            }
        }
    }
}
