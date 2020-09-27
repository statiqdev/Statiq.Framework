using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    /// <summary>
    /// Represents a result from an analyzer.
    /// </summary>
    public class AnalyzerResult
    {
        /// <summary>
        /// The level of the analyzer result.
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// The message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The document to which this analysis applies (or null if it does not apply to a single document).
        /// </summary>
        public IDocument Document { get; set; }
    }
}
