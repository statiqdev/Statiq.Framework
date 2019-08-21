using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Common
{
    public static class IDocumentExtensions
    {
        /// <summary>
        /// Gets the content associated with this document as a string.
        /// This will result in reading the entire content stream.
        /// It's preferred to read directly as a stream using <see cref="IDocument.GetStream"/> if possible.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <value>The content associated with this document.</value>
        public static async Task<string> GetStringAsync(this IDocument document)
        {
            Stream stream = document.GetStream();
            if (stream == null || stream == Stream.Null)
            {
                return string.Empty;
            }
            using (StreamReader reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        /// <summary>
        /// Gets the content associated with this document as a byte array.
        /// This will result in reading the entire content stream.
        /// It's preferred to read directly as a stream using <see cref="IDocument.GetStream"/> if possible.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <value>The content associated with this document.</value>
        public static async Task<byte[]> GetBytesAsync(this IDocument document)
        {
            using (Stream stream = document.GetStream())
            {
                if (stream == null || stream == Stream.Null)
                {
                    return Array.Empty<byte>();
                }
                MemoryStream memory = new MemoryStream();
                await stream.CopyToAsync(memory);
                return memory.ToArray();
            }
        }

        /// <summary>
        /// Recursively flattens metadata that contains a document or documents.
        /// </summary>
        /// <param name="document">The parent document to flatten.</param>
        /// <returns>A set containing all nested documents in metadata including the parent document.</returns>
        /// <param name="key">If <c>null</c> aggregates children from all metadata keys, otherwise only gathers children from the specified key.</param>
        public static HashSet<IDocument> Flatten(this IDocument document, string key = null)
        {
            _ = document ?? throw new ArgumentNullException(nameof(document));
            HashSet<IDocument> results = new HashSet<IDocument>();
            document.Flatten(results, key);
            return results;
        }

        /// <summary>
        /// Recursively flattens metadata that contains a document or documents.
        /// </summary>
        /// <param name="document">The parent document to flatten.</param>
        /// <param name="results">A set containing all nested documents in metadata including the parent document.</param>
        /// <param name="key">If <c>null</c> aggregates children from all metadata keys, otherwise only gathers children from the specified key.</param>
        public static void Flatten(this IDocument document, HashSet<IDocument> results, string key = null)
        {
            _ = document ?? throw new ArgumentNullException(nameof(document));

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

        /// <summary>
        /// Gets a normalized title derived from the document source.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <returns>A normalized title.</returns>
        public static string GetTitle(this IDocument doc) => doc.Source?.GetTitle();

        /// <summary>
        /// Returns a document enumerable given a single document. This is just a convenience
        /// method for converting a single document into an <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="document">The document to return.</param>
        /// <returns>A document enumerable.</returns>
        public static IEnumerable<TDocument> Yield<TDocument>(this TDocument document)
            where TDocument : IDocument
        {
            yield return document;
        }

        /// <summary>
        /// Returns a document enumerable given a single document. This is just a convenience
        /// method for converting a single document into an <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="document">The document to return.</param>
        /// <returns>A document enumerable.</returns>
        public static Task<IEnumerable<TDocument>> YieldAsync<TDocument>(this TDocument document)
            where TDocument : IDocument
        {
            return Task.FromResult(document.Yield());
        }
    }
}
