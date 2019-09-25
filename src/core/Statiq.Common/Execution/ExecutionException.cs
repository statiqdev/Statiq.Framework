using System;

namespace Statiq.Common
{
    /// <summary>
    /// The exception that is thrown when there is an error during execution.
    /// </summary>
    public class ExecutionException : Exception
    {
        public ExecutionException(string message)
            : base(message)
        {
        }
    }
}
