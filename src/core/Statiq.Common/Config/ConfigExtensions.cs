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

        /// <summary>
        /// Filters the documents.
        /// </summary>
        /// <param name="documents">The documents to filter.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="context">The current execution context.</param>
        /// <returns>Filtered documents where the provided predicate is true.</returns>
        public static IAsyncEnumerable<IDocument> FilterAsync(this IEnumerable<IDocument> documents, Config<bool> predicate, IExecutionContext context)
        {
            _ = predicate ?? throw new ArgumentNullException(nameof(predicate));
            return documents.FilterAsync(new Config<bool>[] { predicate }, context);
        }

        /// <summary>
        /// Filters the documents using "or" logic on multiple predicates.
        /// </summary>
        /// <param name="documents">The documents to filter.</param>
        /// <param name="predicates">The predicates to combine.</param>
        /// <param name="context">The current execution context.</param>
        /// <returns>Filtered documents where at least one of the provided predicates is true.</returns>
        public static async IAsyncEnumerable<IDocument> FilterAsync(this IEnumerable<IDocument> documents, ICollection<Config<bool>> predicates, IExecutionContext context)
        {
            _ = documents ?? throw new ArgumentNullException(nameof(documents));
            _ = predicates ?? throw new ArgumentNullException(nameof(predicates));

            foreach (IDocument document in documents)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                foreach (Config<bool> predicate in predicates)
                {
                    _ = predicate ?? throw new ArgumentException("Null predicates are not supported", nameof(predicates));
                    if (await predicate.GetAndTransformValueAsync(document, context))
                    {
                        yield return document;
                        break;
                    }
                }
            }
        }
    }
}
