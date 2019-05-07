using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Execution;
using Wyam.Common.IO;

namespace Wyam.Common.Documents
{
    /// <summary>
    /// Contains the content for a document. Instances will
    /// be automatically reference-counted and disposed when
    /// the last consuming document is disposed (and not before).
    /// </summary>
    public interface IContentProvider : IDisposable
    {
        /// <summary>
        /// Gets the content stream. The returned stream will be disposed
        /// after use.
        /// </summary>
        /// <returns>The content stream for a document.</returns>
        Task<Stream> GetStreamAsync();
    }
}
