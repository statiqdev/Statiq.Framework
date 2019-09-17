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
    public class AfterModuleExecution : EventArgs
    {
        internal AfterModuleExecution(IExecutionContext context, ImmutableArray<IDocument> outputs)
        {
            Context = context;
            Outputs = outputs;
        }

        public IExecutionContext Context { get; }

        public ImmutableArray<IDocument> Outputs { get; }

        internal IEnumerable<IDocument> OverriddenOutputs { get; private set; }

        public void OverrideOutputs(IEnumerable<IDocument> outputs)
        {
            _ = outputs ?? throw new ArgumentNullException(nameof(outputs));
            if (OverriddenOutputs != null)
            {
                throw new InvalidOperationException("Only one event may override module results.");
            }
            OverriddenOutputs = outputs;
        }
    }
}
