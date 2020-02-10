using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Statiq.Common
{
    /// <summary>
    /// Extensions for working with specific types of collections.
    /// </summary>
    public static class IDocumentEnumerableExtensions
    {
        /// <summary>
        /// Creates an immutable array from the specified document collection and removes null items.
        /// </summary>
        /// <typeparam name="TDocument">The document type.</typeparam>
        /// <param name="documents">The documents to convert to an immutable array.</param>
        /// <returns>An immutable array of documents.</returns>
        public static ImmutableArray<TDocument> ToImmutableDocumentArray<TDocument>(this IEnumerable<TDocument> documents)
            where TDocument : IDocument
        {
            if (documents == null)
            {
                return ImmutableArray<TDocument>.Empty;
            }

            // Convert to unsorted immutable array while eliminating nulls
            return documents is ImmutableArray<TDocument> documentsArray && !documentsArray.Any(x => x == null)
                ? documentsArray
                : documents.Where(x => x != null).ToImmutableArray();
        }

        /// <summary>
        /// Returns all documents that contain the specified metadata key.
        /// </summary>
        /// <typeparam name="TDocument">The document type.</typeparam>
        /// <param name="documents">The documents.</param>
        /// <param name="metadataKey">The key.</param>
        /// <returns>All documents that contain the specified metadata key.</returns>
        public static IEnumerable<TDocument> WhereContainsKey<TDocument>(this IEnumerable<TDocument> documents, string metadataKey)
            where TDocument : IDocument =>
            documents.Where(x => x.ContainsKey(metadataKey));

        /// <summary>
        /// Returns all documents that contain all of the specified metadata keys.
        /// </summary>
        /// <typeparam name="TDocument">The document type.</typeparam>
        /// <param name="documents">The documents.</param>
        /// <param name="metadataKeys">The metadata keys.</param>
        /// <returns>All documents that contain all of the specified metadata keys.</returns>
        public static IEnumerable<TDocument> WhereContainsAllKeys<TDocument>(this IEnumerable<TDocument> documents, params string[] metadataKeys)
            where TDocument : IDocument =>
            documents.Where(x => metadataKeys.All(x.ContainsKey));

        /// <summary>
        /// Returns all documents that contain any of the specified metadata keys.
        /// </summary>
        /// <typeparam name="TDocument">The document type.</typeparam>
        /// <param name="documents">The documents.</param>
        /// <param name="metadataKeys">The metadata keys.</param>
        /// <returns>All documents that contain any of the specified metadata keys.</returns>
        public static IEnumerable<TDocument> WhereContainsAnyKeys<TDocument>(this IEnumerable<TDocument> documents, params string[] metadataKeys)
            where TDocument : IDocument =>
            documents.Where(x => metadataKeys.Any(x.ContainsKey));

        /// <summary>
        /// Filters the documents by source.
        /// </summary>
        /// <remarks>
        /// This module filters documents using "or" logic. If you want to also apply
        /// "and" conditions, chain additional calls.
        /// </remarks>
        /// <typeparam name="TDocument">The document type.</typeparam>
        /// <param name="documents">The documents.</param>
        /// <param name="context">The current execution context.</param>
        /// <param name="patterns">The globbing pattern(s) to match.</param>
        /// <returns>The documents that match the globbing pattern(s).</returns>
        public static IEnumerable<TDocument> FilterSources<TDocument>(this IEnumerable<TDocument> documents, IExecutionContext context, params string[] patterns)
            where TDocument : IDocument =>
            documents.FilterSources(context, (IEnumerable<string>)patterns);

        /// <summary>
        /// Filters the documents by source.
        /// </summary>
        /// <remarks>
        /// This module filters documents using "or" logic. If you want to also apply
        /// "and" conditions, chain additional calls.
        /// </remarks>
        /// <typeparam name="TDocument">The document type.</typeparam>
        /// <param name="documents">The documents.</param>
        /// <param name="context">The current execution context.</param>
        /// <param name="patterns">The globbing pattern(s) to match.</param>
        /// <returns>The documents that match the globbing pattern(s).</returns>
        public static IEnumerable<TDocument> FilterSources<TDocument>(this IEnumerable<TDocument> documents, IExecutionContext context, IEnumerable<string> patterns)
            where TDocument : IDocument
        {
            _ = documents ?? throw new ArgumentNullException(nameof(documents));
            _ = context ?? throw new ArgumentNullException(nameof(context));
            _ = patterns ?? throw new ArgumentNullException(nameof(patterns));

            DocumentFileProvider fileProvider = new DocumentFileProvider((IEnumerable<IDocument>)documents, true);
            IEnumerable<IDirectory> directories = context.FileSystem
                .GetInputDirectories()
                .Select(x => fileProvider.GetDirectory(x.Path));
            IEnumerable<IFile> matches = directories.SelectMany(x => Globber.GetFiles(x, patterns));
            return matches.Select(x => x.Path).Distinct().Select(match => fileProvider.GetDocument(match)).Cast<TDocument>();
        }

        /// <summary>
        /// Filters the documents by destination.
        /// </summary>
        /// <remarks>
        /// This module filters documents using "or" logic. If you want to also apply
        /// "and" conditions, chain additional calls.
        /// </remarks>
        /// <typeparam name="TDocument">The document type.</typeparam>
        /// <param name="documents">The documents.</param>
        /// <param name="patterns">The globbing pattern(s) to match.</param>
        /// <returns>The documents that match the globbing pattern(s).</returns>
        public static IEnumerable<TDocument> FilterDestinations<TDocument>(this IEnumerable<TDocument> documents, params string[] patterns)
            where TDocument : IDocument =>
            documents.FilterDestinations((IEnumerable<string>)patterns);

        /// <summary>
        /// Filters the documents by destination.
        /// </summary>
        /// <remarks>
        /// This module filters documents using "or" logic. If you want to also apply
        /// "and" conditions, chain additional calls.
        /// </remarks>
        /// <typeparam name="TDocument">The document type.</typeparam>
        /// <param name="documents">The documents.</param>
        /// <param name="patterns">The globbing pattern(s) to match.</param>
        /// <returns>The documents that match the globbing pattern(s).</returns>
        public static IEnumerable<TDocument> FilterDestinations<TDocument>(this IEnumerable<TDocument> documents, IEnumerable<string> patterns)
            where TDocument : IDocument
        {
            _ = documents ?? throw new ArgumentNullException(nameof(documents));
            _ = patterns ?? throw new ArgumentNullException(nameof(patterns));

            DocumentFileProvider fileProvider = new DocumentFileProvider((IEnumerable<IDocument>)documents, false);
            IEnumerable<IFile> matches = Globber.GetFiles(fileProvider.GetDirectory("/"), patterns);
            return matches.Select(x => x.Path).Distinct().Select(match => fileProvider.GetDocument(match)).Cast<TDocument>();
        }
    }
}
