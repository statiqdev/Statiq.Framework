using System.Collections.Generic;
using System.Collections.Immutable;

namespace Statiq.Common
{
    /// <summary>
    /// Contains a collection of documents output by the process
    /// phase of each pipeline (except isolated ones).
    /// </summary>
    public interface IPipelineResults : IEnumerable<IDocument>
    {
        /// <summary>
        /// Gets documents by pipeline.
        /// </summary>
        /// <returns>All documents output by each pipeline.</returns>
        IReadOnlyDictionary<string, ImmutableArray<IDocument>> ByPipeline();

        /// <summary>
        /// Gets documents from a specific pipeline.
        /// </summary>
        /// <param name="pipeline">The pipeline.</param>
        /// <returns>The documents output by the specified pipeline.</returns>
        ImmutableArray<IDocument> FromPipeline(string pipeline);

        /// <summary>
        /// Gets all documents output by every pipeline except those from the specified pipeline.
        /// </summary>
        /// <param name="pipeline">The pipeline.</param>
        /// <returns>All documents output by every pipeline except the specified one.</returns>
        IEnumerable<IDocument> ExceptPipeline(string pipeline);

        /// <summary>
        /// Gets documents from a specific pipeline.
        /// </summary>
        /// <value>
        /// The documents output by the specified pipeline..
        /// </value>
        /// <param name="pipline">The pipeline.</param>
        /// <returns>The documents output by the specified pipeline.</returns>
        ImmutableArray<IDocument> this[string pipline] { get; }
    }
}
