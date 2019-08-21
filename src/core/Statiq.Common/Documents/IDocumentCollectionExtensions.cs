using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Statiq.Common
{
    public static class IDocumentCollectionExtensions
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
    }
}
