using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    public abstract class Analyzer : IAnalyzer
    {
        /// <inheritdoc/>
        public virtual LogLevel LogLevel { get; set; } = LogLevel.Information;

        /// <inheritdoc/>
        public virtual string[] Pipelines { get; }

        /// <inheritdoc/>
        public virtual Phase[] Phases { get; } = new Phase[] { Phase.Process };

        /// <inheritdoc/>
        public abstract Task AnalyzeAsync(ImmutableArray<IDocument> documents, IAnalyzerContext context);
    }
}
