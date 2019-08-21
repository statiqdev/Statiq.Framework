using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static class IExecutionContextCloneOrCreateDocumentsExtensions
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
        /// <param name="moduleInputs">The inputs to use when executing if <paramref name="value"/> contains modules.</param>
        /// <param name="value">The value to clone or create documents from.</param>
        /// <returns>The result documents.</returns>
        public static async Task<IEnumerable<IDocument>> CloneOrCreateDocumentsAsync(
            this IExecutionContext context,
            IDocument document,
            IEnumerable<IDocument> moduleInputs,
            object value) =>
            value == null
                ? document.Yield()
                : GetDocuments(value)
                    ?? await ExecuteModulesAsync(context, moduleInputs, value)
                        ?? await ChangeContentAsync(context, document, value);

        private static IEnumerable<IDocument> GetDocuments(object value) =>
            value is IDocument document ? document.Yield() : value as IEnumerable<IDocument>;

        private static async Task<IEnumerable<IDocument>> ExecuteModulesAsync(IExecutionContext context, IEnumerable<IDocument> moduleInputs, object value)
        {
            // Check for a single IModule first since some modules also implement IEnumerable<IModule>
            IEnumerable<IModule> modules = value is IModule module ? new[] { module } : value as IEnumerable<IModule>;
            return modules == null ? null : (IEnumerable<IDocument>)await context.ExecuteModulesAsync(modules, moduleInputs);
        }

        private static async Task<IEnumerable<IDocument>> ChangeContentAsync(IExecutionContext context, IDocument document, object value)
        {
            // Check if this is a known content provider type first
            IContentProvider contentProvider = await GetContentProviderAsync(context, value, false);
            if (contentProvider != null)
            {
                return context.CloneOrCreateDocument(document, contentProvider).Yield();
            }

            // It wasn't a known content provider type, so treat as an enumeration and convert the string value
            object[] valueObjects = (value as IEnumerable ?? new[] { value }).Cast<object>().ToArray();
            IDocument[] results = new IDocument[valueObjects.Length];
            for (int c = 0; c < valueObjects.Length; c++)
            {
                results[c] = context.CloneOrCreateDocument(document, await GetContentProviderAsync(context, valueObjects[c], true));
            }
            return results;
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
