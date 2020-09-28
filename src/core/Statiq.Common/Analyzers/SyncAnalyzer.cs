using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public abstract class SyncAnalyzer : Analyzer
    {
        public sealed override Task AnalyzeAsync(ImmutableArray<IDocument> documents, IAnalyzerContext context)
        {
            Analyze(documents, context);
            return Task.CompletedTask;
        }

        protected abstract void Analyze(ImmutableArray<IDocument> documents, IAnalyzerContext context);
    }
}
