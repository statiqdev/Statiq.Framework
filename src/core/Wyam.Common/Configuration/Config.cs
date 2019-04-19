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
        public static ContextConfig<T> FromValue<T>(T value) => new ContextConfig<T>(_ => Task.FromResult(value));

        public static ContextConfig<T> AsyncFromValue<T>(Task<T> value) => new ContextConfig<T>(_ => value);

        public static ContextConfig<IEnumerable<T>> FromValues<T>(params T[] values) => new ContextConfig<IEnumerable<T>>(_ => Task.FromResult<IEnumerable<T>>(values));

        public static ContextConfig<T> FromContext<T>(Func<IExecutionContext, T> func) => new ContextConfig<T>(ctx => Task.FromResult(func(ctx)));

        public static ContextConfig<T> AsyncFromContext<T>(Func<IExecutionContext, Task<T>> func) => new ContextConfig<T>(func);

        public static DocumentConfig<T> FromDocument<T>(Func<IDocument, IExecutionContext, T> func) => new DocumentConfig<T>((doc, ctx) => Task.FromResult(func(doc, ctx)));

        public static DocumentConfig<T> AsyncFromDocument<T>(Func<IDocument, IExecutionContext, Task<T>> func) => new DocumentConfig<T>(func);

        public static DocumentConfig<T> FromDocument<T>(Func<IDocument, T> func) => new DocumentConfig<T>((doc, _) => Task.FromResult(func(doc)));

        public static DocumentConfig<T> AsyncFromDocument<T>(Func<IDocument, Task<T>> func) => new DocumentConfig<T>((doc, _) => func(doc));

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
