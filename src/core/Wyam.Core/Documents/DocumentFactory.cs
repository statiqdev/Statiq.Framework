using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Execution;
using Wyam.Core.Modules;
using Wyam.Core.Execution;
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
            IDocument sourceDocument,
            FilePath source,
            Stream stream,
            IEnumerable<KeyValuePair<string, object>> items = null,
            bool disposeStream = true)
        {
            if (sourceDocument == null || ModuleExtensions.AsNewDocumentModules.Contains(context.Module))
            {
                return new Document(_settings, source, stream, items, disposeStream);
            }
            return new Document((Document)sourceDocument, source, stream, items, disposeStream);
        }
    }
}
