using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Common
{
    public static class IDocumentCloneExtensions
    {
        /// <summary>
        /// Clones this document.
        /// </summary>
        /// <param name="document">The document to clone.</param>
        /// <param name="destination">The new destination or <c>null</c> to keep the existing destination.</param>
        /// <param name="items">New metadata items.</param>
        /// <param name="contentProvider">The new content provider or <c>null</c> to keep the existing content provider.</param>
        /// <returns>A new document of the same type as this document.</returns>
        public static IDocument Clone(
            this IDocument document,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
        {
            _ = document ?? throw new ArgumentNullException(nameof(document), "Attempted to clone a null document");
            return document.Clone(null, destination, items, contentProvider);
        }

        /// <summary>
        /// Clones this document.
        /// </summary>
        /// <param name="document">The document to clone.</param>
        /// <param name="source">The new source. If this document already contains a source, then it's used and this is ignored.</param>
        /// <param name="destination">The new destination or <c>null</c> to keep the existing destination.</param>
        /// <param name="contentProvider">The new content provider or <c>null</c> to keep the existing content provider.</param>
        /// <returns>A new document of the same type as this document.</returns>
        public static IDocument Clone(
            this IDocument document,
            FilePath source,
            FilePath destination,
            IContentProvider contentProvider = null)
        {
            _ = document ?? throw new ArgumentNullException(nameof(document), "Attempted to clone a null document");
            return document.Clone(source, destination, null, contentProvider);
        }

        /// <summary>
        /// Clones this document.
        /// </summary>
        /// <param name="document">The document to clone.</param>
        /// <param name="destination">The new destination or <c>null</c> to keep the existing destination.</param>
        /// <param name="contentProvider">The new content provider or <c>null</c> to keep the existing content provider.</param>
        /// <returns>A new document of the same type as this document.</returns>
        public static IDocument Clone(
            this IDocument document,
            FilePath destination,
            IContentProvider contentProvider = null)
        {
            _ = document ?? throw new ArgumentNullException(nameof(document), "Attempted to clone a null document");
            return document.Clone(null, destination, null, contentProvider);
        }

        /// <summary>
        /// Clones this document.
        /// </summary>
        /// <param name="document">The document to clone.</param>
        /// <param name="items">New metadata items or <c>null</c> not to add any new metadata.</param>
        /// <param name="contentProvider">The new content provider or <c>null</c> to keep the existing content provider.</param>
        /// <returns>A new document of the same type as this document.</returns>
        public static IDocument Clone(
            this IDocument document,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
        {
            _ = document ?? throw new ArgumentNullException(nameof(document), "Attempted to clone a null document");
            return document.Clone(null, null, items, contentProvider);
        }

        /// <summary>
        /// Clones this document.
        /// </summary>
        /// <param name="document">The document to clone.</param>
        /// <param name="contentProvider">The new content provider or <c>null</c> to keep the existing content provider.</param>
        /// <returns>A new document of the same type as this document.</returns>
        public static IDocument Clone(
            this IDocument document,
            IContentProvider contentProvider)
        {
            _ = document ?? throw new ArgumentNullException(nameof(document), "Attempted to clone a null document");
            return document.Clone(null, null, null, contentProvider);
        }
    }
}
