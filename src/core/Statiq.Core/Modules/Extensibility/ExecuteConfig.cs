using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Gets documents from an arbitrary config value.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This module is very useful for customizing pipeline execution without having to write an entire module.
    /// Returning modules from the config value is also useful for customizing existing modules based on the
    /// current set of documents.
    /// </para>
    /// <para>
    /// If the config value is a <see cref="IEnumerable{IDocument}"/> or <see cref="IDocument"/>, the document(s) will be the
    /// output(s) of this module. If the config value is a <see cref="IEnumerable{IModule}"/> or
    /// <see cref="IModule"/>, the module(s) will be executed with the input document(s) as their input
    /// and the results will be the output of this module. If the config value is <c>null</c>,
    /// this module will output the original input document(s). If the config value is an <see cref="IContentProvider"/>,
    /// <see cref="IContentProviderFactory"/>, or <see cref="Stream"/> it will be used to get new content for the
    /// input document (or new documents). If the config value is anything else, the content of the input document will be
    /// changed to (or new documents created with) it's string value.
    /// </para>
    /// <para>
    /// If the provided config value does not require a document (for example, it's created with the
    /// <see cref="Config.FromContext(string, object)"/> factory method) then it will be invoked once for all input documents.
    /// If the provided config value does require a document (for example, it's created with the
    /// <see cref="Config.FromDocument(string, object)"/> factory method) then if will be invoked once for each input document.
    /// </para>
    /// </remarks>
    /// <category>Extensibility</category>
    public class ExecuteConfig : ConfigModule<object>
    {
        public ExecuteConfig(Config<object> config)
            : base(config, false)
        {
        }

        protected override async Task<IEnumerable<IDocument>> ExecuteAsync(
            IDocument input,
            IReadOnlyList<IDocument> inputs,
            IExecutionContext context,
            object value)
        {
            if (input != null)
            {
                inputs = new[] { input };
            }

            // This behavior is important because action-based config values always return null, so by returning
            // the input(s) when the value is null it ensures actions will execute without affecting the input documents
            if (value == null)
            {
                return inputs;
            }

            return GetValueDocuments(value)
                ?? await ExecuteModulesAsync(value, context, inputs)
                ?? await ChangeContentAsync(value, context, input);
        }

        private static IEnumerable<IDocument> GetValueDocuments(object value) =>
            value is IDocument document ? document.Yield() : value as IEnumerable<IDocument>;

        private static async Task<IEnumerable<IDocument>> ExecuteModulesAsync(object value, IExecutionContext context, IEnumerable<IDocument> inputs)
        {
            IEnumerable<IModule> modules = value is IModule module ? new[] { module } : value as IEnumerable<IModule>;
            return modules != null ? await context.ExecuteAsync(modules, inputs) : (IEnumerable<IDocument>)null;
        }

        private static async Task<IEnumerable<IDocument>> ChangeContentAsync(object value, IExecutionContext context, IDocument document)
        {
            // Check if this is a known content provider type first
            IContentProvider contentProvider = await GetContentProviderAsync(value, context, false);
            if (contentProvider != null)
            {
                return context.CloneOrCreateDocument(document, contentProvider).Yield();
            }

            // It wasn't a known content provider type, so treat as an enumeration and convert the string value
            return await (value as IEnumerable ?? new[] { value })
                .Cast<object>()
                .SelectAsync(async x => context.CloneOrCreateDocument(document, await GetContentProviderAsync(x, context, true)));
        }

        private static async Task<IContentProvider> GetContentProviderAsync(object value, IExecutionContext context, bool stringValue)
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
            return stringValue && value != null ? await context.GetContentProviderAsync(value.ToString()) : null;
        }
    }
}