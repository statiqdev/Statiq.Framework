using System;
using System.Collections.Generic;
using System.Linq;

namespace Statiq.Common
{
    public static class IPipelineResultsExtensions
    {
        /// <summary>
        /// Gets and concatenates all documents from multiple pipelines.
        /// Note that if a document exists in more than one pipeline it
        /// may appear multiple times in the result.
        /// </summary>
        /// <param name="collection">The document collection.</param>
        /// <param name="pipelines">The pipeline(s) to get documents from.</param>
        /// <returns>All documents from all specified pipeline(s).</returns>
        public static IEnumerable<IDocument> FromPipelines(
            this IPipelineResults collection,
            params string[] pipelines) =>
            (pipelines ?? Array.Empty<string>()).SelectMany(x => collection.FromPipeline(x));
    }
}
