using System.Threading.Tasks;

namespace Statiq.Common
{
    public abstract class Analyzer : IAnalyzer
    {
        /// <inheritdoc/>
        public virtual string[] Pipelines { get; set; }

        /// <inheritdoc/>
        public virtual Phase[] Phases { get; set; }

        /// <inheritdoc/>
        public abstract Task AnalyzeAsync(IAnalyzerContext context);
    }
}
