using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Statiq.Common
{
    /// <summary>
    /// A list of documents.
    /// </summary>
    /// <typeparam name="TDocument">The document type the list contains.</typeparam>
    public class DocumentList<TDocument> : IReadOnlyList<TDocument>
        where TDocument : IDocument
    {
        public static readonly DocumentList<TDocument> Empty = new DocumentList<TDocument>(null);

        private readonly IReadOnlyList<TDocument> _documents;

        public DocumentList(IEnumerable<TDocument> documents)
        {
            if (documents is null)
            {
                // If it's null create an empty list
                _documents = Array.Empty<TDocument>();
            }
            else if (documents is IReadOnlyList<TDocument> list && !list.Any(x => x is null))
            {
                // It's already a list and doesn't contain nulls
                _documents = list;
            }
            else
            {
                // Remove nulls
                _documents = documents.Where(x => x is object).ToArray();
            }
        }

        /// <summary>
        /// Returns documents with destination paths that satisfy the globbing pattern(s).
        /// </summary>
        /// <param name="destinationPatterns">The globbing pattern(s) to filter by (can be a single path).</param>
        /// <returns>The documents that satisfy the pattern or <c>null</c>.</returns>
        public FilteredDocumentList<TDocument> this[params string[] destinationPatterns] => _documents.FilterDestinations(destinationPatterns);

        // IReadOnlyList<IDocument>

        /// <inheritdoc />
        public TDocument this[int index] => _documents[index];

        /// <inheritdoc />
        public int Count => _documents.Count;

        /// <inheritdoc />
        public IEnumerator<TDocument> GetEnumerator() => _documents.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => _documents.GetEnumerator();
    }
}
