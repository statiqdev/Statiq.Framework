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
    /// Provides outputs from the most recently executed phase of each pipeline.
    /// </summary>
    internal class PipelineOutputs : IPipelineOutputs
    {
        private readonly IReadOnlyDictionary<string, PhaseResult[]> _phaseResults;

        public PipelineOutputs(IReadOnlyDictionary<string, PhaseResult[]> phaseResults)
        {
            _phaseResults = phaseResults.ThrowIfNull(nameof(phaseResults));
        }

        public IReadOnlyDictionary<string, DocumentList<IDocument>> ByPipeline() =>
            _phaseResults.ToDictionary(x => x.Key, x => x.Value.Last(x => x != null).Outputs.ToDocumentList());

        public DocumentList<IDocument> ExceptPipeline(string pipelineName)
        {
            pipelineName.ThrowIfNull(nameof(pipelineName));
            return _phaseResults
                .Where(x => !x.Key.Equals(pipelineName, StringComparison.OrdinalIgnoreCase))
                .SelectMany(x => x.Value.Last(x => x != null).Outputs)
                .ToDocumentList();
        }

        public DocumentList<IDocument> FromPipeline(string pipelineName)
        {
            pipelineName.ThrowIfNull(nameof(pipelineName));
            return _phaseResults.TryGetValue(pipelineName, out PhaseResult[] results)
                ? results.Last(x => x != null).Outputs.ToDocumentList()
                : DocumentList<IDocument>.Empty;
        }

        public IEnumerator<IDocument> GetEnumerator() =>
            _phaseResults.SelectMany(x => x.Value.Last(x => x != null).Outputs).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
