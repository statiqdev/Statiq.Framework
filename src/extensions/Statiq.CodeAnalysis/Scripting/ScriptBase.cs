using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.Meta;

namespace Statiq.CodeAnalysis.Scripting
{
    public abstract class ScriptBase
    {
        protected ScriptBase(IDocument document, IExecutionContext context)
        {
            Document = document;
            Context = context;
        }

        public IDocument Document { get; }

        public IExecutionContext Context { get; }

        public abstract Task<object> EvaluateAsync();
    }
}
