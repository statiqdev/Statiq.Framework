using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static class IExecutionContextGetDocumentsExtensions
    {
        /// <summary>
        /// Clones or creates documents from several types of values.
        /// </summary>
        /// <remarks>
        /// If the value is <c>null</c> the provided document will be returned.
        /// If the value is a <see cref="IDocument"/>, <see cref="IEnumerable{IDocument}"/>, or
        /// <see cref="IAsyncEnumerable{IDocument}"/>, the document(s) will be returned.
        /// If the value is a <see cref="IEnumerable{IModule}"/> or <see cref="IModule"/>,
        /// the module(s) will be executed with the input document(s) as their input
        /// and the results will be returned.
        /// If the value is an <see cref="IContentProvider"/>,
        /// <see cref="IContentProviderFactory"/>, or <see cref="Stream"/> it will be used to get new content for the
        /// provided document (or create a new document if the provided document is <c>null</c>).
        /// If config value is anything else, the content of the input document will be
        /// changed to (or a new document created with) the string value.
        /// </remarks>
        /// <param name="context">The execution context.</param>
        /// <param name="document">The document to clone (if appropriate).</param>
        /// <param name="value">The value to clone or create documents from.</param>
        /// <returns>The result documents.</returns>
        public static IAsyncEnumerable<IDocument> CloneOrCreateDocuments(this IExecutionContext context, IDocument document, object value) =>
            value == null
                ? document.YieldAsync()
                : GetDocuments(value)
                    ?? ExecuteModulesAsync(context, document, value)
                    ?? ChangeContentAsync(context, document, value);

        private static IAsyncEnumerable<IDocument> GetDocuments(object value)
        {
            if (value is IDocument document)
            {
                return document.YieldAsync();
            }
            if (value is IEnumerable<IDocument> enumerable)
            {
                return enumerable.ToAsyncEnumerable();
            }
            return value as IAsyncEnumerable<IDocument>;
        }

        private static IAsyncEnumerable<IDocument> ExecuteModulesAsync(IExecutionContext context, IDocument document, object value)
        {
            // Check for a single IModule first since some modules also implement IEnumerable<IModule>
            IEnumerable<IModule> modules = value is IModule module ? new[] { module } : value as IEnumerable<IModule>;
            return modules == null ? null : ExecuteModulesAsync(context, document, modules);
        }

        private static async IAsyncEnumerable<IDocument> ExecuteModulesAsync(IExecutionContext context, IDocument document, IEnumerable<IModule> modules)
        {
            foreach (IDocument result in await context.ExecuteModulesAsync(modules, document.Yield()))
            {
                yield return result;
            }
        }

        private static async IAsyncEnumerable<IDocument> ChangeContentAsync(IExecutionContext context, IDocument document, object value)
        {
            // Check if this is a known content provider type first
            IContentProvider contentProvider = await GetContentProviderAsync(context, value, false);
            if (contentProvider != null)
            {
                yield return context.CloneOrCreateDocument(document, contentProvider);
                yield break;
            }

            // It wasn't a known content provider type, so treat as an enumeration and convert the string value
            IAsyncEnumerable<IDocument> results = (value as IEnumerable ?? new[] { value })
                .Cast<object>()
                .ToAsyncEnumerable()
                .SelectAwait(async x => context.CloneOrCreateDocument(document, await GetContentProviderAsync(context, x, true)));
            await foreach (IDocument result in results.WithCancellation(context.CancellationToken))
            {
                yield return result;
            }
        }

        private static async Task<IContentProvider> GetContentProviderAsync(IExecutionContext context, object value, bool asString)
        {
            switch (value)
            {
                case IContentProvider contentProvider:
                    return contentProvider;
                case IContentProviderFactory factory:
                    return context.GetContentProvider(factory);
                case Stream stream:
                    return context.GetContentProvider(stream);
                case string str:
                    return await context.GetContentProviderAsync(str);
            }
            return asString && value != null ? await context.GetContentProviderAsync(value.ToString()) : null;
        }
    }
}
