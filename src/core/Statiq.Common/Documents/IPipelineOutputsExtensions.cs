using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Statiq.Common
{
    public static class IPipelineOutputsExtensions
    {
        /// <summary>
        /// Gets and concatenates all documents from multiple pipelines.
        /// Note that if a document exists in more than one pipeline it
        /// may appear multiple times in the result.
        /// </summary>
        /// <param name="pipelineOutputs">The pipeline outputs.</param>
        /// <param name="pipelines">The pipeline(s) to get documents from.</param>
        /// <returns>All documents from all specified pipeline(s).</returns>
        public static DocumentList<IDocument> FromPipelines(
            this IPipelineOutputs pipelineOutputs,
            params string[] pipelines) =>
            (pipelines ?? Array.Empty<string>()).SelectMany(x => pipelineOutputs.FromPipeline(x)).ToDocumentList();
    }
}
