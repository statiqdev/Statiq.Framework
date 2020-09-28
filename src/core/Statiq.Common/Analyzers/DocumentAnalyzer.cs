using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public abstract class DocumentAnalyzer : Analyzer
    {
        public sealed override async Task AnalyzeAsync(ImmutableArray<IDocument> documents, IAnalyzerContext context) =>
            await documents.ParallelForEachAsync(async doc => await AnalyzeAsync(doc, new DocumentAnalyzerContext(context, doc)), context.CancellationToken);

        protected abstract Task AnalyzeAsync(IDocument document, IAnalyzerContext context);
    }
}
