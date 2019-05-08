using System;
using System.Collections.Generic;

namespace Wyam.Razor
{
    public class CompilationErrorsException : AggregateException
    {
        public CompilationErrorsException(IEnumerable<CompilationErrorException> errors)
            : base("Razor compilation failed, see the inner exceptions for details", errors)
        {
        }
    }
}