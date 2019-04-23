using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Util;

namespace Wyam.Common.Configuration
{
    public static class Config
    {
        /// <summary>
        /// Creates a <see cref="ContextConfig{T}"/> (and via casting, a <see cref="DocumentConfig{T}"/>) from an object value.
        /// </summary>
        /// <remarks>
        /// If you need to get a strongly typed configuration, you don't need a helper method as the value can be directly casted to a configuration item.
        /// </remarks>
        /// <param name="value">The value of the configuration.</param>
        /// <returns>A configuration item with the given value.</returns>
        public static ContextConfig<TValue> FromValue<TValue>(TValue value) => new ContextConfig<TValue>(_ => Task.FromResult(value));

        public static ContextConfig<TValue> FromValue<TValue>(Task<TValue> value) => new ContextConfig<TValue>(_ => value);

        public static ContextConfig<IEnumerable<TValue>> FromValues<TValue>(params TValue[] values) => new ContextConfig<IEnumerable<TValue>>(_ => Task.FromResult<IEnumerable<TValue>>(values));

        // No arguments

        public static ContextConfig<TValue> FromContext<TValue>(Func<IExecutionContext, TValue> func) => new ContextConfig<TValue>(ctx => Task.FromResult(func(ctx)));

        public static ContextConfig<TValue> FromContext<TValue>(Func<IExecutionContext, Task<TValue>> func) => new ContextConfig<TValue>(func);

        public static DocumentConfig<TValue> FromDocument<TValue>(Func<IDocument, TValue> func) => new DocumentConfig<TValue>((doc, _) => Task.FromResult(func(doc)));

        public static DocumentConfig<TValue> FromDocument<TValue>(Func<IDocument, Task<TValue>> func) => new DocumentConfig<TValue>((doc, _) => func(doc));

        public static DocumentConfig<TValue> FromDocument<TValue>(Func<IDocument, IExecutionContext, TValue> func) => new DocumentConfig<TValue>((doc, ctx) => Task.FromResult(func(doc, ctx)));

        public static DocumentConfig<TValue> FromDocument<TValue>(Func<IDocument, IExecutionContext, Task<TValue>> func) => new DocumentConfig<TValue>(func);

        // 1 argument

        public static ContextConfig<TArg, TValue> FromContext<TArg, TValue>(Func<IExecutionContext, TArg, TValue> func) => new ContextConfig<TArg, TValue>((ctx, arg) => Task.FromResult(func(ctx, arg)));

        public static ContextConfig<TArg, TValue> FromContext<TArg, TValue>(Func<IExecutionContext, TArg, Task<TValue>> func) => new ContextConfig<TArg, TValue>(func);

        public static DocumentConfig<TArg, TValue> FromDocument<TArg, TValue>(Func<IDocument, IExecutionContext, TArg, TValue> func) => new DocumentConfig<TArg, TValue>((doc, ctx, arg) => Task.FromResult(func(doc, ctx, arg)));

        public static DocumentConfig<TArg, TValue> FromDocument<TArg, TValue>(Func<IDocument, IExecutionContext, TArg, Task<TValue>> func) => new DocumentConfig<TArg, TValue>(func);

        public static ContextConfig<TArg, TValue> FromArgument<TArg, TValue>(Func<TArg, TValue> func) => new ContextConfig<TArg, TValue>((_, arg) => Task.FromResult(func(arg)));

        public static ContextConfig<TArg, TValue> FromArgument<TArg, TValue>(Func<TArg, Task<TValue>> func) => new ContextConfig<TArg, TValue>((_, arg) => func(arg));

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
