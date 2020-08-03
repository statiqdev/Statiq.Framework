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
    public class BeforeModuleExecution
    {
        internal BeforeModuleExecution(IExecutionContext context)
        {
            Context = context;
        }

        public IExecutionContext Context { get; }

        internal IEnumerable<IDocument> OverriddenOutputs { get; private set; }

        public void OverrideOutputs(IEnumerable<IDocument> outputs)
        {
            outputs.ThrowIfNull(nameof(outputs));
            if (OverriddenOutputs is object)
            {
                throw new InvalidOperationException("Only one event may override module results.");
            }
            OverriddenOutputs = outputs;
        }
    }
}
