using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Statiq.Common;

namespace Statiq.Common
{
    internal class DocumentFactory : IDocumentFactory
    {
        private readonly IMetadata _baseMetadata;

        private IFactory _defaultFactory = Factory<Document>.Instance;

        public DocumentFactory(IMetadata baseMetadata)
        {
            _baseMetadata = baseMetadata ?? throw new ArgumentNullException(nameof(baseMetadata));
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

        internal void SetDefaultDocumentType<TDocument>()
            where TDocument : FactoryDocument, IDocument, new() =>
            _defaultFactory = Factory<TDocument>.Instance;

        public IDocument CreateDocument(
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider) =>
            _defaultFactory.CreateDocument(
                _baseMetadata,
                source,
                destination,
                items,
                contentProvider);

        public TDocument CreateDocument<TDocument>(
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)Factory<TDocument>.Instance.CreateDocument(
                _baseMetadata,
                source,
                destination,
                items,
                contentProvider);
    }
}
