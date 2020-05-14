using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JavaScriptEngineSwitcher.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Wraps an exception thrown inside
    /// <see cref="Engine.ExecuteModulesAsync(ExecutionContextData, IExecutionContext, IEnumerable{IModule}, ImmutableArray{IDocument}, ILogger)"/>
    /// to prevent repeating the log message.
    /// </summary>
    internal class ExecuteModulesException : Exception
    {
        public ExecuteModulesException(Exception innerException)
            : base(null, innerException)
        {
        }
    }
}
