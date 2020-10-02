using System;

namespace Statiq.Common
{
    /// <summary>
    /// Wraps an exception thrown inside the engine while executing nested modules to prevent repeating the log message.
    /// </summary>
    internal class LoggedException : Exception
    {
        public LoggedException(Exception innerException)
            : base(null, innerException)
        {
        }
    }
}
