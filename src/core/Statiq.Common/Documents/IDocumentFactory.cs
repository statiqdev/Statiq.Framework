using System.Collections.Generic;
using System.IO;
using Statiq.Common.Content;
using Statiq.Common.Execution;
using Statiq.Common.IO;

namespace Statiq.Common.Documents
{
    /// <summary>
    /// Responsible for creating new default document instances.
    /// </summary>
    public interface IDocumentFactory
    {
        /// <summary>
        /// Gets a new default document.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="source">The source (if the source document contains a source, then this is ignored and the source document's source is used instead).</param>
        /// <param name="destination">The destination.</param>
        /// <param name="items">The metadata items.</param>
        /// <param name="contentProvider">The content provider.</param>
        /// <returns>The cloned or new document.</returns>
        IDocument GetDocument(
            IExecutionContext context,
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider);
    }
}
