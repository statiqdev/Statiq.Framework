using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wyam.Common.Execution;

namespace Wyam.Common.Documents
{
    public static class IDocumentFactoryExtensions
    {
        /// <summary>
        /// Gets a new document with default initial metadata.
        /// </summary>
        /// <param name="documentFactory">The document factory.</param>
        /// <param name="context">The current execution context.</param>
        /// <returns>The new document.</returns>
        public static IDocument GetDocument(this IDocumentFactory documentFactory, IExecutionContext context) =>
            documentFactory.GetDocument(context, null, null, null, null);

        /// <summary>
        /// Clones the specified source document with identical content and additional metadata (all existing metadata is retained)
        /// or gets a new document if the source document is null or <c>AsNewDocuments()</c> was called on the module.
        /// </summary>
        /// <param name="documentFactory">The document factory.</param>
        /// <param name="context">The current execution context.</param>
        /// <param name="sourceDocument">The source document.</param>
        /// <param name="items">The metadata items.</param>
        /// <returns>The cloned or new document.</returns>
        public static IDocument GetDocument(
            this IDocumentFactory documentFactory,
            IExecutionContext context,
            IDocument sourceDocument,
            IEnumerable<KeyValuePair<string, object>> items) =>
            documentFactory.GetDocument(context, sourceDocument, null, null, items);

        /// <summary>
        /// Clones the specified source document with a new content stream, and additional metadata (all existing metadata is retained)
        /// or gets a new document if the source document is null or <c>AsNewDocuments()</c> was called on the module.
        /// If <paramref name="disposeStream"/> is true (which it is by default), the provided
        /// <see cref="Stream"/> will automatically be disposed when the document is disposed (I.e., the
        /// document takes ownership of the <see cref="Stream"/>).
        /// </summary>
        /// <param name="documentFactory">The document factory.</param>
        /// <param name="context">The current execution context.</param>
        /// <param name="sourceDocument">The source document.</param>
        /// <param name="stream">The content stream.</param>
        /// <param name="items">The metadata items.</param>
        /// <param name="disposeStream">If set to <c>true</c> the provided <see cref="Stream"/> is disposed when the document is.</param>
        /// <returns>The cloned or new document.</returns>
        public static IDocument GetDocument(
            this IDocumentFactory documentFactory,
            IExecutionContext context,
            IDocument sourceDocument,
            Stream stream,
            IEnumerable<KeyValuePair<string, object>> items = null,
            bool disposeStream = true) =>
            documentFactory.GetDocument(context, sourceDocument, null, stream, items, disposeStream);
    }
}
