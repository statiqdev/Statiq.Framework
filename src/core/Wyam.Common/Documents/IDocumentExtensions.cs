using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Meta;
using Wyam.Common.Util;

namespace Wyam.Common.Documents
{
    public static class IDocumentExtensions
    {
        /// <summary>
        /// Gets a hash of the provided document content and metadata.
        /// </summary>
        /// <param name="document">The document to hash.</param>
        /// <returns>A hash that represents the document content and it's metadata.</returns>
        public static async Task<int> GetHashAsync(this IDocument document)
        {
            if (document == null)
            {
                return 0;
            }

            HashCode hash = default;
            using (Stream stream = await document.GetStreamAsync())
            {
                hash.Add(await Crc32.CalculateAsync(stream));
            }
            foreach (KeyValuePair<string, object> item in document.WithoutSettings)
            {
                hash.Add(item.Key);
                hash.Add(item.Value);
            }

            return hash.ToHashCode();
        }

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
