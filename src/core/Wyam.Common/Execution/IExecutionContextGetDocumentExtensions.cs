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
        /// If <paramref name="disposeStream"/> is true (which it is by default), the provided
        /// <see cref="Stream"/> will automatically be disposed when the document is disposed (I.e., the
        /// document takes ownership of the <see cref="Stream"/>).
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="source">The source.</param>
        /// <param name="stream">The content stream.</param>
        /// <param name="items">The metadata items.</param>
        /// <param name="disposeStream">If set to <c>true</c> the provided <see cref="Stream"/> is disposed when the document is.</param>
        /// <returns>The new document.</returns>
        public static IDocument GetDocument(
            this IExecutionContext context,
            FilePath source,
            Stream stream,
            IEnumerable<KeyValuePair<string, object>> items = null,
            bool disposeStream = true) =>
            context.GetDocument((IDocument)null, source, stream, items, disposeStream);

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
            context.GetDocument((IDocument)null, source, (Stream)null, items);

        /// <summary>
        /// Gets a new document with the specified content stream and metadata (in addition to the default initial metadata).
        /// If <paramref name="disposeStream"/> is true (which it is by default), the provided
        /// <see cref="Stream"/> will automatically be disposed when the document is disposed (I.e., the
        /// document takes ownership of the <see cref="Stream"/>).
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="stream">The content stream.</param>
        /// <param name="items">The metadata items.</param>
        /// <param name="disposeStream">If set to <c>true</c> the provided <see cref="Stream"/> is disposed when the document is.</param>
        /// <returns>The new document.</returns>
        public static IDocument GetDocument(
            this IExecutionContext context,
            Stream stream,
            IEnumerable<KeyValuePair<string, object>> items = null,
            bool disposeStream = true) =>
            context.GetDocument((IDocument)null, stream, items, disposeStream);

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
            context.GetDocument(sourceDocument, source, (Stream)null, items);

        /// <summary>
        /// Gets a new document with default initial metadata.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <returns>The new document.</returns>
        public static IDocument GetDocument(this IExecutionContext context) =>
            context.GetDocument(null, null, null, null);

        /// <summary>
        /// Clones the specified source document with a new content stream, and additional metadata (all existing metadata is retained)
        /// or gets a new document if the source document is null or <c>AsNewDocuments()</c> was called on the module.
        /// If <paramref name="disposeStream"/> is true (which it is by default), the provided
        /// <see cref="Stream"/> will automatically be disposed when the document is disposed (I.e., the
        /// document takes ownership of the <see cref="Stream"/>).
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="sourceDocument">The source document.</param>
        /// <param name="stream">The content stream.</param>
        /// <param name="items">The metadata items.</param>
        /// <param name="disposeStream">If set to <c>true</c> the provided <see cref="Stream"/> is disposed when the document is.</param>
        /// <returns>The cloned or new document.</returns>
        public static IDocument GetDocument(
            this IExecutionContext context,
            IDocument sourceDocument,
            Stream stream,
            IEnumerable<KeyValuePair<string, object>> items = null,
            bool disposeStream = true) =>
            context.GetDocument(sourceDocument, null, stream, items, disposeStream);

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
    }
}
