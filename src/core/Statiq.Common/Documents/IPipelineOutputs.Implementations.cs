using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Statiq.Common
{
    public partial interface IPipelineOutputs
    {
        /// <summary>
        /// Gets and concatenates all documents from multiple pipelines.
        /// Note that if a document exists in more than one pipeline it
        /// may appear multiple times in the result.
        /// </summary>
        /// <param name="pipelines">The pipeline(s) to get documents from.</param>
        /// <returns>All documents from all specified pipeline(s).</returns>
        public IEnumerable<IDocument> FromPipelines(
            params string[] pipelines) =>
            (pipelines ?? Array.Empty<string>()).SelectMany(x => FromPipeline(x));

        /// <summary>
        /// Gets documents from a specific pipeline.
        /// </summary>
        /// <value>
        /// The documents output by the specified pipeline..
        /// </value>
        /// <param name="pipline">The pipeline.</param>
        /// <returns>The documents output by the specified pipeline.</returns>
        public ImmutableArray<IDocument> this[string pipline] => FromPipeline(pipline);
    }
}
