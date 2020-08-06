using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
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
        private readonly Lazy<KeyValuePair<string, ImmutableArray<IDocument>>[]> _cachedDependencyOutputs;

        public PhaseOutputs(IReadOnlyDictionary<string, PhaseResult[]> phaseResults, PipelinePhase currentPhase, IPipelineCollection pipelines)
        {
            _phaseResults = phaseResults.ThrowIfNull(nameof(phaseResults));
            _currentPhase = currentPhase.ThrowIfNull(nameof(currentPhase));
            _pipelines = pipelines.ThrowIfNull(nameof(pipelines));

            _cachedDependencyOutputs = new Lazy<KeyValuePair<string, ImmutableArray<IDocument>>[]>(
                () => GetOutputsFromDependencies(), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public DocumentList<IDocument> FromPipeline(string pipelineName)
        {
            ValidateCurrent();
            ValidateArguments(pipelineName);
            ValidatePipeline(pipelineName);

            // If we're in the output phase (which will only happen if this is a deployment module because the checks
            // will throw otherwise) get documents from the output phase, otherwise get documents from the process phase
            return GetOutputs(_phaseResults[pipelineName], _currentPhase.Phase == Phase.Output ? Phase.Output : Phase.Process).ToDocumentList();
        }

        public IReadOnlyDictionary<string, DocumentList<IDocument>> ByPipeline()
        {
            ValidateCurrent();

            return _cachedDependencyOutputs.Value.ToDictionary(x => x.Key, x => x.Value.ToDocumentList());
        }

        public DocumentList<IDocument> ExceptPipeline(string pipelineName)
        {
            ValidateCurrent();
            ValidateArguments(pipelineName);

            return _cachedDependencyOutputs.Value
                .Where(x => !x.Key.Equals(pipelineName, StringComparison.OrdinalIgnoreCase))
                .SelectMany(x => x.Value)
                .ToDocumentList();
        }

        public IEnumerator<IDocument> GetEnumerator()
        {
            ValidateCurrent();

            return _cachedDependencyOutputs.Value.SelectMany(x => x.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private KeyValuePair<string, ImmutableArray<IDocument>>[] GetOutputsFromDependencies()
        {
            // If we're in the transform phase, get outputs from all process phases including this one
            if (_currentPhase.Phase == Phase.PostProcess)
            {
                return _phaseResults
                    .Select(x => KeyValuePair.Create(x.Key, GetOutputs(x.Value, Phase.Process)))
                    .ToArray();
            }

            // If we're in the output phase, get outputs from all other non-deployment output phases
            // This will only happen for deployment pipelines (otherwise we would have thrown)
            if (_currentPhase.Phase == Phase.Output)
            {
                return _pipelines
                    .AsEnumerable()
                    .Where(x => !x.Value.Deployment)
                    .Select(x => KeyValuePair.Create(x.Key, GetOutputs(_phaseResults[x.Key], Phase.Output)))
                    .ToArray();
            }

            // If we're in the process phase, get outputs from the process phase only from dependencies
            HashSet<string> transientDependencies = GatherProcessPhaseDependencies(_currentPhase);
            return _phaseResults
                .Where(x => transientDependencies.Contains(x.Key))
                .Select(x => KeyValuePair.Create(x.Key, GetOutputs(x.Value, Phase.Process)))
                .ToArray();
        }

        // Crawl up the phases looking for one with outputs if the one specified below doesn't have any
        private static ImmutableArray<IDocument> GetOutputs(PhaseResult[] phaseResults, Phase phase)
        {
            int p = (int)phase;
            while (p >= 0 && phaseResults[p] is null)
            {
                p--;
            }
            return p == -1 ? ImmutableArray<IDocument>.Empty : phaseResults[p].Outputs;
        }

        private HashSet<string> GatherProcessPhaseDependencies(PipelinePhase phase, HashSet<string> transientDependencies = null)
        {
            if (transientDependencies is null)
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
        private void ValidatePipeline(string pipelineName)
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
            if (!_phaseResults.ContainsKey(pipelineName))
            {
                throw new KeyNotFoundException($"The pipeline results for {pipelineName} could not be found");
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
