using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static class Config
    {
        public static DocumentConfig<TValue> FromValue<TValue>(TValue value) => new DocumentConfig<TValue>((_, __) => Task.FromResult(value), false);

        public static DocumentConfig<TValue> FromValue<TValue>(Task<TValue> value) => new DocumentConfig<TValue>((_, __) => value, false);

        public static DocumentConfig<IEnumerable<TValue>> FromValues<TValue>(params TValue[] values) => new DocumentConfig<IEnumerable<TValue>>((_, __) => Task.FromResult<IEnumerable<TValue>>(values), false);

        public static DocumentConfig<TValue> FromContext<TValue>(Func<IExecutionContext, TValue> func) => new DocumentConfig<TValue>((_, ctx) => Task.FromResult(func(ctx)), false);

        public static DocumentConfig<TValue> FromContext<TValue>(Func<IExecutionContext, Task<TValue>> func) => new DocumentConfig<TValue>((_, ctx) => func(ctx), false);

        public static DocumentConfig<TValue> FromContext<TValue>(string key) => new DocumentConfig<TValue>((_, ctx) => Task.FromResult(ctx.Get<TValue>(key)), false);

        public static DocumentConfig<TValue> FromContext<TValue>(string key, TValue defaultValue) => new DocumentConfig<TValue>((_, ctx) => Task.FromResult(ctx.Get(key, defaultValue)), false);

        public static DocumentConfig<object> FromContext(string key, object defaultValue = null) => new DocumentConfig<object>((_, ctx) => Task.FromResult(ctx.Get(key, defaultValue)), false);

        public static DocumentConfig<TValue> FromDocument<TValue>(Func<IDocument, TValue> func) => new DocumentConfig<TValue>((doc, _) => Task.FromResult(func(doc)));

        public static DocumentConfig<TValue> FromDocument<TValue>(Func<IDocument, Task<TValue>> func) => new DocumentConfig<TValue>((doc, _) => func(doc));

        public static DocumentConfig<TValue> FromDocument<TValue>(Func<IDocument, IExecutionContext, TValue> func) => new DocumentConfig<TValue>((doc, ctx) => Task.FromResult(func(doc, ctx)));

        public static DocumentConfig<TValue> FromDocument<TValue>(Func<IDocument, IExecutionContext, Task<TValue>> func) => new DocumentConfig<TValue>(func);

        public static DocumentConfig<TValue> FromDocument<TValue>(string key) => new DocumentConfig<TValue>((doc, _) => Task.FromResult(doc.Get<TValue>(key)));

        public static DocumentConfig<TValue> FromDocument<TValue>(string key, TValue defaultValue) => new DocumentConfig<TValue>((doc, _) => Task.FromResult(doc.Get(key, defaultValue)));

        public static DocumentConfig<object> FromDocument(string key, object defaultValue = null) => new DocumentConfig<object>((doc, _) => Task.FromResult(doc.Get(key, defaultValue)));

        // This just adds a space to the front of error details so it'll format nicely
        // Used by the extensions that convert values from a DocumentConfig<object> or ContextConfig<object>
        internal static string GetErrorDetails(string errorDetails)
        {
            if (errorDetails?.StartsWith(" ") == false)
            {
                errorDetails = " " + errorDetails;
            }
            return errorDetails;
        }
    }
}
