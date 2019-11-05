using System.IO;
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
        /// Gets the media type of the content.
        /// </summary>
        /// <remarks>
        /// A registered IANA media type will be used if available.
        /// Unregistered media type names may be returned if a registered type is unavailable.
        /// If the media type is unknown this may null or empty.
        /// </remarks>
        string MediaType { get; }
    }
}
