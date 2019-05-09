using System;
using System.Collections.Generic;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Execution;
using Wyam.Common.Content;

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

        public IDocument GetDocument(
            IExecutionContext context,
            IDocument originalDocument,
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> metadata,
            IContentProvider contentProvider)
        {
            CustomDocument customDocument = (CustomDocument)originalDocument;
            IDocument document = _documentFactory.GetDocument(context, customDocument?.Document, source, destination, metadata, contentProvider);

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