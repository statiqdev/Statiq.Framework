using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// Contains content and metadata for each item as it propagates through the pipeline.
    /// </summary>
    public interface IDocument : IMetadata
    {
        /// <summary>
        /// An identifier that is generated when the document is created and stays the same after cloning.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// An identifier for the document meant to reflect the source of the data. These should be unique (such as a file name).
        /// This property is always an absolute path. If you want to get a relative path, use <see cref="FilePath.GetRelativeInputPath(IExecutionContext)"/>.
        /// </summary>
        /// <value>
        /// The source of the document, or <c>null</c> if the document doesn't have a source.
        /// </value>
        FilePath Source { get; }

        /// <summary>
        /// The destination of the document. Can be either relative or absolute.
        /// </summary>
        FilePath Destination { get; }

        /// <summary>
        /// Gets the content associated with this document as a <see cref="Stream"/>.
        /// The underlying stream will be reset to position 0 each time this method is called.
        /// The stream you get from this call must be disposed as soon as reading is complete.
        /// </summary>
        /// <returns>A <see cref="Stream"/> of the content associated with this document.</returns>
        Task<Stream> GetStreamAsync();

        /// <summary>
        /// Indicates if this document has content (if not, <see cref="GetStreamAsync()"/> will return <see cref="Stream.Null"/>.
        /// </summary>
        bool HasContent { get; }

        /// <summary>
        /// The content provider responsible for creating content streams for the document.
        /// </summary>
        IContentProvider ContentProvider { get; }

        /// <summary>
        /// Gets a hash of the provided document content and metadata appropriate for caching.
        /// Custom <see cref="IDocument"/> implementations may also contribute additional state
        /// data to the resulting hash code.
        /// </summary>
        /// <returns>A hash appropriate for caching.</returns>
        Task<int> GetCacheHashCodeAsync();

        /// <summary>
        /// Clones this document.
        /// </summary>
        /// <param name="source">The new source. If this document already contains a source, then it's used and this is ignored.</param>
        /// <param name="destination">The new destination or <c>null</c> to keep the existing destination.</param>
        /// <param name="items">New metadata items or <c>null</c> not to add any new metadata.</param>
        /// <param name="contentProvider">The new content provider or <c>null</c> to keep the existing content provider.</param>
        /// <returns>A new document of the same type as this document.</returns>
        IDocument Clone(
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null);
    }
}