using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static class ConfigTransformExtensions
    {
        public static Config<TValue> Transform<TValue, TSource>(this Config<TSource> config, Func<TSource, TValue> transform)
        {
            _ = transform ?? throw new ArgumentNullException(nameof(transform));

            if (config == null)
            {
                return Config.FromValue(transform(default));
            }
            return config.RequiresDocument
                ? Config.FromDocument(async (doc, ctx) => transform(await config.GetValueAsync(doc, ctx)))
                : Config.FromContext(async ctx => transform(await config.GetValueAsync(null, ctx)));
        }

        public static Config<TValue> Transform<TValue, TSource>(this Config<TSource> config, Func<TSource, Task<TValue>> transform)
        {
            _ = transform ?? throw new ArgumentNullException(nameof(transform));

            if (config == null)
            {
                return Config.FromValue(transform(default));
            }
            return config.RequiresDocument
                ? Config.FromDocument(async (doc, ctx) => await transform(await config.GetValueAsync(doc, ctx)))
                : Config.FromContext(async ctx => await transform(await config.GetValueAsync(null, ctx)));
        }

        public static Config<TValue> Transform<TValue, TSource>(this Config<TSource> config, Func<TSource, IExecutionContext, TValue> transform)
        {
            _ = transform ?? throw new ArgumentNullException(nameof(transform));

            if (config == null)
            {
                return Config.FromContext(ctx => transform(default, ctx));
            }
            return config.RequiresDocument
                ? Config.FromDocument(async (doc, ctx) => transform(await config.GetValueAsync(doc, ctx), ctx))
                : Config.FromContext(async ctx => transform(await config.GetValueAsync(null, ctx), ctx));
        }

        public static Config<TValue> Transform<TValue, TSource>(this Config<TSource> config, Func<TSource, IExecutionContext, Task<TValue>> transform)
        {
            _ = transform ?? throw new ArgumentNullException(nameof(transform));

            if (config == null)
            {
                return Config.FromContext(ctx => transform(default, ctx));
            }
            return config.RequiresDocument
                ? Config.FromDocument(async (doc, ctx) => await transform(await config.GetValueAsync(doc, ctx), ctx))
                : Config.FromContext(async ctx => await transform(await config.GetValueAsync(null, ctx), ctx));
        }

        public static Config<TValue> Transform<TValue, TSource>(this Config<TSource> config, Func<TSource, IDocument, IExecutionContext, TValue> transform)
        {
            _ = transform ?? throw new ArgumentNullException(nameof(transform));

            if (config == null)
            {
                return Config.FromDocument((doc, ctx) => transform(default, doc, ctx));
            }
            return Config.FromDocument(async (doc, ctx) => transform(await config.GetValueAsync(doc, ctx), doc, ctx));
        }

        public static Config<TValue> Transform<TValue, TSource>(this Config<TSource> config, Func<TSource, IDocument, IExecutionContext, Task<TValue>> transform)
        {
            _ = transform ?? throw new ArgumentNullException(nameof(transform));

            if (config == null)
            {
                return Config.FromDocument((doc, ctx) => transform(default, doc, ctx));
            }
            return Config.FromDocument(async (doc, ctx) => await transform(await config.GetValueAsync(doc, ctx), doc, ctx));
        }
    }
}
