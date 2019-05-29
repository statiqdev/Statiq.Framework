using System;
using System.IO;
using System.Threading.Tasks;
using Wyam.Common.IO;
using Wyam.Common.Meta;

namespace Wyam.Common.Documents
{
    /// <summary>
    /// Contains content and metadata for each item as it propagates through the pipeline.
    /// </summary>
    /// <remarks>
    /// Documents are immutable so you must call one of the <c>GetDocument</c> methods of <see cref="IDocumentFactory"/>
    /// to create a new document. Implements <see cref="IMetadata"/> and all metadata calls are passed through
    /// to the document's internal <see cref="IMetadata"/> instance (exposed via the <see cref="Metadata"/>
    /// property). Note that the result of both the <see cref="GetStringAsync"/> and the <see cref="GetStreamAsync"/>
    /// methods are guaranteed not to be null. When a document is created, either a string or a <see cref="Stream"/>
    /// is provided. Whenever the other of the two is requested, the system will convert the current representation
    /// for you.
    /// </remarks>
    public interface IDocument : IMetadata, IDisposable
    {
        /// <summary>
        /// An identifier for the document meant to reflect the source of the data. These should be unique (such as a file name).
        /// This property is always an absolute path. If you want to get a relative path, use <see cref="FilePath.GetRelativeInputPath(Execution.IExecutionContext)"/>.
        /// </summary>
        /// <value>
        /// The source of the document, or <c>null</c> if the document doesn't have a source.
        /// </value>
        FilePath Source { get; }

        /// <summary>
        /// The destination of the document. Can be either relative or absolute.
        /// </summary>
        FilePath Destination { get; }

        /// <summary>An identifier that is generated when the document is created and stays the same after cloning.</summary>
        string Id { get; }

        /// <summary>
        /// A document version that gets incremented on every clone operation. Starts at 0.
        /// </summary>
        int Version { get; }

        /// <summary>Gets the metadata associated with this document.</summary>
        IMetadata Metadata { get; }

        /// <summary>
        /// Gets the content associated with this document as a string.
        /// This will result in reading the entire content stream.
        /// It's prefered to read directly as a stream using <see cref="GetStreamAsync"/> if possible.
        /// </summary>
        /// <value>The content associated with this document.</value>
        Task<string> GetStringAsync();

        /// <summary>
        /// Gets the content associated with this document as a <see cref="Stream"/>.
        /// The underlying stream will be reset to position 0 each time this method is called.
        /// The stream you get from this call must be disposed as soon as reading is complete.
        /// </summary>
        /// <returns>A <see cref="Stream"/> of the content associated with this document.</returns>
        Task<Stream> GetStreamAsync();

        /// <summary>
        /// Indicates if this document has content (if not, <see cref="GetStreamAsync()"/> will
        /// return <see cref="Stream.Null"/> and <see cref="GetStringAsync()"/> will return <see cref="string.Empty"/>.
        /// </summary>
        bool HasContent { get; }

        /// <summary>
        /// Gets the metadata for this document without any global settings included.
        /// </summary>
        /// <returns>The document metadata without global settings.</returns>
        IMetadata WithoutSettings { get; }

        /// <summary>
        /// Gets a hash of the provided document content and metadata appropriate for caching.
        /// Custom <see cref="IDocument"/> implementations may also contribute additional state
        /// data to the resulting hash code.
        /// </summary>
        /// <returns>A hash appropriate for caching.</returns>
        Task<int> GetCacheHashCodeAsync();
    }
}