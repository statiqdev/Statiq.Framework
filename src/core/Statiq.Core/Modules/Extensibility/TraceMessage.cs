using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Outputs trace messages during execution.
    /// </summary>
    /// <remarks>
    /// This module has no effect on documents and the input documents are passed through to output documents.
    /// </remarks>
    /// <category>Extensibility</category>
    public class TraceMessage : ConfigModule<string>
    {
        private TraceEventType _traceEventType = TraceEventType.Information;

        /// <summary>
        /// Outputs the string value of the returned object to trace. This allows
        /// you to trace different content for each document depending on the input document.
        /// </summary>
        /// <param name="content">A delegate that returns the content to trace.</param>
        public TraceMessage(Config<string> content)
            : base(content, false)
        {
        }

        /// <summary>
        /// Sets the event type to trace.
        /// </summary>
        /// <param name="traceEventType">The event type to trace.</param>
        /// <returns>The current module instance.</returns>
        public TraceMessage EventType(TraceEventType traceEventType)
        {
            _traceEventType = traceEventType;
            return this;
        }

        protected override Task<IEnumerable<IDocument>> ExecuteAsync(
            IDocument input,
            IExecutionContext context,
            string value)
        {
            Common.Trace.TraceEvent(_traceEventType, value);
            return Task.FromResult(input == null ? (IEnumerable<IDocument>)context.Inputs : input.Yield());
        }
    }
}
