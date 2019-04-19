using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Common.Configuration
{
    public static class ContextConfigExtensions
    {
        public static Task<T> GetValueAsync<T>(
            this ContextConfig<T> config,
            IExecutionContext context,
            Func<T, T> transform = null) =>
            config?.GetAndCacheValueAsync(null, context, transform) ?? Task.FromResult(default(T));

        public static async Task<T> GetValueAsync<T>(this ContextConfig<object> config, IExecutionContext context, string errorDetails = null)
        {
            if (config == null)
            {
                return default;
            }

            object value = await config.GetAndCacheValueAsync(null, context);
            if (!context.TryConvert(value, out T result))
            {
                throw new InvalidOperationException(
                    $"Could not convert from type {value?.GetType().Name ?? "null"} to type {typeof(T).Name}{Config.GetErrorDetails(errorDetails)}");
            }
            return result;
        }

        public static async Task<T> TryGetValueAsync<T>(this ContextConfig<object> config, IExecutionContext context)
        {
            if (config == null)
            {
                return default;
            }

            object value = await config.GetAndCacheValueAsync(null, context);
            return context.TryConvert(value, out T result) ? result : default;
        }

        public static ContextConfig<bool> CombineWith(this ContextConfig<bool> first, ContextConfig<bool> second)
        {
            if (first == null)
            {
                return second;
            }
            if (second == null)
            {
                return first;
            }
            return new ContextConfig<bool>(async ctx => await first.GetValueAsync(ctx) && await second.GetValueAsync(ctx));
        }

        public static ContextConfig<Func<T, bool>> CombineWith<T>(
            this ContextConfig<Func<T, bool>> first,
            ContextConfig<Func<T, bool>> second)
        {
            if (first == null)
            {
                return second;
            }
            if (second == null)
            {
                return first;
            }
            return new ContextConfig<Func<T, bool>>(async ctx =>
            {
                Func<T, bool> innerFirst = await first.GetValueAsync(ctx);
                Func<T, bool> innerSecond = await second.GetValueAsync(ctx);
                if (innerFirst == null)
                {
                    return innerSecond;
                }
                if (innerSecond == null)
                {
                    return innerFirst;
                }
                return x => innerFirst(x) && innerSecond(x);
            });
        }
    }
}
