using System;
using System.Collections.Generic;
using System.IO;
using Statiq.Common.Configuration;
using Statiq.Common.Content;
using Statiq.Common.Execution;
using Statiq.Common.IO;
using Statiq.Common.Meta;

namespace Statiq.Common.Documents
{
    public class DocumentFactory : IDocumentFactoryProvider
    {
        private readonly IReadOnlySettings _settings;

        // This lets the IDocumentFactoryProvider extensions work directly on the document factory
        DocumentFactory IDocumentFactoryProvider.DocumentFactory => this;

        private IFactory _defaultFactory = Factory<Document>.Instance;

        public DocumentFactory(IReadOnlySettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        private interface IFactory
        {
            IDocument CreateDocument(
                IMetadata baseMetadata,
                FilePath source,
                FilePath destination,
                IEnumerable<KeyValuePair<string, object>> items,
                IContentProvider contentProvider);
        }

        private class Factory<TDocument> : IFactory
             where TDocument : FactoryDocument, IDocument, new()
        {
            // Use a singleton pattern to avoid repeated allocations of factories for the same document type
            public static readonly Factory<TDocument> Instance = new Factory<TDocument>();

            private Factory()
            {
            }

            public IDocument CreateDocument(
                IMetadata baseMetadata,
                FilePath source,
                FilePath destination,
                IEnumerable<KeyValuePair<string, object>> items,
                IContentProvider contentProvider) =>
                new TDocument().Initialize(baseMetadata, source, destination, new Metadata(items), contentProvider);
        }

        internal void InternalSetDefaultDocumentType<TDocument>()
            where TDocument : FactoryDocument, IDocument, new() =>
            _defaultFactory = Factory<TDocument>.Instance;

        internal IDocument InternalCreateDocument(
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider) =>
            _defaultFactory.CreateDocument(
                _settings,
                source,
                destination,
                items,
                contentProvider);

        internal TDocument InternalCreateDocument<TDocument>(
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)Factory<TDocument>.Instance.CreateDocument(
                _settings,
                source,
                destination,
                items,
                contentProvider);
    }
}
