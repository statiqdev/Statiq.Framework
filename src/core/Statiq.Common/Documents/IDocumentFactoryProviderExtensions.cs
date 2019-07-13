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
    }
}
