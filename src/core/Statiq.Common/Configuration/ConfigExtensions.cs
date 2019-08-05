using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static class ConfigExtensions
    {
        public static Task<TValue> GetValueAsync<TValue>(
            this Config<TValue> config,
            IDocument document,
            IExecutionContext context,
            Func<TValue, TValue> transform = null) =>
            config?.GetAndTransformValueAsync(document, context, transform) ?? Task.FromResult(default(TValue));

        public static async Task<TValue> GetValueAsync<TValue>(
            this Config<object> config,
            IDocument document,
            IExecutionContext context,
            string errorDetails = null)
        {
            if (config == null)
            {
                return default;
            }

            object value = await config.GetAndTransformValueAsync(document, context);
            if (!TypeHelper.TryConvert(value, out TValue result))
            {
                throw new InvalidOperationException(
                    $"Could not convert from type {value?.GetType().Name ?? "null"} to type {typeof(TValue).Name}{Config.GetErrorDetails(errorDetails)}");
            }
            return result;
        }

        public static async Task<TValue> TryGetValueAsync<TValue>(
            this Config<object> config,
            IDocument document,
            IExecutionContext context)
        {
            if (config == null)
            {
                return default;
            }

            object value = await config.GetAndTransformValueAsync(document, context);
            return TypeHelper.TryConvert(value, out TValue result) ? result : default;
        }

        public static Config<bool> CombineWith(this Config<bool> first, Config<bool> second)
        {
            if (first == null)
            {
                return second;
            }
            if (second == null)
            {
                return first;
            }
            return new Config<bool>(
                async (doc, ctx) => await first.GetValueAsync(doc, ctx) && await second.GetValueAsync(doc, ctx),
                first.RequiresDocument || second.RequiresDocument);
        }

        public static Task<IEnumerable<IDocument>> FilterAsync(this IEnumerable<IDocument> documents, Config<bool> predicate, IExecutionContext context) =>
            predicate == null ? Task.FromResult(documents) : documents.Query(context).WhereAsync(x => predicate.GetAndTransformValueAsync(x, context)).Task;

        /// <summary>
        /// Filters the documents using "or" logic on multiple predicates.
        /// </summary>
        /// <param name="documents">The documents to filter.</param>
        /// <param name="predicates">The predicates to combine.</param>
        /// <param name="context">The current execution context.</param>
        /// <returns>Filtered documents where at least one of the provided predicates is true.</returns>
        public static async Task<IEnumerable<IDocument>> FilterAsync(this IEnumerable<IDocument> documents, ICollection<Config<bool>> predicates, IExecutionContext context) =>
            predicates == null || predicates.Count == 0
                ? documents
                : await documents.Query(context).WhereAsync(
                    async doc => await predicates.AnyAsync(
                        async pred => await pred.GetAndTransformValueAsync(doc, context)));
    }
}
