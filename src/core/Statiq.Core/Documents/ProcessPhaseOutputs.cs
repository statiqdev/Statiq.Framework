using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Returns pipeline outputs from the process phase of pipelines for use within modules during execution.
    /// </summary>
    internal class ProcessPhaseOutputs : IPipelineOutputs
    {
        private readonly IReadOnlyDictionary<string, PhaseResult[]> _phaseResults;
        private readonly PipelinePhase _currentPhase;
        private readonly IPipelineCollection _pipelines;

        // Cache the dependency calculation
        private KeyValuePair<string, ImmutableArray<IDocument>>[] _cachedDependencyOutputs;

        public ProcessPhaseOutputs(IReadOnlyDictionary<string, PhaseResult[]> phaseResults, PipelinePhase currentPhase, IPipelineCollection pipelines)
        {
            _phaseResults = phaseResults ?? throw new ArgumentNullException(nameof(phaseResults));
            _currentPhase = currentPhase ?? throw new ArgumentNullException(nameof(currentPhase));
            _pipelines = pipelines ?? throw new ArgumentNullException(nameof(pipelines));
        }

        public IEnumerator<IDocument> GetEnumerator()
        {
            CheckCurrentPhase();
            return GetOutputsFromDependencies().SelectMany(x => x.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IReadOnlyDictionary<string, ImmutableArray<IDocument>> ByPipeline()
        {
            CheckCurrentPhase();
            return GetOutputsFromDependencies().ToDictionary(x => x.Key, x => x.Value);
        }

        public ImmutableArray<IDocument> FromPipeline(string pipelineName)
        {
            Check(pipelineName);
            return _phaseResults[pipelineName][(int)Phase.Process].Outputs;
        }

        public IEnumerable<IDocument> ExceptPipeline(string pipelineName)
        {
            Check(pipelineName, false);
            return GetOutputsFromDependencies()
                .Where(x => !x.Key.Equals(pipelineName, StringComparison.OrdinalIgnoreCase))
                .SelectMany(x => x.Value);
        }

        // If in the process phase only get documents from dependencies (including transient ones)
        private IEnumerable<KeyValuePair<string, ImmutableArray<IDocument>>> GetOutputsFromDependencies()
        {
            if (_currentPhase.Phase == Phase.Transform)
            {
                return _phaseResults
                    .Where(x => x.Value[(int)Phase.Process] != null)
                    .Select(x => KeyValuePair.Create(x.Key, x.Value[(int)Phase.Process].Outputs))
                    .ToArray();
            }

            if (_cachedDependencyOutputs != null)
            {
                return _cachedDependencyOutputs;
            }

            HashSet<string> transientDependencies = GatherProcessPhaseDependencies(_currentPhase);
            _cachedDependencyOutputs = _phaseResults
                .Where(x => transientDependencies.Contains(x.Key) && x.Value[(int)Phase.Process] != null)
                .Select(x => KeyValuePair.Create(x.Key, x.Value[(int)Phase.Process].Outputs))
                .ToArray();
            return _cachedDependencyOutputs;
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

        private void Check(string pipelineName, bool pipelineChecks = true)
        {
            CheckCurrentPhase();
            if (string.IsNullOrEmpty(pipelineName))
            {
                throw new ArgumentException(nameof(pipelineName));
            }
            if (!_pipelines.ContainsKey(pipelineName))
            {
                throw new KeyNotFoundException($"The pipeline {pipelineName} could not be found");
            }
            if (pipelineChecks)
            {
                if (!_phaseResults.TryGetValue(pipelineName, out PhaseResult[] phaseResults))
                {
                    throw new KeyNotFoundException($"The pipeline results for {pipelineName} could not be found");
                }
                if (phaseResults[(int)Phase.Process] == null)
                {
                    throw new KeyNotFoundException($"Process outputs for pipeline {pipelineName} could not be found");
                }
                if (_pipelines[pipelineName].Isolated)
                {
                    throw new InvalidOperationException($"Cannot access documents in isolated pipeline {pipelineName}");
                }
                if (_currentPhase.Phase == Phase.Process)
                {
                    if (pipelineName.Equals(_currentPhase.PipelineName, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException($"Cannot access documents from currently executing pipeline {pipelineName} while in the {nameof(Phase.Process)} phase");
                    }
                    if (!GatherProcessPhaseDependencies(_currentPhase).Contains(pipelineName))
                    {
                        throw new InvalidOperationException($"Cannot access documents from pipeline {pipelineName} without a dependency while in the {nameof(Phase.Process)} phase");
                    }
                }
            }
        }

        private void CheckCurrentPhase()
        {
            if (_pipelines[_currentPhase.PipelineName].Isolated)
            {
                throw new InvalidOperationException($"Cannot access documents from inside isolated pipeline {_currentPhase.PipelineName}");
            }
            if (_currentPhase.Phase != Phase.Process && _currentPhase.Phase != Phase.Transform)
            {
                throw new InvalidOperationException($"Cannot access the document collection during the {_currentPhase.Phase} phase");
            }
        }
    }
}
