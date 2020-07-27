using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Statiq.Common
{
    public class DocumentList<TDocument> : IReadOnlyList<TDocument>
        where TDocument : IDocument
    {
        public static readonly DocumentList<TDocument> Empty = new DocumentList<TDocument>(null);

        private readonly IReadOnlyList<TDocument> _documents;

        public DocumentList(IEnumerable<TDocument> documents)
        {
            // If it's null create an empty list
            if (documents == null)
            {
                _documents = Array.Empty<TDocument>();
            }
            else
            {
                // If it's already a list make sure there aren't any null entries
                _documents = documents as IReadOnlyList<TDocument>;
                if (_documents != null && _documents.Any(x => x == null))
                {
                    // It was a list but there were null items
                    _documents = null;
                }

                // Create a new list without the null items
                _documents = documents.Where(x => x != null).ToArray();
            }
        }

        /// <summary>
        /// Returns documents with destination paths that satisfy the globbing pattern(s).
        /// </summary>
        /// <param name="destinationPatterns">The globbing pattern(s) to filter by (can be a single path).</param>
        /// <returns>The documents that satisfy the pattern or <c>null</c>.</returns>
        public IEnumerable<TDocument> this[params string[] destinationPatterns] => _documents.FilterDestinations(destinationPatterns);

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
