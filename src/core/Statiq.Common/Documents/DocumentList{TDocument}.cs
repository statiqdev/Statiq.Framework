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
        /// Returns the first document that satisfies the pattern.
        /// </summary>
        /// <param name="destinationPattern">The globbing pattern to filter by (can be a single path).</param>
        /// <returns>The first document that satisfies the pattern or <c>null</c>.</returns>
        public TDocument this[string destinationPattern] => _documents.FirstOrDefaultDestination(destinationPattern);

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
