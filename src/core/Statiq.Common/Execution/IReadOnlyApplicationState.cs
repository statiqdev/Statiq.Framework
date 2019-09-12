using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// Represents the state of the application when it was run.
    /// </summary>
    public interface IReadOnlyApplicationState
    {
        /// <summary>
        /// Gets the raw arguments passed to the application (the first argument is typically the "command").
        /// </summary>
        public string[] Arguments { get; }

        /// <summary>
        /// Gets any input that was passed to the application (for example, on stdin via piping).
        /// </summary>
        public string Input { get; }
    }
}
