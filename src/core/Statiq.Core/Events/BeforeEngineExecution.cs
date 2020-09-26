using System;
using Statiq.Common;

namespace Statiq.Core
{
    public class BeforeEngineExecution
    {
        internal BeforeEngineExecution(IEngine engine, Guid executionId)
        {
            Engine = engine;
            ExecutionId = executionId;
        }

        public IEngine Engine { get; }

        public Guid ExecutionId { get; }
    }
}
