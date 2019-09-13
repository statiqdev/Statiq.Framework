using System;
using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// Represents the state of the application when it was run.
    /// </summary>
    public class ApplicationState : IReadOnlyApplicationState
    {
        private string[] _arguments = new string[0];

        /// <inheritdoc />
        public string[] Arguments
        {
            get => _arguments;
            set => _arguments = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <inheritdoc />
        public string Input { get; set; }
    }
}
