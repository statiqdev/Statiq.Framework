using System;
using System.IO;
using System.Threading.Tasks;

namespace Statiq.Common.Content
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
        Task<Stream> GetStreamAsync();
    }
}
