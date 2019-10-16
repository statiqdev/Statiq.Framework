using System;
using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// Represents the state of the application when it was run.
    /// </summary>
    public class ApplicationState : IReadOnlyApplicationState
    {
        public ApplicationState(string[] arguments, string input)
        {
            Arguments = arguments ?? new string[] { };
            Input = input;
        }

        /// <inheritdoc />
        public string[] Arguments { get; }

        /// <inheritdoc />
        public string Input { get; }
    }
}
