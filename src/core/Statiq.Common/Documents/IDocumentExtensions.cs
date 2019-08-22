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
