using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Common
{
    public partial interface IDocument
    {
        /// <inheritdoc />
        IContentProvider IContentProviderFactory.GetContentProvider() => ContentProvider;

        /// <inheritdoc />
        IContentProvider IContentProviderFactory.GetContentProvider(string mediaType) => ContentProvider.CloneWithMediaType(mediaType);

        /// <summary>
        /// Gets the content associated with this document as a <see cref="Stream"/>.
        /// The underlying stream will be reset to position 0 each time this method is called.
        /// The stream you get from this call must be disposed as soon as reading is complete.
        /// </summary>
        /// <returns>A <see cref="Stream"/> of the content associated with this document.</returns>
        public Stream GetContentStream() => ContentProvider.GetStream();

        /// <summary>
        /// Gets the content associated with this document as a string.
        /// This will result in reading the entire content stream.
        /// It's preferred to read directly as a stream using <see cref="GetContentStream"/> if possible.
        /// </summary>
        /// <value>The content associated with this document.</value>
        public async Task<string> GetContentStringAsync()
        {
            Stream stream = GetContentStream();
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
        /// <value>The content associated with this document.</value>
        public async Task<byte[]> GetContentBytesAsync()
        {
            using (Stream stream = GetContentStream())
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
        /// <param name="mediaType">The media type to check.</param>
        /// <returns><c>true</c> if the media types are equal, <c>false</c> otherwise.</returns>
        public bool MediaTypeEquals(string mediaType) =>
            string.Equals(ContentProvider.MediaType, mediaType, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Gets a hash of the provided document content and metadata appropriate for caching.
        /// Custom <see cref="IDocument"/> implementations may also contribute additional state
        /// data to the resulting hash code.
        /// </summary>
        /// <returns>A hash appropriate for caching.</returns>
        public async Task<int> GetCacheHashCodeAsync()
        {
            HashCode hash = default;
            using (Stream stream = GetContentStream())
            {
                hash.Add(await Crc32.CalculateAsync(stream));
            }

            // We exclude ContentProvider from hash as we already added CRC for content above.
            foreach (KeyValuePair<string, object> item in this
                .Where(x => x.Key != nameof(ContentProvider)))
            {
                hash.Add(item.Key);
                hash.Add(item.Value);
            }

            return hash.ToHashCode();
        }

        /// <summary>
        /// Gets a normalized title derived from the document source.
        /// </summary>
        /// <returns>A normalized title.</returns>
        public string GetTitle() => Source?.GetTitle();

        /// <inheritdoc />
        string IDisplayable.ToDisplayString() => Source?.ToDisplayString() ?? "unknown source";
    }
}
