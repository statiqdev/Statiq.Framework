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
    /// Returns pipeline outputs for use within modules during execution.
    /// </summary>
    internal class PhaseOutputs : IPipelineOutputs
    {
        private readonly IReadOnlyDictionary<string, PhaseResult[]> _phaseResults;
        private readonly PipelinePhase _currentPhase;
        private readonly IPipelineCollection _pipelines;

        // Cache the dependency calculation
        private KeyValuePair<string, ImmutableArray<IDocument>>[] _cachedDependencyOutputs;

        public PhaseOutputs(IReadOnlyDictionary<string, PhaseResult[]> phaseResults, PipelinePhase currentPhase, IPipelineCollection pipelines)
        {
            _phaseResults = phaseResults ?? throw new ArgumentNullException(nameof(phaseResults));
            _currentPhase = currentPhase ?? throw new ArgumentNullException(nameof(currentPhase));
            _pipelines = pipelines ?? throw new ArgumentNullException(nameof(pipelines));
        }

        public ImmutableArray<IDocument> FromPipeline(string pipelineName)
        {
            ValidateCurrent();
            ValidateArguments(pipelineName);

            // If we're in the output phase (which will only happen if this is a deployment module)
            // get documents from the output phase, otherwise get documents from the process phase
            Phase phase = _currentPhase.Phase == Phase.Output ? Phase.Output : Phase.Process;
            ValidatePipeline(pipelineName, phase);
            return _phaseResults[pipelineName][(int)phase].Outputs;
        }

        public IReadOnlyDictionary<string, ImmutableArray<IDocument>> ByPipeline()
        {
            ValidateCurrent();

            return GetOutputsFromDependencies().ToDictionary(x => x.Key, x => x.Value);
        }

        public IEnumerable<IDocument> ExceptPipeline(string pipelineName)
        {
            ValidateCurrent();
            ValidateArguments(pipelineName);

            return GetOutputsFromDependencies()
                .Where(x => !x.Key.Equals(pipelineName, StringComparison.OrdinalIgnoreCase))
                .SelectMany(x => x.Value);
        }

        public IEnumerator<IDocument> GetEnumerator()
        {
            ValidateCurrent();

            return GetOutputsFromDependencies().SelectMany(x => x.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private IEnumerable<KeyValuePair<string, ImmutableArray<IDocument>>> GetOutputsFromDependencies()
        {
            // If we're in the transform phase, get outputs from all process phases including this one
            if (_currentPhase.Phase == Phase.Transform)
            {
                return _phaseResults
                    .Where(x => x.Value[(int)Phase.Process] != null)
                    .Select(x => KeyValuePair.Create(x.Key, x.Value[(int)Phase.Process].Outputs))
                    .ToArray();
            }

            // If we're in the output phase, get outputs from all other non-deployment output phases
            // This will only happen for deployment pipelines (otherwise we would have thrown)
            if (_currentPhase.Phase == Phase.Output)
            {
                return _pipelines
                    .Where(x => !x.Value.Deployment)
                    .Select(x => KeyValuePair.Create(x.Key, _phaseResults[x.Key]))
                    .Where(x => x.Value[(int)Phase.Output] != null)
                    .Select(x => KeyValuePair.Create(x.Key, x.Value[(int)Phase.Output].Outputs))
                    .ToArray();
            }

            // If we're in the process phase, get outputs from the process phase only from dependencies
            if (_cachedDependencyOutputs == null)
            {
                HashSet<string> transientDependencies = GatherProcessPhaseDependencies(_currentPhase);
                _cachedDependencyOutputs = _phaseResults
                    .Where(x => transientDependencies.Contains(x.Key) && x.Value[(int)Phase.Process] != null)
                    .Select(x => KeyValuePair.Create(x.Key, x.Value[(int)Phase.Process].Outputs))
                    .ToArray();
            }
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

        /// <summary>
        /// Validates the pipeline name argument.
        /// </summary>
        private void ValidateArguments(string pipelineName)
        {
            if (string.IsNullOrEmpty(pipelineName))
            {
                throw new ArgumentException(nameof(pipelineName));
            }
            if (!_pipelines.ContainsKey(pipelineName))
            {
                throw new KeyNotFoundException($"The pipeline {pipelineName} could not be found");
            }
        }

        /// <summary>
        /// Validates the requested pipeline.
        /// </summary>
        private void ValidatePipeline(string pipelineName, Phase phase)
        {
            // Make sure the pipeline isn't isolated
            if (_pipelines[pipelineName].Isolated)
            {
                throw new InvalidOperationException($"Cannot access documents in isolated pipeline {pipelineName}");
            }

            // Make sure this pipeline isn't deployment (which it is if we're in the output phase) and we're asking for a non-deployment output phase
            if (_currentPhase.Phase == Phase.Output && _pipelines[pipelineName].Deployment)
            {
                throw new InvalidOperationException($"Cannot access output documents from another deployment pipeline {pipelineName}");
            }

            // Make sure the pipeline has results
            if (!_phaseResults.TryGetValue(pipelineName, out PhaseResult[] phaseResults))
            {
                throw new KeyNotFoundException($"The pipeline results for {pipelineName} could not be found");
            }

            // Make sure the pipeline has results for the requested phase
            if (phaseResults[(int)phase] == null)
            {
                throw new KeyNotFoundException($"{phase} phase outputs for pipeline {pipelineName} could not be found");
            }

            // Make sure we're not accessing our own documents or documents from a non-dependency while in the process phase
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

        /// <summary>
        /// Validates the current pipeline and phase.
        /// </summary>
        private void ValidateCurrent()
        {
            // An isolated pipeline cannot access outputs
            if (_currentPhase.Pipeline.Isolated)
            {
                throw new InvalidOperationException($"Cannot access documents from inside isolated pipeline {_currentPhase.PipelineName}");
            }

            // Outputs cannot be accessed from the input or output phases (unless a deployment pipeline)
            if (_currentPhase.Phase == Phase.Input || (_currentPhase.Phase == Phase.Output && !_currentPhase.Pipeline.Deployment))
            {
                throw new InvalidOperationException($"Cannot access document outputs during the {_currentPhase.Phase} phase");
            }
        }
    }
}
