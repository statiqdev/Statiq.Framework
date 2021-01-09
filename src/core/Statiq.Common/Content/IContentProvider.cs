using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// Contains the content for a document.
    /// </summary>
    public interface IContentProvider
    {
        /// <summary>
        /// Gets the content stream. The returned stream should be disposed after use.
        /// </summary>
        /// <returns>The content stream for a document.</returns>
        Stream GetStream();

        /// <summary>
        /// Gets an appropriate <see cref="TextReader"/> for the content. This is prefered
        /// over reading the stream as text since it might be optimized for text-based use cases.
        /// The returned <see cref="TextReader"/> should be disposed after use.
        /// </summary>
        /// <returns>A <see cref="TextReader"/> for the content.</returns>
        TextReader GetTextReader();

        /// <summary>
        /// Gets the media type of the content.
        /// </summary>
        /// <remarks>
        /// A registered IANA media type will be used if available.
        /// Unregistered media type names may be returned if a registered type is unavailable.
        /// If the media type is unknown this may null or empty.
        /// </remarks>
        string MediaType { get; }

        /// <summary>
        /// Clones the current content provider with a new media type.
        /// </summary>
        /// <param name="mediaType">The new media type.</param>
        /// <returns>A clone of the current content provider.</returns>
        IContentProvider CloneWithMediaType(string mediaType);

        /// <summary>
        /// Gets a hash of the content appropriate for caching.
        /// </summary>
        /// <returns>A hash appropriate for caching.</returns>
        Task<int> GetCacheHashCodeAsync();
    }
}
