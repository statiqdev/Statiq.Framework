using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Statiq.Common;

namespace Statiq.Core
{
    internal class ExecutionContextData
    {
        public ExecutionContextData(
            PipelinePhase pipelinePhase,
            Engine engine,
            Guid executionId,
            IReadOnlyDictionary<string, PhaseResult[]> phaseResults,
            IServiceProvider services,
            CancellationToken cancellationToken)
        {
            Engine = engine ?? throw new ArgumentNullException(nameof(engine));
            ExecutionId = executionId;
            PipelinePhase = pipelinePhase ?? throw new ArgumentNullException(nameof(pipelinePhase));
            Services = services ?? throw new ArgumentNullException(nameof(services));
            Outputs = new ProcessPhaseOutputs(phaseResults, pipelinePhase, engine.Pipelines);
            CancellationToken = cancellationToken;
        }

        public PipelinePhase PipelinePhase { get; }
        public Engine Engine { get; }
        public Guid ExecutionId { get; }
        public IServiceProvider Services { get; }
        public CancellationToken CancellationToken { get; }
        public IPipelineOutputs Outputs { get; }
    }
}
