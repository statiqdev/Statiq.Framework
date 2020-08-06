using System.Collections.Generic;
using System.Collections.Immutable;

namespace Statiq.Common
{
    /// <summary>
    /// Contains a collection of documents output by pipelines.
    /// </summary>
    public interface IPipelineOutputs : IEnumerable<IDocument>, IDocumentTree<IDocument>
    {
        /// <summary>
        /// Gets documents by pipeline.
        /// </summary>
        /// <returns>All documents output by each pipeline.</returns>
        IReadOnlyDictionary<string, DocumentList<IDocument>> ByPipeline();

        /// <summary>
        /// Gets documents from a specific pipeline.
        /// </summary>
        /// <param name="pipelineName">The pipeline.</param>
        /// <returns>The documents output by the specified pipeline.</returns>
        DocumentList<IDocument> FromPipeline(string pipelineName);

        /// <summary>
        /// Gets all documents output by every pipeline except those from the specified pipeline.
        /// </summary>
        /// <param name="pipelineName">The pipeline.</param>
        /// <returns>All documents output by every pipeline except the specified one.</returns>
        DocumentList<IDocument> ExceptPipeline(string pipelineName);

        /// <summary>
        /// Returns documents with destination paths from all pipelines that satisfy the globbing pattern(s).
        /// </summary>
        /// <param name="destinationPatterns">The globbing pattern(s) to filter by (can be a single path).</param>
        /// <returns>The documents that satisfy the pattern or <c>null</c>.</returns>
        public IEnumerable<IDocument> this[params string[] destinationPatterns] => this.FilterDestinations(destinationPatterns);

        DocumentList<IDocument> IDocumentTree<IDocument>.GetAncestorsOf(IDocument document, bool includeSelf) => this.AsDestinationTree().GetAncestorsOf(document, includeSelf);

        DocumentList<IDocument> IDocumentTree<IDocument>.GetChildrenOf(IDocument document) => this.AsDestinationTree().GetChildrenOf(document);

        DocumentList<IDocument> IDocumentTree<IDocument>.GetDescendantsOf(IDocument document, bool includeSelf) => this.AsDestinationTree().GetDescendantsOf(document, includeSelf);

        IDocument IDocumentTree<IDocument>.GetParentOf(IDocument document) => this.AsDestinationTree().GetParentOf(document);

        DocumentList<IDocument> IDocumentTree<IDocument>.GetSiblingsOf(IDocument document, bool includeSelf) => this.AsDestinationTree().GetSiblingsOf(document, includeSelf);
    }
}
