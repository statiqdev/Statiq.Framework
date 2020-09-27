using System.Threading.Tasks;

namespace Statiq.Common
{
    public abstract class DocumentAnalyzer : Analyzer
    {
        public sealed override async Task AnalyzeAsync(IAnalyzerContext context) =>
            await context.Documents.ParallelForEachAsync(async doc => await AnalyzeAsync(doc, context), context.CancellationToken);

        protected abstract Task AnalyzeAsync(IDocument document, IAnalyzerContext context);
    }
}
