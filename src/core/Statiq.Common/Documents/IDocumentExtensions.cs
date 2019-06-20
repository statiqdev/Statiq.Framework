using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Statiq.Common.Content;
using Statiq.Common.IO;
using Statiq.Common.Meta;
using Statiq.Common.Util;

namespace Statiq.Common.Documents
{
    public static class IDocumentExtensions
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
            IContentProvider contentProvider = null) =>
            document.Clone(null, destination, items, contentProvider);

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
            IContentProvider contentProvider = null) =>
            document.Clone(source, destination, null, contentProvider);

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
            IContentProvider contentProvider = null) =>
            document.Clone(null, destination, null, contentProvider);

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
            IContentProvider contentProvider = null) =>
            document.Clone(null, null, items, contentProvider);

        /// <summary>
        /// Clones this document.
        /// </summary>
        /// <param name="document">The document to clone.</param>
        /// <param name="contentProvider">The new content provider or <c>null</c> to keep the existing content provider.</param>
        /// <returns>A new document of the same type as this document.</returns>
        public static IDocument Clone(
            this IDocument document,
            IContentProvider contentProvider) =>
            document.Clone(null, null, null, contentProvider);

        /// <summary>
        /// Recursivly flattens metadata that contains a document or documents.
        /// </summary>
        /// <param name="document">The parent document to flatten.</param>
        /// <returns>A set containing all nested documents in metadata including the parent document.</returns>
        /// <param name="key">If <c>null</c> aggregates children from all metadata keys, otherwise only gathers children from the specified key.</param>
        public static HashSet<IDocument> Flatten(this IDocument document, string key = null)
        {
            HashSet<IDocument> results = new HashSet<IDocument>();
            document.Flatten(results, key);
            return results;
        }

        /// <summary>
        /// Recursivly flattens metadata that contains a document or documents.
        /// </summary>
        /// <param name="document">The parent document to flatten.</param>
        /// <param name="results">A set containing all nested documents in metadata including the parent document.</param>
        /// <param name="key">If <c>null</c> aggregates children from all metadata keys, otherwise only gathers children from the specified key.</param>
        public static void Flatten(this IDocument document, HashSet<IDocument> results, string key = null)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            if (document == null)
            {
                return;
            }

            // Use a stack so we don't overflow the call stack with recursive calls for deep trees
            Stack<IDocument> stack = new Stack<IDocument>();
            stack.Push(document);
            while (stack.Count > 0)
            {
                IDocument current = stack.Pop();

                // Only process if we haven't already processed this document
                if (results.Add(current))
                {
                    IEnumerable<IDocument> children = key == null
                        ? current.SelectMany(x => current.DocumentList(x.Key) ?? Array.Empty<IDocument>())
                        : current.DocumentList(key);
                    if (children != null)
                    {
                        foreach (IDocument child in children)
                        {
                            stack.Push(child);
                        }
                    }
                }
            }
        }
    }
}
