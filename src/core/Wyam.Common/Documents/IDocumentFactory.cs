using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Wyam.Common.Execution;
using Wyam.Common.IO;

namespace Wyam.Common.Documents
{
    /// <summary>
    /// Responsible for creating new document instances.
    /// </summary>
    public interface IDocumentFactory
    {
        /// <summary>
        /// Clones the specified source document with a new source, new content, and additional metadata (all existing metadata is retained)
        /// or gets a new document if the source document is null or <c>AsNewDocuments()</c> was called on the module.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="sourceDocument">The source document.</param>
        /// <param name="source">The source (if the source document contains a source, then this is ignored and the source document's source is used instead).</param>
        /// <param name="metadata">The metadata items.</param>
        /// <param name="content">
        /// The content, which should be dynamically infered based on type. Implementations should support creating content from
        /// <see cref="Stream"/>, <see cref="string"/>, or <see cref="IFile"/>.
        /// They should also support directly supplying an <see cref="IDocument"/> (in which case the existing content should be passed to the new document).
        /// Any other content types should result in using the <see cref="object.ToString()"/> value as document content.
        /// </param>
        /// <returns>The cloned or new document.</returns>
        Task<IDocument> GetDocumentAsync(
            IExecutionContext context,
            IDocument sourceDocument,
            FilePath source,
            IEnumerable<KeyValuePair<string, object>> metadata,
            object content);
    }
}
