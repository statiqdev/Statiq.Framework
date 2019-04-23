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
        // No arguments

        public static Task<TValue> GetValueAsync<TValue>(
            this DocumentConfig<TValue> config,
            IDocument document,
            IExecutionContext context,
            Func<TValue, TValue> transform = null) =>
            config?.GetAndCacheValueAsync(document, context, transform) ?? Task.FromResult(default(TValue));

        public static async Task<TValue> GetValueAsync<TValue>(
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
            if (!context.TryConvert(value, out TValue result))
            {
                throw new InvalidOperationException(
                    $"Could not convert from type {value?.GetType().Name ?? "null"} to type {typeof(TValue).Name}{Config.GetErrorDetails(errorDetails)}");
            }
            return result;
        }

        public static async Task<TValue> TryGetValueAsync<TValue>(
            this DocumentConfig<object> config,
            IDocument document,
            IExecutionContext context)
        {
            if (config == null)
            {
                return default;
            }

            object value = await config.GetAndCacheValueAsync(document, context);
            return context.TryConvert(value, out TValue result) ? result : default;
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

        // 1 argument

        public static Task<TValue> GetValueAsync<TArg, TValue>(
            this DocumentConfig<TArg, TValue> config,
            IDocument document,
            IExecutionContext context,
            TArg arg,
            Func<TValue, TValue> transform = null) =>
            config?.GetAndCacheValueAsync(document, context, arg, transform) ?? Task.FromResult(default(TValue));

        public static async Task<TValue> GetValueAsync<TArg, TValue>(
            this DocumentConfig<TArg, object> config,
            IDocument document,
            IExecutionContext context,
            TArg arg,
            string errorDetails = null)
        {
            if (config == null)
            {
                return default;
            }

            object value = await config.GetAndCacheValueAsync(document, context, arg);
            if (!context.TryConvert(value, out TValue result))
            {
                throw new InvalidOperationException(
                    $"Could not convert from type {value?.GetType().Name ?? "null"} to type {typeof(TValue).Name}{Config.GetErrorDetails(errorDetails)}");
            }
            return result;
        }

        public static async Task<TValue> TryGetValueAsync<TArg, TValue>(
            this DocumentConfig<TArg, object> config,
            IDocument document,
            IExecutionContext context,
            TArg arg)
        {
            if (config == null)
            {
                return default;
            }

            object value = await config.GetAndCacheValueAsync(document, context, arg);
            return context.TryConvert(value, out TValue result) ? result : default;
        }

        public static DocumentConfig<TArg, bool> CombineWith<TArg>(this DocumentConfig<TArg, bool> first, DocumentConfig<TArg, bool> second)
        {
            if (first == null)
            {
                return second;
            }
            if (second == null)
            {
                return first;
            }
            return new DocumentConfig<TArg, bool>(async (doc, ctx, arg) => await first.GetValueAsync(doc, ctx, arg) && await second.GetValueAsync(doc, ctx, arg));
        }

        public static Task<IEnumerable<IDocument>> FilterAsync<TArg>(this IEnumerable<IDocument> documents, DocumentConfig<TArg, bool> predicate, IExecutionContext context, TArg arg) =>
            predicate == null ? Task.FromResult(documents) : documents.WhereAsync(context, x => predicate.GetValueAsync(x, context, arg));
    }
}
