using System;
using Statiq.Common;

namespace Statiq.Core
{
    public class BeforePipelinePhaseExecution
    {
        internal BeforePipelinePhaseExecution(Guid executionId, string pipelineName, Phase phase)
        {
            ExecutionId = executionId;
            PipelineName = pipelineName;
            Phase = phase;
        }

        public Guid ExecutionId { get; }

        public string PipelineName { get; }

        public Phase Phase { get; }
    }
}
