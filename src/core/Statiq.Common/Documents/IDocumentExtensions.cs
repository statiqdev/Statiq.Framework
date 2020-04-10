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
        /// Gets the content associated with this document as a <see cref="Stream"/>.
        /// The underlying stream will be reset to position 0 each time this method is called.
        /// The stream you get from this call must be disposed as soon as reading is complete.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>A <see cref="Stream"/> of the content associated with this document.</returns>
        public static Stream GetContentStream(this IDocument document) => document.ContentProvider.GetStream();

        /// <summary>
        /// Gets the content associated with this document as a string.
        /// This will result in reading the entire content stream.
        /// It's preferred to read directly as a stream using <see cref="GetContentStream"/> if possible.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <value>The content associated with this document.</value>
        public static async Task<string> GetContentStringAsync(this IDocument document)
        {
            Stream stream = document.GetContentStream();
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
        /// It's preferred to read directly as a stream using <see cref="GetContentStream"/> if possible.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <value>The content associated with this document.</value>
        public static async Task<byte[]> GetContentBytesAsync(this IDocument document)
        {
            using (Stream stream = document.GetContentStream())
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
        /// Determines if the supplied media type equals the media type of the content.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="mediaType">The media type to check.</param>
        /// <returns><c>true</c> if the media types are equal, <c>false</c> otherwise.</returns>
        public static bool MediaTypeEquals(this IDocument document, string mediaType) =>
            string.Equals(document.ContentProvider.MediaType, mediaType, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Determines if a document is equal by comparing their IDs.
        /// </summary>
        /// <param name="document">The first document.</param>
        /// <param name="other">The second document.</param>
        /// <returns><c>true</c> if the documents have the same ID (they come from the same initial document), <c>false</c> otherwise.</returns>
        public static bool IdEquals(this IDocument document, IDocument other) =>
            DocumentIdComparer.Instance.Equals(document, other);

        /// <summary>
        /// Gets a normalized title derived from the document source (or <see cref="Keys.Title"/> if defined).
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>A normalized title or <c>null</c> if the source is null.</returns>
        public static string GetTitle(this IDocument document) => document.GetString(Keys.Title) ?? document.Source.GetTitle();

        /// <summary>
        /// Presents the metadata of a document as a dynamic object. Cast the return object to <see cref="IDocument"/>
        /// to convert it back to a document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>A dynamic object that contains the document metadata.</returns>
        public static dynamic AsDynamic(this IDocument document) => new DynamicDocument(document ?? throw new ArgumentNullException(nameof(document)));
    }
}
