using System.Threading.Tasks;
using Statiq.Common;

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
