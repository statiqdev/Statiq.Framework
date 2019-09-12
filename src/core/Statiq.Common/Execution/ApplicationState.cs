using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// Represents the state of the application when it was run.
    /// </summary>
    public class ApplicationState : IReadOnlyApplicationState
    {
        /// <inheritdoc />
        public string[] Arguments { get; set; }

        /// <inheritdoc />
        public string Input { get; set; }
    }
}
