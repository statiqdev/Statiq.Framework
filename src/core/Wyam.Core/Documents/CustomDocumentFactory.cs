using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Execution;

namespace Wyam.Core.Documents
{
    public class CustomDocumentFactory<T> : IDocumentFactory
        where T : CustomDocument, new()
    {
        private readonly IDocumentFactory _documentFactory;

        public CustomDocumentFactory(IDocumentFactory documentFactory)
        {
            _documentFactory = documentFactory;
        }

        public async Task<IDocument> GetDocumentAsync(
            IExecutionContext context,
            IDocument sourceDocument,
            FilePath source,
            IEnumerable<KeyValuePair<string, object>> metadata,
            object content)
        {
            CustomDocument customDocument = (CustomDocument)sourceDocument;
            IDocument document = await _documentFactory.GetDocumentAsync(context, customDocument?.Document, source, metadata, content);

            CustomDocument newCustomDocument = customDocument == null
                ? Activator.CreateInstance<T>()
                : customDocument.Clone();
            if (newCustomDocument == null || newCustomDocument == customDocument)
            {
                throw new Exception("Custom document type must return new instance from Clone method");
            }
            newCustomDocument.Document = document;
            return newCustomDocument;
        }
    }
}