using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    public abstract class ScriptBase
    {
        private readonly IExecutionContext _executionContext;

        protected ScriptBase(IMetadata metadata, IExecutionState executionState, IExecutionContext executionContext)
        {
            Metadata = metadata;
            ExecutionState = executionState;
            _executionContext = executionContext;
        }

        public IExecutionState ExecutionState { get; }

        public IMetadata Metadata { get; }

        public abstract Task<object> EvaluateAsync();

        // Manually implement IExecutionContext pass-throughs since we don't
        // want to automatically proxy everything in IExecutionContext

        public string PipelineName => _executionContext?.PipelineName;

        public IReadOnlyPipeline Pipeline => _executionContext?.Pipeline;

        public Phase Phase => _executionContext?.Phase ?? default;

        public IExecutionContext Parent => _executionContext?.Parent;

        public IModule Module => _executionContext?.Module;

        public ImmutableArray<IDocument> Inputs => _executionContext?.Inputs ?? default;
    }
}