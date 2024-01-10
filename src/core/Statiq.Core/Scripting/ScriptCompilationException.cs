using System;
using System.Collections.Generic;
using Polly;

namespace Statiq.Core
{
    public class ScriptCompilationException : Exception
    {
        public IReadOnlyList<string> ErrorMessages { get; }

        public ScriptCompilationException(List<string> errorMessages)
        {
            ArgumentNullException.ThrowIfNull(errorMessages);

            ErrorMessages = errorMessages.AsReadOnly();
        }
    }
}