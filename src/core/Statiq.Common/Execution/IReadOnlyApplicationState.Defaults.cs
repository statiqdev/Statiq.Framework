using System;
using System.Collections.Generic;

namespace Statiq.Common
{
    public partial interface IReadOnlyApplicationState
    {
        /// <summary>
        /// Determines if the application was run with the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns><c>true</c> if the application was run with the specified command, <c>false</c> otherwise.</returns>
        public bool IsCommand(string command) => CommandName.Equals(command, StringComparison.OrdinalIgnoreCase);
    }
}
