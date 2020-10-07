using System;
using System.Collections.Immutable;
using System.Linq;
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

        // Unused, prevent overriding in derived classes
        protected sealed override Task AnalyzeDocumentAsync(IDocument document, IAnalyzerContext context) =>
            throw new NotSupportedException();

        protected virtual void Analyze(ImmutableArray<IDocument> documents, IAnalyzerContext context) =>
            documents.AsParallel().ForAll(doc => AnalyzeDocument(doc, new DocumentAnalyzerContext(context, doc)));

        /// <summary>
        /// Analyzes an individual document.
        /// </summary>
        /// <remarks>
        /// This method will be called for each document unless <see cref="Analyze(ImmutableArray{IDocument}, IAnalyzerContext)"/> is overridden.
        /// </remarks>
        /// <param name="document">The document to analyze.</param>
        /// <param name="context">An analysis context that contains the documents to analyze as well as other state information.</param>
        protected virtual void AnalyzeDocument(IDocument document, IAnalyzerContext context)
        {
        }
    }
}
