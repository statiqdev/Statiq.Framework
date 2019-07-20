using System.Collections.Generic;

namespace Statiq.Common
{
    public static class IDocumentFactoryProviderExtensions
    {
        public static void SetDefaultDocumentType<TDocument>(this IDocumentFactoryProvider provider)
            where TDocument : FactoryDocument, IDocument, new() =>
            provider.DocumentFactory.InternalSetDefaultDocumentType<TDocument>();

        public static IDocument CreateDocument(
            this IDocumentFactoryProvider provider,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            provider.DocumentFactory.InternalCreateDocument(null, destination, items, contentProvider);

        public static IDocument CreateDocument(
            this IDocumentFactoryProvider provider,
            FilePath source,
            FilePath destination,
            IContentProvider contentProvider = null) =>
            provider.DocumentFactory.InternalCreateDocument(source, destination, null, contentProvider);

        public static IDocument CreateDocument(
            this IDocumentFactoryProvider provider,
            FilePath destination,
            IContentProvider contentProvider = null) =>
            provider.DocumentFactory.InternalCreateDocument(null, destination, null, contentProvider);

        public static IDocument CreateDocument(
            this IDocumentFactoryProvider provider,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            provider.DocumentFactory.InternalCreateDocument(null, null, items, contentProvider);

        public static IDocument CreateDocument(
            this IDocumentFactoryProvider provider,
            IContentProvider contentProvider = null) =>
            provider.DocumentFactory.InternalCreateDocument(null, null, null, contentProvider);

        public static IDocument CreateDocument(
            this IDocumentFactoryProvider provider,
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            provider.DocumentFactory.InternalCreateDocument(source, destination, items, contentProvider);

        public static TDocument CreateDocument<TDocument>(
            this IDocumentFactoryProvider provider,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            provider.DocumentFactory.InternalCreateDocument<TDocument>(null, destination, items, contentProvider);

        public static TDocument CreateDocument<TDocument>(
            this IDocumentFactoryProvider provider,
            FilePath source,
            FilePath destination,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            provider.DocumentFactory.InternalCreateDocument<TDocument>(source, destination, null, contentProvider);

        public static TDocument CreateDocument<TDocument>(
            this IDocumentFactoryProvider provider,
            FilePath destination,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            provider.DocumentFactory.InternalCreateDocument<TDocument>(null, destination, null, contentProvider);

        public static TDocument CreateDocument<TDocument>(
            this IDocumentFactoryProvider provider,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            provider.DocumentFactory.InternalCreateDocument<TDocument>(null, null, items, contentProvider);

        public static TDocument CreateDocument<TDocument>(
            this IDocumentFactoryProvider provider,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            provider.DocumentFactory.InternalCreateDocument<TDocument>(null, null, null, contentProvider);

        public static TDocument CreateDocument<TDocument>(
            this IDocumentFactoryProvider provider,
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            provider.DocumentFactory.InternalCreateDocument<TDocument>(source, destination, items, contentProvider);

        public static IDocument CloneOrCreateDocument(
            this IDocumentFactoryProvider provider,
            IDocument document,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            document?.Clone(destination, items, contentProvider)
                ?? provider.CreateDocument(destination, items, contentProvider);

        public static IDocument CloneOrCreateDocument(
            this IDocumentFactoryProvider provider,
            IDocument document,
            FilePath source,
            FilePath destination,
            IContentProvider contentProvider = null) =>
            document?.Clone(source, destination, contentProvider)
                ?? provider.CreateDocument(source, destination, contentProvider);

        public static IDocument CloneOrCreateDocument(
            this IDocumentFactoryProvider provider,
            IDocument document,
            FilePath destination,
            IContentProvider contentProvider = null) =>
            document?.Clone(destination, contentProvider)
                ?? provider.CreateDocument(destination, contentProvider);

        public static IDocument CloneOrCreateDocument(
            this IDocumentFactoryProvider provider,
            IDocument document,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            document?.Clone(items, contentProvider)
                ?? provider.CreateDocument(items, contentProvider);

        public static IDocument CloneOrCreateDocument(
            this IDocumentFactoryProvider provider,
            IDocument document,
            IContentProvider contentProvider = null) =>
            document?.Clone(contentProvider) ?? provider.CreateDocument(contentProvider);

        public static IDocument CloneOrCreateDocument(
            this IDocumentFactoryProvider provider,
            IDocument document,
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            document?.Clone(source, destination, items, contentProvider)
                ?? provider.CreateDocument(source, destination, items, contentProvider);

        public static TDocument CloneOrCreateDocument<TDocument>(
            this IDocumentFactoryProvider provider,
            TDocument document,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(destination, items, contentProvider)
                ?? provider.CreateDocument<TDocument>(destination, items, contentProvider);

        public static TDocument CloneOrCreateDocument<TDocument>(
            this IDocumentFactoryProvider provider,
            TDocument document,
            FilePath source,
            FilePath destination,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(source, destination, contentProvider)
                ?? provider.CreateDocument<TDocument>(source, destination, contentProvider);

        public static TDocument CloneOrCreateDocument<TDocument>(
            this IDocumentFactoryProvider provider,
            TDocument document,
            FilePath destination,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(destination, contentProvider)
                ?? provider.CreateDocument<TDocument>(destination, contentProvider);

        public static TDocument CloneOrCreateDocument<TDocument>(
            this IDocumentFactoryProvider provider,
            TDocument document,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(items, contentProvider)
                ?? provider.CreateDocument<TDocument>(items, contentProvider);

        public static TDocument CloneOrCreateDocument<TDocument>(
            this IDocumentFactoryProvider provider,
            TDocument document,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(contentProvider) ?? provider.CreateDocument<TDocument>(contentProvider);

        public static TDocument CloneOrCreateDocument<TDocument>(
            this IDocumentFactoryProvider provider,
            TDocument document,
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(source, destination, items, contentProvider)
                ?? provider.CreateDocument<TDocument>(source, destination, items, contentProvider);
    }
}
