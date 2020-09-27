using System.Linq;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public abstract class SyncDocumentAnalyzer : DocumentAnalyzer
    {
        protected sealed override Task AnalyzeAsync(IDocument document, IAnalyzerContext context)
        {
            Analyze(document, context);
            return Task.CompletedTask;
        }

        protected abstract void Analyze(IDocument document, IAnalyzerContext context);
    }
}
