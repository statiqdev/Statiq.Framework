using System;
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
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            : base(destination, items, contentProvider)
        {
        }

        public Document(
            NormalizedPath source,
            NormalizedPath destination,
            IContentProvider contentProvider = null)
            : base(source, destination, contentProvider)
        {
        }

        public Document(
            NormalizedPath destination,
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
            NormalizedPath source,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            : base(source, destination, items, contentProvider)
        {
        }

        public Document(
            IMetadata baseMetadata,
            NormalizedPath source,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            : base(baseMetadata, source, destination, items, contentProvider)
        {
        }

        public Document(
            IMetadata baseMetadata,
            NormalizedPath source,
            NormalizedPath destination,
            IMetadata metadata,
            IContentProvider contentProvider = null)
            : base(baseMetadata, source, destination, metadata, contentProvider)
        {
        }
    }
}
