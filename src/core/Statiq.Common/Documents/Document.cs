using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Statiq.Common.Configuration;
using Statiq.Common.Content;
using Statiq.Common.Execution;
using Statiq.Common.IO;
using Statiq.Common.Meta;
using Statiq.Common.Util;

namespace Statiq.Common.Documents
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
        public Document(
            IEngine engine,
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            : base(engine, source, destination, items, contentProvider)
        {
        }

        private Document(
            Document sourceDocument,
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            : base(sourceDocument, source, destination, items, contentProvider)
        {
        }

        public override Document Clone(
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            new Document(this, source, destination, items, contentProvider);
    }
}
