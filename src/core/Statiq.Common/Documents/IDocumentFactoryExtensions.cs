using System.Collections.Generic;

namespace Statiq.Common
{
    public static class IDocumentFactoryExtensions
    {
        public static IDocument CreateDocument(
            this IDocumentFactory factory,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            factory.CreateDocument(null, destination, items, contentProvider);

        public static IDocument CreateDocument(
            this IDocumentFactory factory,
            FilePath source,
            FilePath destination,
            IContentProvider contentProvider = null) =>
            factory.CreateDocument(source, destination, null, contentProvider);

        public static IDocument CreateDocument(
            this IDocumentFactory factory,
            FilePath destination,
            IContentProvider contentProvider = null) =>
            factory.CreateDocument(null, destination, null, contentProvider);

        public static IDocument CreateDocument(
            this IDocumentFactory factory,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            factory.CreateDocument(null, null, items, contentProvider);

        public static IDocument CreateDocument(
            this IDocumentFactory factory,
            IContentProvider contentProvider = null) =>
            factory.CreateDocument(null, null, null, contentProvider);

        public static TDocument CreateDocument<TDocument>(
            this IDocumentFactory factory,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            factory.CreateDocument<TDocument>(null, destination, items, contentProvider);

        public static TDocument CreateDocument<TDocument>(
            this IDocumentFactory factory,
            FilePath source,
            FilePath destination,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            factory.CreateDocument<TDocument>(source, destination, null, contentProvider);

        public static TDocument CreateDocument<TDocument>(
            this IDocumentFactory factory,
            FilePath destination,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            factory.CreateDocument<TDocument>(null, destination, null, contentProvider);

        public static TDocument CreateDocument<TDocument>(
            this IDocumentFactory factory,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            factory.CreateDocument<TDocument>(null, null, items, contentProvider);

        public static TDocument CreateDocument<TDocument>(
            this IDocumentFactory factory,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            factory.CreateDocument<TDocument>(null, null, null, contentProvider);

        public static IDocument CloneOrCreateDocument(
            this IDocumentFactory factory,
            IDocument document,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            document?.Clone(destination, items, contentProvider)
                ?? factory.CreateDocument(destination, items, contentProvider);

        public static IDocument CloneOrCreateDocument(
            this IDocumentFactory factory,
            IDocument document,
            FilePath source,
            FilePath destination,
            IContentProvider contentProvider = null) =>
            document?.Clone(source, destination, contentProvider)
                ?? factory.CreateDocument(source, destination, contentProvider);

        public static IDocument CloneOrCreateDocument(
            this IDocumentFactory factory,
            IDocument document,
            FilePath destination,
            IContentProvider contentProvider = null) =>
            document?.Clone(destination, contentProvider)
                ?? factory.CreateDocument(destination, contentProvider);

        public static IDocument CloneOrCreateDocument(
            this IDocumentFactory factory,
            IDocument document,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            document?.Clone(items, contentProvider)
                ?? factory.CreateDocument(items, contentProvider);

        public static IDocument CloneOrCreateDocument(
            this IDocumentFactory factory,
            IDocument document,
            IContentProvider contentProvider = null) =>
            document?.Clone(contentProvider) ?? factory.CreateDocument(contentProvider);

        public static IDocument CloneOrCreateDocument(
            this IDocumentFactory factory,
            IDocument document,
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            document?.Clone(source, destination, items, contentProvider)
                ?? factory.CreateDocument(source, destination, items, contentProvider);

        public static TDocument CloneOrCreateDocument<TDocument>(
            this IDocumentFactory factory,
            TDocument document,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(destination, items, contentProvider)
                ?? factory.CreateDocument<TDocument>(destination, items, contentProvider);

        public static TDocument CloneOrCreateDocument<TDocument>(
            this IDocumentFactory factory,
            TDocument document,
            FilePath source,
            FilePath destination,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(source, destination, contentProvider)
                ?? factory.CreateDocument<TDocument>(source, destination, contentProvider);

        public static TDocument CloneOrCreateDocument<TDocument>(
            this IDocumentFactory factory,
            TDocument document,
            FilePath destination,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(destination, contentProvider)
                ?? factory.CreateDocument<TDocument>(destination, contentProvider);

        public static TDocument CloneOrCreateDocument<TDocument>(
            this IDocumentFactory factory,
            TDocument document,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(items, contentProvider)
                ?? factory.CreateDocument<TDocument>(items, contentProvider);

        public static TDocument CloneOrCreateDocument<TDocument>(
            this IDocumentFactory factory,
            TDocument document,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(contentProvider) ?? factory.CreateDocument<TDocument>(contentProvider);

        public static TDocument CloneOrCreateDocument<TDocument>(
            this IDocumentFactory factory,
            TDocument document,
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(source, destination, items, contentProvider)
                ?? factory.CreateDocument<TDocument>(source, destination, items, contentProvider);
    }
}
