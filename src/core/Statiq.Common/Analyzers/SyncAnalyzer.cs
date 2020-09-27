using System.Threading.Tasks;

namespace Statiq.Common
{
    public abstract class SyncAnalyzer : Analyzer
    {
        public sealed override Task AnalyzeAsync(IAnalyzerContext context)
        {
            Analyze(context);
            return Task.CompletedTask;
        }

        protected abstract void Analyze(IAnalyzerContext context);
    }
}
