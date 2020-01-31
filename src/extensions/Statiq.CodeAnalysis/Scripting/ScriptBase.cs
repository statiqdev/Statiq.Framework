using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.CodeAnalysis.Scripting
{
    public abstract class ScriptBase
    {
        protected ScriptBase(IMetadata metadata, IExecutionState executionState)
        {
            Metadata = metadata;
            ExecutionState = executionState;
        }

        public IExecutionState ExecutionState { get; }

        public IMetadata Metadata { get; }

        public abstract Task<object> EvaluateAsync();
    }
}