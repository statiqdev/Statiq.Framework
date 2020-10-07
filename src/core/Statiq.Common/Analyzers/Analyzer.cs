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
        public virtual async Task AnalyzeAsync(ImmutableArray<IDocument> documents, IAnalyzerContext context) =>
            await documents.ParallelForEachAsync(async doc => await AnalyzeDocumentAsync(doc, new DocumentAnalyzerContext(context, doc)), context.CancellationToken);

        /// <summary>
        /// Analyzes an individual document.
        /// </summary>
        /// <remarks>
        /// This method will be called for each document unless <see cref="AnalyzeAsync(ImmutableArray{IDocument}, IAnalyzerContext)"/> is overridden.
        /// </remarks>
        /// <param name="document">The document to analyze.</param>
        /// <param name="context">An analysis context that contains the documents to analyze as well as other state information.</param>
        protected virtual Task AnalyzeDocumentAsync(IDocument document, IAnalyzerContext context) => Task.CompletedTask;
    }
}
