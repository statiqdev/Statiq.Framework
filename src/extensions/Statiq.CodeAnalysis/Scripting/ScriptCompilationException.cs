using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.Meta;

namespace Statiq.CodeAnalysis.Scripting
{
    public class ScriptCompilationException : Exception
    {
        public IReadOnlyList<string> ErrorMessages { get; }

        public ScriptCompilationException(List<string> errorMessages)
        {
            ErrorMessages = errorMessages.AsReadOnly();
        }
    }
}
