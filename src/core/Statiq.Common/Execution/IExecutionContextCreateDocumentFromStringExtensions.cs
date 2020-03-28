using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// Extensions to <see cref="IExecutionContext"/> that create a document from string content sources.
    /// </summary>
    public static class IExecutionContextCreateDocumentFromStringExtensions
    {
        public static async Task<IDocument> CreateDocumentAsync(
            this IExecutionContext context,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            string content,
            string mediaType = null) =>
            context.CreateDocument(null, destination, items, await context.GetContentProviderAsync(content, mediaType));

        public static async Task<IDocument> CreateDocumentAsync(
            this IExecutionContext context,
            NormalizedPath source,
            NormalizedPath destination,
            string content,
            string mediaType = null) =>
            context.CreateDocument(source, destination, null, await context.GetContentProviderAsync(content, mediaType));

        public static async Task<IDocument> CreateDocumentAsync(
            this IExecutionContext context,
            NormalizedPath destination,
            string content,
            string mediaType = null) =>
            context.CreateDocument(null, destination, null, await context.GetContentProviderAsync(content, mediaType));

        public static async Task<IDocument> CreateDocumentAsync(
            this IExecutionContext context,
            IEnumerable<KeyValuePair<string, object>> items,
            string content,
            string mediaType = null) =>
            context.CreateDocument(null, null, items, await context.GetContentProviderAsync(content, mediaType));

        public static async Task<IDocument> CreateDocumentAsync(
            this IExecutionContext context,
            string content,
            string mediaType = null) =>
            context.CreateDocument(null, null, null, await context.GetContentProviderAsync(content, mediaType));

        public static async Task<TDocument> CreateDocumentAsync<TDocument>(
            this IExecutionContext context,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            string content,
            string mediaType = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            context.CreateDocument<TDocument>(null, destination, items, await context.GetContentProviderAsync(content, mediaType));

        public static async Task<TDocument> CreateDocumentAsync<TDocument>(
            this IExecutionContext context,
            NormalizedPath source,
            NormalizedPath destination,
            string content,
            string mediaType = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            context.CreateDocument<TDocument>(source, destination, null, await context.GetContentProviderAsync(content, mediaType));

        public static async Task<TDocument> CreateDocumentAsync<TDocument>(
            this IExecutionContext context,
            NormalizedPath destination,
            string content,
            string mediaType = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            context.CreateDocument<TDocument>(null, destination, null, await context.GetContentProviderAsync(content, mediaType));

        public static async Task<TDocument> CreateDocumentAsync<TDocument>(
            this IExecutionContext context,
            IEnumerable<KeyValuePair<string, object>> items,
            string content,
            string mediaType = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            context.CreateDocument<TDocument>(null, null, items, await context.GetContentProviderAsync(content, mediaType));

        public static async Task<TDocument> CreateDocumentAsync<TDocument>(
            this IExecutionContext context,
            string content,
            string mediaType = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            context.CreateDocument<TDocument>(null, null, null, await context.GetContentProviderAsync(content, mediaType));

        public static async Task<IDocument> CloneOrCreateDocumentAsync(
            this IExecutionContext context,
            IDocument document,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            string content,
            string mediaType = null) =>
            document?.Clone(destination, items, await context.GetContentProviderAsync(content, mediaType))
                ?? context.CreateDocument(destination, items, await context.GetContentProviderAsync(content, mediaType));

        public static async Task<IDocument> CloneOrCreateDocumentAsync(
            this IExecutionContext context,
            IDocument document,
            NormalizedPath source,
            NormalizedPath destination,
            string content,
            string mediaType = null) =>
            document?.Clone(source, destination, await context.GetContentProviderAsync(content, mediaType))
                ?? context.CreateDocument(source, destination, await context.GetContentProviderAsync(content, mediaType));

        public static async Task<IDocument> CloneOrCreateDocumentAsync(
            this IExecutionContext context,
            IDocument document,
            NormalizedPath destination,
            string content,
            string mediaType = null) =>
            document?.Clone(destination, await context.GetContentProviderAsync(content, mediaType))
                ?? context.CreateDocument(destination, await context.GetContentProviderAsync(content, mediaType));

        public static async Task<IDocument> CloneOrCreateDocumentAsync(
            this IExecutionContext context,
            IDocument document,
            IEnumerable<KeyValuePair<string, object>> items,
            string content,
            string mediaType = null) =>
            document?.Clone(items, await context.GetContentProviderAsync(content, mediaType))
                ?? context.CreateDocument(items, await context.GetContentProviderAsync(content, mediaType));

        public static async Task<IDocument> CloneOrCreateDocumentAsync(
            this IExecutionContext context,
            IDocument document,
            string content,
            string mediaType = null) =>
            document?.Clone(await context.GetContentProviderAsync(content, mediaType))
                ?? context.CreateDocument(await context.GetContentProviderAsync(content, mediaType));

        public static async Task<IDocument> CloneOrCreateDocumentAsync(
            this IExecutionContext context,
            IDocument document,
            NormalizedPath source,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            string content,
            string mediaType = null) =>
            document?.Clone(source, destination, items, await context.GetContentProviderAsync(content, mediaType))
                ?? context.CreateDocument(source, destination, items, await context.GetContentProviderAsync(content, mediaType));

        public static async Task<TDocument> CloneOrCreateDocumentAsync<TDocument>(
            this IExecutionContext context,
            TDocument document,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            string content,
            string mediaType = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(destination, items, await context.GetContentProviderAsync(content, mediaType))
                ?? context.CreateDocument<TDocument>(destination, items, await context.GetContentProviderAsync(content, mediaType));

        public static async Task<TDocument> CloneOrCreateDocumentAsync<TDocument>(
            this IExecutionContext context,
            TDocument document,
            NormalizedPath source,
            NormalizedPath destination,
            string content,
            string mediaType = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(source, destination, await context.GetContentProviderAsync(content, mediaType))
                ?? context.CreateDocument<TDocument>(source, destination, await context.GetContentProviderAsync(content, mediaType));

        public static async Task<TDocument> CloneOrCreateDocumentAsync<TDocument>(
            this IExecutionContext context,
            TDocument document,
            NormalizedPath destination,
            string content,
            string mediaType = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(destination, await context.GetContentProviderAsync(content, mediaType))
                ?? context.CreateDocument<TDocument>(destination, await context.GetContentProviderAsync(content, mediaType));

        public static async Task<TDocument> CloneOrCreateDocumentAsync<TDocument>(
            this IExecutionContext context,
            TDocument document,
            IEnumerable<KeyValuePair<string, object>> items,
            string content,
            string mediaType = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(items, await context.GetContentProviderAsync(content, mediaType))
                ?? context.CreateDocument<TDocument>(items, await context.GetContentProviderAsync(content, mediaType));

        public static async Task<TDocument> CloneOrCreateDocumentAsync<TDocument>(
            this IExecutionContext context,
            TDocument document,
            string content,
            string mediaType = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(await context.GetContentProviderAsync(content, mediaType))
                ?? context.CreateDocument<TDocument>(await context.GetContentProviderAsync(content, mediaType));

        public static async Task<TDocument> CloneOrCreateDocumentAsync<TDocument>(
            this IExecutionContext context,
            TDocument document,
            NormalizedPath source,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            string content,
            string mediaType = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(source, destination, items, await context.GetContentProviderAsync(content, mediaType))
                ?? context.CreateDocument<TDocument>(source, destination, items, await context.GetContentProviderAsync(content, mediaType));
    }
}
