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
    public static class IDocumentFactoryProviderExtensions
    {
        public static void SetDefaultDocumentType<TDocument>(this IDocumentFactoryProvider provider)
            where TDocument : FactoryDocument, IDocument, new() =>
            provider.DocumentFactory.InternalSetDefaultDocumentType<TDocument>();

        public static IDocument GetDocument(
            this IDocumentFactoryProvider provider,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            provider.DocumentFactory.InternalGetDocument(null, destination, items, contentProvider);

        public static IDocument GetDocument(
            this IDocumentFactoryProvider provider,
            FilePath source,
            FilePath destination,
            IContentProvider contentProvider = null) =>
            provider.DocumentFactory.InternalGetDocument(source, destination, null, contentProvider);

        public static IDocument GetDocument(
            this IDocumentFactoryProvider provider,
            FilePath destination,
            IContentProvider contentProvider = null) =>
            provider.DocumentFactory.InternalGetDocument(null, destination, null, contentProvider);

        public static IDocument GetDocument(
            this IDocumentFactoryProvider provider,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            provider.DocumentFactory.InternalGetDocument(null, null, items, contentProvider);

        public static IDocument GetDocument(
            this IDocumentFactoryProvider provider,
            IContentProvider contentProvider = null) =>
            provider.DocumentFactory.InternalGetDocument(null, null, null, contentProvider);

        public static IDocument GetDocument(
            this IDocumentFactoryProvider provider,
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            provider.DocumentFactory.InternalGetDocument(source, destination, items, contentProvider);

        public static TDocument GetDocument<TDocument>(
            this IDocumentFactoryProvider provider,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            provider.DocumentFactory.InternalGetDocument<TDocument>(null, destination, items, contentProvider);

        public static TDocument GetDocument<TDocument>(
            this IDocumentFactoryProvider provider,
            FilePath source,
            FilePath destination,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            provider.DocumentFactory.InternalGetDocument<TDocument>(source, destination, null, contentProvider);

        public static TDocument GetDocument<TDocument>(
            this IDocumentFactoryProvider provider,
            FilePath destination,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            provider.DocumentFactory.InternalGetDocument<TDocument>(null, destination, null, contentProvider);

        public static TDocument GetDocument<TDocument>(
            this IDocumentFactoryProvider provider,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            provider.DocumentFactory.InternalGetDocument<TDocument>(null, null, items, contentProvider);

        public static TDocument GetDocument<TDocument>(
            this IDocumentFactoryProvider provider,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            provider.DocumentFactory.InternalGetDocument<TDocument>(null, null, null, contentProvider);

        public static TDocument GetDocument<TDocument>(
            this IDocumentFactoryProvider provider,
            FilePath source,
            FilePath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            provider.DocumentFactory.InternalGetDocument<TDocument>(source, destination, items, contentProvider);
    }
}
