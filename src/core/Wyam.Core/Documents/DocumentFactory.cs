using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Wyam.Common.Content;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Execution;
using Wyam.Core.Content;
using Wyam.Core.Modules;
using Wyam.Core.Meta;

namespace Wyam.Core.Documents
{
    internal class DocumentFactory : IDocumentFactory
    {
        private readonly MetadataDictionary _settings;

        public DocumentFactory(MetadataDictionary settings)
        {
            _settings = settings;
        }

        public IDocument GetDocument(
            IExecutionContext context,
            IDocument originalDocument,
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> metadata,
            IContentProvider contentProvider)
        {
            if (originalDocument == null || ModuleExtensions.AsNewDocumentModules.Contains(context.Module))
            {
                return new Document(_settings, source, destination, contentProvider, metadata);
            }
            return new Document((Document)originalDocument, source, destination, contentProvider, metadata);
        }
    }
}
