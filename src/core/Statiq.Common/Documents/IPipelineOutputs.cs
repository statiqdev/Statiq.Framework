using System.Collections.Generic;
using System.Collections.Immutable;

namespace Statiq.Common
{
    /// <summary>
    /// Contains a collection of documents output by pipelines.
    /// </summary>
    public interface IPipelineOutputs : IEnumerable<IDocument>
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
        /// Gets documents from a specific pipeline.
        /// </summary>
        /// <value>
        /// The documents output by the specified pipeline..
        /// </value>
        /// <param name="pipline">The pipeline.</param>
        /// <returns>The documents output by the specified pipeline.</returns>
        public DocumentList<IDocument> this[string pipline] => FromPipeline(pipline);
    }
}
