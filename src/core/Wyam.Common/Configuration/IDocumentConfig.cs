using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Common.Configuration
{
    /// <summary>
    /// Used to support the implicit casting operator.
    /// </summary>
    internal interface IDocumentConfig
    {
        Type ValueType { get; }
        Func<IDocument, IExecutionContext, Task<object>> Delegate { get; }
    }
}
