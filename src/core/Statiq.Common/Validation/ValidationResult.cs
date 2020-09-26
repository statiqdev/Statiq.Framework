using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    /// <summary>
    /// Represents a result from validation.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// The level of the validation result.
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// The validation message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The document to which this validation applies (or null if it does not apply to a single document).
        /// </summary>
        public IDocument Document { get; set; }
    }
}
