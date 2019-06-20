using System.Collections.Concurrent;
using System.Collections.Generic;
using Statiq.Common;
using Statiq.Common.Content;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.IO;
using Statiq.Common.Tracing;
using Statiq.Core.Meta;
using Statiq.Core.Modules;

namespace Statiq.Core.Documents
{
    internal class DocumentFactory : IDocumentFactory
    {
        private readonly IEngine _engine;

        public DocumentFactory(IEngine engine)
        {
            _engine = engine;
        }

        public IDocument GetDocument(
            IExecutionContext context,
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider) =>
            new Document(_engine, source, destination, items, contentProvider);
    }
}
