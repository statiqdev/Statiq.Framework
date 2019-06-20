using System.Collections.Generic;
using Statiq.Common.Content;
using Statiq.Common.Documents;
using Statiq.Common.IO;

namespace Statiq.Common.Execution
{
    public static class IExecutionContextGetDocumentExtensions
    {
        /// <summary>
        /// Gets a new document with the specified source and destination, content, and metadata.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="items">The metadata items.</param>
        /// <param name="contentProvider">The content provider.</param>
        /// <returns>The new document.</returns>
        public static IDocument GetDocument(
            this IExecutionContext context,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            context.GetDocument(
                null,
                destination,
                items,
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
                source,
                destination,
                null,
                contentProvider);

        /// <summary>
        /// Gets a new document with the specified source and destination, content, and metadata.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="contentProvider">The content provider.</param>
        /// <returns>The new document.</returns>
        public static IDocument GetDocument(
            this IExecutionContext context,
            FilePath destination,
            IContentProvider contentProvider = null) =>
            context.GetDocument(
                null,
                destination,
                null,
                contentProvider);

        /// <summary>
        /// Gets a new document with the specified content and metadata.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="items">The metadata items.</param>
        /// <param name="contentProvider">The content provider.</param>
        /// <returns>The new document.</returns>
        public static IDocument GetDocument(
            this IExecutionContext context,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            context.GetDocument(
                null,
                null,
                items,
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
                contentProvider);
    }
}
