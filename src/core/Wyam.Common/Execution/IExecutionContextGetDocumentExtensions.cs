using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.IO;

namespace Wyam.Common.Execution
{
    public static class IExecutionContextGetDocumentExtensions
    {
        /// <summary>
        /// Gets a new document with the specified source, content stream, and metadata (in addition to the default initial metadata).
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="source">The source.</param>
        /// <param name="contentProvider">The content provider.</param>
        /// <param name="items">The metadata items.</param>
        /// <returns>The new document.</returns>
        public static IDocument GetDocument(
            this IExecutionContext context,
            FilePath source,
            IContentProvider contentProvider,
            IEnumerable<KeyValuePair<string, object>> items = null) =>
            context.GetDocument((IDocument)null, source, contentProvider, items);

        /// <summary>
        /// Gets a new document with the specified source and metadata (in addition to the default initial metadata).
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="source">The source.</param>
        /// <param name="items">The metadata items.</param>
        /// <returns>The new document.</returns>
        public static IDocument GetDocument(
            this IExecutionContext context,
            FilePath source,
            IEnumerable<KeyValuePair<string, object>> items = null) =>
            context.GetDocument((IDocument)null, source, (IContentProvider)null, items);

        /// <summary>
        /// Gets a new document with the specified content stream and metadata (in addition to the default initial metadata).
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="contentProvider">The content provider.</param>
        /// <param name="items">The metadata items.</param>
        public static IDocument GetDocument(
            this IExecutionContext context,
            IContentProvider contentProvider,
            IEnumerable<KeyValuePair<string, object>> items = null) =>
            context.GetDocument((IDocument)null, contentProvider, items);

        /// <summary>
        /// Clones the specified source document with a new content stream, and additional metadata (all existing metadata is retained)
        /// or gets a new document if the source document is null or <c>AsNewDocuments()</c> was called on the module.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="sourceDocument">The source document.</param>
        /// <param name="contentProvider">The content provider.</param>
        /// <param name="items">The metadata items.</param>
        public static IDocument GetDocument(
            this IExecutionContext context,
            IDocument sourceDocument,
            IContentProvider contentProvider,
            IEnumerable<KeyValuePair<string, object>> items = null) =>
            context.GetDocument(sourceDocument, null, contentProvider, items);

        /// <summary>
        /// Gets a new document with the specified metadata (in addition to the default initial metadata).
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="items">The metadata items.</param>
        /// <returns>The new document.</returns>
        public static IDocument GetDocument(
            this IExecutionContext context,
            IEnumerable<KeyValuePair<string, object>> items) =>
            context.GetDocument((IDocument)null, items);

        /// <summary>
        /// Clones the specified source document with a new source and additional metadata (all existing metadata is retained)
        /// or gets a new document if the source document is null or <c>AsNewDocuments()</c> was called on the module.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="sourceDocument">The source document.</param>
        /// <param name="source">The source (if the source document contains a source, then this is ignored and the source document's source is used instead).</param>
        /// <param name="items">The metadata items.</param>
        /// <returns>The cloned or new document.</returns>
        public static IDocument GetDocument(
            this IExecutionContext context,
            IDocument sourceDocument,
            FilePath source,
            IEnumerable<KeyValuePair<string, object>> items = null) =>
            context.GetDocument(sourceDocument, source, (IContentProvider)null, items);

        /// <summary>
        /// Gets a new document with default initial metadata.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <returns>The new document.</returns>
        public static IDocument GetDocument(this IExecutionContext context) =>
            context.GetDocument(null, null, null, null);

        /// <summary>
        /// Clones the specified source document with identical content and additional metadata (all existing metadata is retained)
        /// or gets a new document if the source document is null or <c>AsNewDocuments()</c> was called on the module.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="sourceDocument">The source document.</param>
        /// <param name="items">The metadata items.</param>
        /// <returns>The cloned or new document.</returns>
        public static IDocument GetDocument(
            this IExecutionContext context,
            IDocument sourceDocument,
            IEnumerable<KeyValuePair<string, object>> items) =>
            context.GetDocument(sourceDocument, null, null, items);

        /// <summary>
        /// Gets a new document with the specified source, content stream, and metadata (in addition to the default initial metadata).
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="source">The source.</param>
        /// <param name="content">The content.</param>
        /// <param name="items">The metadata items.</param>
        /// <returns>The new document.</returns>
        public static async Task<IDocument> GetDocumentAsync(
            this IExecutionContext context,
            FilePath source,
            string content,
            IEnumerable<KeyValuePair<string, object>> items = null) =>
            context.GetDocument((IDocument)null, source, await context.GetContentStreamAsync(content), items);

        /// <summary>
        /// Gets a new document with the specified content stream and metadata (in addition to the default initial metadata).
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="content">The content.</param>
        /// <param name="items">The metadata items.</param>
        /// <returns>The new document.</returns>
        public static async Task<IDocument> GetDocumentAsync(
            this IExecutionContext context,
            string content,
            IEnumerable<KeyValuePair<string, object>> items = null) =>
            context.GetDocument((IDocument)null, await context.GetContentStreamAsync(content), items);

        /// <summary>
        /// Clones the specified source document with a new source, new content stream, and additional metadata (all existing metadata is retained)
        /// or gets a new document if the source document is null or <c>AsNewDocuments()</c> was called on the module.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="sourceDocument">The source document.</param>
        /// <param name="source">The source (if the source document contains a source, then this is ignored and the source document's source is used instead).</param>
        /// <param name="content">The content.</param>
        /// <param name="items">The metadata items.</param>
        /// <returns>The cloned or new document.</returns>
        public static async Task<IDocument> GetDocumentAsync(
            this IExecutionContext context,
            IDocument sourceDocument,
            FilePath source,
            string content,
            IEnumerable<KeyValuePair<string, object>> items = null) =>
            context.GetDocument(sourceDocument, source, await context.GetContentStreamAsync(content), items);

        /// <summary>
        /// Clones the specified source document with a new content stream, and additional metadata (all existing metadata is retained)
        /// or gets a new document if the source document is null or <c>AsNewDocuments()</c> was called on the module.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="sourceDocument">The source document.</param>
        /// <param name="content">The content.</param>
        /// <param name="items">The metadata items.</param>
        /// <returns>The cloned or new document.</returns>
        public static async Task<IDocument> GetDocumentAsync(
            this IExecutionContext context,
            IDocument sourceDocument,
            string content,
            IEnumerable<KeyValuePair<string, object>> items = null) =>
            context.GetDocument(sourceDocument, await context.GetContentStreamAsync(content), items);
    }
}
