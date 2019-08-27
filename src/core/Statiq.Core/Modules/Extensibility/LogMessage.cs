using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Outputs log messages during execution.
    /// </summary>
    /// <remarks>
    /// This module has no effect on documents and the input documents are passed through to output documents.
    /// </remarks>
    /// <category>Extensibility</category>
    public class LogMessage : SyncConfigModule<string>
    {
        private LogLevel _logLevel = LogLevel.Information;

        /// <summary>
        /// Outputs the string value of the returned object to trace. This allows
        /// you to trace different content for each document depending on the input document.
        /// </summary>
        /// <param name="content">A delegate that returns the content to trace.</param>
        public LogMessage(Config<string> content)
            : base(content, false)
        {
        }

        /// <summary>
        /// Outputs the string value of the returned object to trace. This allows
        /// you to trace different content for each document depending on the input document.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="content">A delegate that returns the content to trace.</param>
        public LogMessage(LogLevel logLevel, Config<string> content)
            : base(content, false)
        {
            _logLevel = logLevel;
        }

        protected override IEnumerable<IDocument> Execute(IDocument input, IExecutionContext context, string value)
        {
            context.Logger.Log(_logLevel, value);
            return input == null ? context.Inputs : input.Yield();
        }
    }
}
