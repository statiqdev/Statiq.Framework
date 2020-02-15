using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// Contains content and metadata for each item as it propagates through the pipeline.
    /// </summary>
    public interface IDocument : IMetadata, IDisplayable, IContentProviderFactory
    {
        /// <summary>
        /// An identifier that is generated when the document is created and stays the same after cloning.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// An identifier for the document meant to reflect the source of the data. These should be unique (such as a file name).
        /// This property is always an absolute path. If you want to get a relative path, use <see cref="NormalizedPath.GetRelativeInputPath(IExecutionContext)"/>.
        /// </summary>
        /// <value>
        /// The source of the document, or <c>null</c> if the document doesn't have a source.
        /// </value>
        NormalizedPath Source { get; }

        /// <summary>
        /// The destination of the document. Can be either relative or absolute.
        /// </summary>
        NormalizedPath Destination { get; }

        /// <summary>
        /// The content provider responsible for creating content streams for the document.
        /// </summary>
        /// <remarks>
        /// This will always return a content provider, even if there is empty or no content.
        /// </remarks>
        IContentProvider ContentProvider { get; }

        /// <summary>
        /// Clones this document.
        /// </summary>
        /// <param name="source">The new source. If this document already contains a source, then it's used and this is ignored.</param>
        /// <param name="destination">The new destination or <c>null</c> to keep the existing destination.</param>
        /// <param name="items">New metadata items or <c>null</c> not to add any new metadata.</param>
        /// <param name="contentProvider">The new content provider or <c>null</c> to keep the existing content provider.</param>
        /// <returns>A new document of the same type as this document.</returns>
        IDocument Clone(
            NormalizedPath source,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null);

        /// <inheritdoc />
        IContentProvider IContentProviderFactory.GetContentProvider() => ContentProvider;

        /// <inheritdoc />
        IContentProvider IContentProviderFactory.GetContentProvider(string mediaType) => ContentProvider.CloneWithMediaType(mediaType);

        /// <inheritdoc />
        string IDisplayable.ToDisplayString() => Source.IsNull ? "unknown source" : Source.ToDisplayString();
    }
}