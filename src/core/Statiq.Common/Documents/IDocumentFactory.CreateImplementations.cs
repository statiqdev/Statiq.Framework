using System.Collections.Generic;

namespace Statiq.Common
{
    public partial interface IDocumentFactory
    {
        public IDocument CreateDocument(
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            CreateDocument(null, destination, items, contentProvider);

        public IDocument CreateDocument(
            FilePath source,
            FilePath destination,
            IContentProvider contentProvider = null) =>
            CreateDocument(source, destination, null, contentProvider);

        public IDocument CreateDocument(
            FilePath destination,
            IContentProvider contentProvider = null) =>
            CreateDocument(null, destination, null, contentProvider);

        public IDocument CreateDocument(
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            CreateDocument(null, null, items, contentProvider);

        public IDocument CreateDocument(
            IContentProvider contentProvider = null) =>
            CreateDocument(null, null, null, contentProvider);

        public TDocument CreateDocument<TDocument>(
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            CreateDocument<TDocument>(null, destination, items, contentProvider);

        public TDocument CreateDocument<TDocument>(
            FilePath source,
            FilePath destination,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            CreateDocument<TDocument>(source, destination, null, contentProvider);

        public TDocument CreateDocument<TDocument>(
            FilePath destination,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            CreateDocument<TDocument>(null, destination, null, contentProvider);

        public TDocument CreateDocument<TDocument>(
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            CreateDocument<TDocument>(null, null, items, contentProvider);

        public TDocument CreateDocument<TDocument>(
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            CreateDocument<TDocument>(null, null, null, contentProvider);

        public IDocument CloneOrCreateDocument(
            IDocument document,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            document?.Clone(destination, items, contentProvider)
                ?? CreateDocument(destination, items, contentProvider);

        public IDocument CloneOrCreateDocument(
            IDocument document,
            FilePath source,
            FilePath destination,
            IContentProvider contentProvider = null) =>
            document?.Clone(source, destination, contentProvider)
                ?? CreateDocument(source, destination, contentProvider);

        public IDocument CloneOrCreateDocument(
            IDocument document,
            FilePath destination,
            IContentProvider contentProvider = null) =>
            document?.Clone(destination, contentProvider)
                ?? CreateDocument(destination, contentProvider);

        public IDocument CloneOrCreateDocument(
            IDocument document,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            document?.Clone(items, contentProvider)
                ?? CreateDocument(items, contentProvider);

        public IDocument CloneOrCreateDocument(
            IDocument document,
            IContentProvider contentProvider = null) =>
            document?.Clone(contentProvider)
                ?? CreateDocument(contentProvider);

        public IDocument CloneOrCreateDocument(
            IDocument document,
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            document?.Clone(source, destination, items, contentProvider)
                ?? CreateDocument(source, destination, items, contentProvider);

        public TDocument CloneOrCreateDocument<TDocument>(
            TDocument document,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(destination, items, contentProvider)
                ?? CreateDocument<TDocument>(destination, items, contentProvider);

        public TDocument CloneOrCreateDocument<TDocument>(
            TDocument document,
            FilePath source,
            FilePath destination,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(source, destination, contentProvider)
                ?? CreateDocument<TDocument>(source, destination, contentProvider);

        public TDocument CloneOrCreateDocument<TDocument>(
            TDocument document,
            FilePath destination,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(destination, contentProvider)
                ?? CreateDocument<TDocument>(destination, contentProvider);

        public TDocument CloneOrCreateDocument<TDocument>(
            TDocument document,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(items, contentProvider)
                ?? CreateDocument<TDocument>(items, contentProvider);

        public TDocument CloneOrCreateDocument<TDocument>(
            TDocument document,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(contentProvider)
                ?? CreateDocument<TDocument>(contentProvider);

        public TDocument CloneOrCreateDocument<TDocument>(
            TDocument document,
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(source, destination, items, contentProvider)
                ?? CreateDocument<TDocument>(source, destination, items, contentProvider);
    }
}
