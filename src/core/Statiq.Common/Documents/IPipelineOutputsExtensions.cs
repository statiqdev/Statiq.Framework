using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;

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
            params string[] pipelines)
        {
            pipelineOutputs.ThrowIfNull(nameof(pipelineOutputs));
            return (pipelines ?? Array.Empty<string>()).SelectMany(x => pipelineOutputs.FromPipeline(x)).ToDocumentList();
        }

        public static IDocument Get(this IPipelineOutputs pipelineOutputs, NormalizedPath destinationPath)
        {
            pipelineOutputs.ThrowIfNull(nameof(pipelineOutputs));
            return pipelineOutputs.FirstOrDefault(x => x.Destination.Equals(destinationPath));
        }

        public static DocumentList<IDocument> GetAncestorsOf(this IPipelineOutputs pipelineOutputs, in NormalizedPath destinationPath, bool includeSelf)
        {
            pipelineOutputs.ThrowIfNull(nameof(pipelineOutputs));
            IDocument document = pipelineOutputs.Get(destinationPath);
            if (document is null)
            {
                return DocumentList<IDocument>.Empty;
            }
            return pipelineOutputs.GetAncestorsOf(document, includeSelf);
        }

        public static DocumentList<IDocument> GetAncestorsOf(this IPipelineOutputs pipelineOutputs, in NormalizedPath destinationPath) =>
            pipelineOutputs.GetAncestorsOf(destinationPath, false);

        public static DocumentList<IDocument> GetChildrenOf(this IPipelineOutputs pipelineOutputs, in NormalizedPath destinationPath)
        {
            pipelineOutputs.ThrowIfNull(nameof(pipelineOutputs));
            IDocument document = pipelineOutputs.Get(destinationPath);
            if (document is null)
            {
                return DocumentList<IDocument>.Empty;
            }
            return pipelineOutputs.GetChildrenOf(document);
        }

        public static DocumentList<IDocument> GetDescendantsOf(this IPipelineOutputs pipelineOutputs, in NormalizedPath destinationPath, bool includeSelf)
        {
            pipelineOutputs.ThrowIfNull(nameof(pipelineOutputs));
            IDocument document = pipelineOutputs.Get(destinationPath);
            if (document is null)
            {
                return DocumentList<IDocument>.Empty;
            }
            return pipelineOutputs.GetDescendantsOf(document, includeSelf);
        }

        public static DocumentList<IDocument> GetDescendantsOf(this IPipelineOutputs pipelineOutputs, in NormalizedPath destinationPath) =>
            pipelineOutputs.GetDescendantsOf(destinationPath, false);

        public static IDocument GetParentOf(this IPipelineOutputs pipelineOutputs, in NormalizedPath destinationPath)
        {
            pipelineOutputs.ThrowIfNull(nameof(pipelineOutputs));
            IDocument document = pipelineOutputs.Get(destinationPath);
            if (document is null)
            {
                return default;
            }
            return pipelineOutputs.GetParentOf(document);
        }

        public static DocumentList<IDocument> GetSiblingsOf(this IPipelineOutputs pipelineOutputs, in NormalizedPath destinationPath, bool includeSelf)
        {
            pipelineOutputs.ThrowIfNull(nameof(pipelineOutputs));
            IDocument document = pipelineOutputs.Get(destinationPath);
            if (document is null)
            {
                return DocumentList<IDocument>.Empty;
            }
            return pipelineOutputs.GetSiblingsOf(document, includeSelf);
        }

        public static DocumentList<IDocument> GetSiblingsOf(this IPipelineOutputs pipelineOutputs, in NormalizedPath destinationPath) =>
            pipelineOutputs.GetSiblingsOf(destinationPath, false);
    }
}
