using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JavaScriptEngineSwitcher.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Statiq.Common;

namespace Statiq.Core
{
    public class AfterEngineExecution
    {
        internal AfterEngineExecution(IEngine engine, Guid executionId, IPipelineOutputs outputs, long elapsedMilliseconds)
        {
            Engine = engine;
            ExecutionId = executionId;
            Outputs = outputs;
            ElapsedMilliseconds = elapsedMilliseconds;
        }

        public IEngine Engine { get; }

        public Guid ExecutionId { get; }

        public IPipelineOutputs Outputs { get; }

        public long ElapsedMilliseconds { get; }
    }
}
