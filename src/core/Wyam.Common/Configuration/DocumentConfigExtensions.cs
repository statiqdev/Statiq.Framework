using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Common.Configuration
{
    public static class DocumentConfigExtensions
    {
        public static Task<T> GetValueAsync<T>(
            this DocumentConfig<T> config,
            IDocument document,
            IExecutionContext context,
            Func<T, T> transform = null) =>
            config?.GetAndCacheValueAsync(document, context, transform) ?? Task.FromResult(default(T));

        public static async Task<T> GetValueAsync<T>(
            this DocumentConfig<object> config,
            IDocument document,
            IExecutionContext context,
            string errorDetails = null)
        {
            if (config == null)
            {
                return default;
            }

            object value = await config.GetAndCacheValueAsync(document, context);
            if (!context.TryConvert(value, out T result))
            {
                throw new InvalidOperationException(
                    $"Could not convert from type {value?.GetType().Name ?? "null"} to type {typeof(T).Name}{Config.GetErrorDetails(errorDetails)}");
            }
            return result;
        }

        public static async Task<T> TryGetValueAsync<T>(
            this DocumentConfig<object> config,
            IDocument document,
            IExecutionContext context)
        {
            if (config == null)
            {
                return default;
            }

            object value = await config.GetAndCacheValueAsync(document, context);
            return context.TryConvert(value, out T result) ? result : default;
        }

        public static DocumentConfig<bool> CombineWith(this DocumentConfig<bool> first, DocumentConfig<bool> second)
        {
            if (first == null)
            {
                return second;
            }
            if (second == null)
            {
                return first;
            }
            return new DocumentConfig<bool>(async (doc, ctx) => await first.GetValueAsync(doc, ctx) && await second.GetValueAsync(doc, ctx));
        }

        public static Task<IEnumerable<IDocument>> FilterAsync(this IEnumerable<IDocument> documents, DocumentConfig<bool> predicate, IExecutionContext context) =>
            predicate == null ? Task.FromResult(documents) : documents.WhereAsync(context, x => predicate.GetValueAsync(x, context));
    }
}
