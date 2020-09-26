using System;
using System.Collections.Immutable;
using Statiq.Common;

namespace Statiq.Core
{
    public class AfterPipelinePhaseExecution
    {
        internal AfterPipelinePhaseExecution(Guid executionId, string pipelineName, Phase phase, ImmutableArray<IDocument> outputs, long elapsedMilliseconds)
        {
            ExecutionId = executionId;
            PipelineName = pipelineName;
            Phase = phase;
            Outputs = outputs;
            ElapsedMilliseconds = elapsedMilliseconds;
        }

        public Guid ExecutionId { get; }

        public string PipelineName { get; }

        public Phase Phase { get; }

        public ImmutableArray<IDocument> Outputs { get; }

        public long ElapsedMilliseconds { get; }
    }
}
