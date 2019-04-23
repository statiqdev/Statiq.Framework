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
        // No arguments

        public static Task<TValue> GetValueAsync<TValue>(
            this ContextConfig<TValue> config,
            IExecutionContext context,
            Func<TValue, TValue> transform = null) =>
            config?.GetAndCacheValueAsync(null, context, transform) ?? Task.FromResult(default(TValue));

        public static async Task<TValue> GetValueAsync<TValue>(this ContextConfig<object> config, IExecutionContext context, string errorDetails = null)
        {
            if (config == null)
            {
                return default;
            }

            object value = await config.GetAndCacheValueAsync(null, context);
            if (!context.TryConvert(value, out TValue result))
            {
                throw new InvalidOperationException(
                    $"Could not convert from type {value?.GetType().Name ?? "null"} to type {typeof(TValue).Name}{Config.GetErrorDetails(errorDetails)}");
            }
            return result;
        }

        public static async Task<TValue> TryGetValueAsync<TValue>(this ContextConfig<object> config, IExecutionContext context)
        {
            if (config == null)
            {
                return default;
            }

            object value = await config.GetAndCacheValueAsync(null, context);
            return context.TryConvert(value, out TValue result) ? result : default;
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

        // 1 arguments

        public static Task<TValue> GetValueAsync<TArg, TValue>(
            this ContextConfig<TArg, TValue> config,
            IExecutionContext context,
            TArg arg,
            Func<TValue, TValue> transform = null) =>
            config?.GetAndCacheValueAsync(null, context, arg, transform) ?? Task.FromResult(default(TValue));

        public static async Task<TValue> GetValueAsync<TArg, TValue>(
            this ContextConfig<TArg, object> config,
            IExecutionContext context,
            TArg arg,
            string errorDetails = null)
        {
            if (config == null)
            {
                return default;
            }

            object value = await config.GetAndCacheValueAsync(null, context, arg);
            if (!context.TryConvert(value, out TValue result))
            {
                throw new InvalidOperationException(
                    $"Could not convert from type {value?.GetType().Name ?? "null"} to type {typeof(TValue).Name}{Config.GetErrorDetails(errorDetails)}");
            }
            return result;
        }

        public static async Task<TValue> TryGetValueAsync<TArg, TValue>(
            this ContextConfig<TArg, object> config,
            IExecutionContext context,
            TArg arg)
        {
            if (config == null)
            {
                return default;
            }

            object value = await config.GetAndCacheValueAsync(null, context, arg);
            return context.TryConvert(value, out TValue result) ? result : default;
        }

        public static ContextConfig<TArg, bool> CombineWith<TArg>(this ContextConfig<TArg, bool> first, ContextConfig<TArg, bool> second)
        {
            if (first == null)
            {
                return second;
            }
            if (second == null)
            {
                return first;
            }
            return new ContextConfig<TArg, bool>(async (ctx, arg) => await first.GetValueAsync(ctx, arg) && await second.GetValueAsync(ctx, arg));
        }
    }
}
