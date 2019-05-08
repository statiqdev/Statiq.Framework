using System;
using System.IO;
using System.Threading.Tasks;

namespace Wyam.Common.Content
{
    /// <summary>
    /// Contains the content for a document. Instances will
    /// be automatically reference-counted and disposed when
    /// the last consuming document is disposed (and not before).
    /// </summary>
    public interface IContentProvider : IDisposable
    {
        /// <summary>
        /// Gets the content stream. The returned stream should be disposed after use.
        /// </summary>
        /// <returns>The content stream for a document.</returns>
        Task<Stream> GetStreamAsync();
    }
}
