using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// A simple document that has content and metadata.
    /// </summary>
    /// <remarks>
    /// To create your own document types, derive from
    /// <see cref="Document{TDocument}"/>.
    /// </remarks>
    public sealed class Document : Document<Document>
    {
        public Document()
        {
        }

        public Document(
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            : base(destination, items, contentProvider)
        {
        }

        public Document(
            FilePath source,
            FilePath destination,
            IContentProvider contentProvider = null)
            : base(source, destination, contentProvider)
        {
        }

        public Document(
            FilePath destination,
            IContentProvider contentProvider = null)
            : base(destination, contentProvider)
        {
        }

        public Document(
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            : base(items, contentProvider)
        {
        }

        public Document(IContentProvider contentProvider)
            : base(contentProvider)
        {
        }

        public Document(
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            : base(source, destination, items, contentProvider)
        {
        }

        public Document(
            IMetadata baseMetadata,
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            : base(baseMetadata, source, destination, items, contentProvider)
        {
        }

        public Document(
            IMetadata baseMetadata,
            FilePath source,
            FilePath destination,
            IMetadata metadata,
            IContentProvider contentProvider = null)
            : base(baseMetadata, source, destination, metadata, contentProvider)
        {
        }
    }
}
