using System.Collections.Generic;
using Wyam.Common.Content;
using Wyam.Common.Documents;
using Wyam.Common.IO;

namespace Wyam.Common.Execution
{
    public static class IExecutionContextGetDocumentExtensions
    {
        /// <summary>
        /// Gets a new document with the specified source and destination, content, and metadata.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="metadata">The metadata items.</param>
        /// <param name="contentProvider">The content provider.</param>
        /// <returns>The new document.</returns>
        public static IDocument GetDocument(
            this IExecutionContext context,
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> metadata = null,
            IContentProvider contentProvider = null) =>
            context.GetDocument(
                null,
                source,
                destination,
                metadata,
                contentProvider);

        /// <summary>
        /// Gets a new document with the specified source and destination, content, and metadata.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="contentProvider">The content provider.</param>
        /// <returns>The new document.</returns>
        public static IDocument GetDocument(
            this IExecutionContext context,
            FilePath source,
            FilePath destination,
            IContentProvider contentProvider = null) =>
            context.GetDocument(
                null,
                source,
                destination,
                null,
                contentProvider);

        /// <summary>
        /// Gets a new document with the specified content and metadata.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="metadata">The metadata items.</param>
        /// <param name="contentProvider">The content provider.</param>
        /// <returns>The new document.</returns>
        public static IDocument GetDocument(
            this IExecutionContext context,
            IEnumerable<KeyValuePair<string, object>> metadata,
            IContentProvider contentProvider = null) =>
            context.GetDocument(
                null,
                null,
                null,
                metadata,
                contentProvider);

        /// <summary>
        /// Gets a new document with the specified content.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="contentProvider">The content provider.</param>
        /// <returns>The new document.</returns>
        public static IDocument GetDocument(
            this IExecutionContext context,
            IContentProvider contentProvider = null) =>
            context.GetDocument(
                null,
                null,
                null,
                null,
                contentProvider);

        /// <summary>
        /// Clones the original document with a new destination, new content, and additional metadata (all existing metadata is retained)
        /// or gets a new document if the original document is null or <c>AsNewDocuments()</c> was called on the module.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="originalDocument">The original document.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="metadata">The metadata items.</param>
        /// <param name="contentProvider">The content provider.</param>
        /// <returns>The cloned or new document.</returns>
        public static IDocument GetDocument(
            this IExecutionContext context,
            IDocument originalDocument,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> metadata = null,
            IContentProvider contentProvider = null) =>
            context.GetDocument(
                originalDocument,
                null,
                destination,
                metadata,
                contentProvider);

        /// <summary>
        /// Clones the original document with new content and additional metadata (all existing metadata is retained)
        /// or gets a new document if the original document is null or <c>AsNewDocuments()</c> was called on the module.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="originalDocument">The original document.</param>
        /// <param name="metadata">The metadata items.</param>
        /// <param name="contentProvider">The content provider.</param>
        /// <returns>The cloned or new document.</returns>
        public static IDocument GetDocument(
            this IExecutionContext context,
            IDocument originalDocument,
            IEnumerable<KeyValuePair<string, object>> metadata,
            IContentProvider contentProvider = null) =>
            context.GetDocument(
                originalDocument,
                null,
                null,
                metadata,
                contentProvider);

        /// <summary>
        /// Clones the original document with new content
        /// or gets a new document if the original document is null or <c>AsNewDocuments()</c> was called on the module.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="originalDocument">The original document.</param>
        /// <param name="contentProvider">The content provider.</param>
        /// <returns>The cloned or new document.</returns>
        public static IDocument GetDocument(
            this IExecutionContext context,
            IDocument originalDocument,
            IContentProvider contentProvider = null) =>
            context.GetDocument(
                originalDocument,
                null,
                null,
                null,
                contentProvider);
    }
}
