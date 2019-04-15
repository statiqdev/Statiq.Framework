using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Common.Configuration
{
    public static class Config
    {
        /// <summary>
        /// Creates a <see cref="ContextConfig"/> (and via casting, a <see cref="DocumentConfig"/>) from an object value.
        /// </summary>
        /// <remarks>
        /// If you need to get a strongly typed configuration, you don't need a helper method as the value can be directly casted to a configuration item.
        /// </remarks>
        /// <param name="value">The value of the configuration.</param>
        /// <returns>A configuration item with the given value.</returns>
        public static ContextConfig FromValue(object value) => new ContextConfig(_ => Task.FromResult(value));

        public static ContextConfig AsyncFromValue(Task<object> value) => new ContextConfig(_ => value);

        public static ContextConfig FromContext(Func<IExecutionContext, object> func) => new ContextConfig(ctx => Task.FromResult(func(ctx)));

        public static ContextConfig AsyncFromContext(Func<IExecutionContext, Task<object>> func) => new ContextConfig(func);

        public static ContextConfig<T> FromContext<T>(Func<IExecutionContext, T> func) => new ContextConfig<T>(ctx => Task.FromResult(func(ctx)));

        public static ContextConfig<T> AsyncFromContext<T>(Func<IExecutionContext, Task<T>> func) => new ContextConfig<T>(func);

        public static DocumentConfig FromDocument(Func<IDocument, IExecutionContext, object> func) => new DocumentConfig((doc, ctx) => Task.FromResult(func(doc, ctx)));

        public static DocumentConfig AsyncFromDocument(Func<IDocument, IExecutionContext, Task<object>> func) => new DocumentConfig(func);

        public static DocumentConfig<T> FromDocument<T>(Func<IDocument, IExecutionContext, T> func) => new DocumentConfig<T>((doc, ctx) => Task.FromResult(func(doc, ctx)));

        public static DocumentConfig<T> AsyncFromDocument<T>(Func<IDocument, IExecutionContext, Task<T>> func) => new DocumentConfig<T>(func);

        public static DocumentConfig FromDocument(Func<IDocument, object> func) => new DocumentConfig((doc, _) => Task.FromResult(func(doc)));

        public static DocumentConfig AsyncFromDocument(Func<IDocument, Task<object>> func) => new DocumentConfig((doc, _) => func(doc));

        public static DocumentConfig<T> FromDocument<T>(Func<IDocument, T> func) => new DocumentConfig<T>((doc, _) => Task.FromResult(func(doc)));

        public static DocumentConfig<T> AsyncFromDocument<T>(Func<IDocument, Task<T>> func) => new DocumentConfig<T>((doc, _) => func(doc));

        public static ContextPredicate IfContext(Func<IExecutionContext, bool> func) => new ContextPredicate(ctx => Task.FromResult(func(ctx)));

        public static ContextPredicate AsyncIfContext(Func<IExecutionContext, Task<bool>> func) => new ContextPredicate(func);

        public static DocumentPredicate IfDocument(Func<IDocument, IExecutionContext, bool> func) => new DocumentPredicate((doc, ctx) => Task.FromResult(func(doc, ctx)));

        public static DocumentPredicate AsyncIfDocument(Func<IDocument, IExecutionContext, Task<bool>> func) => new DocumentPredicate(func);

        public static DocumentPredicate IfDocument(Func<IDocument, bool> func) => new DocumentPredicate((doc, _) => Task.FromResult(func(doc)));

        public static DocumentPredicate AsyncIfDocument(Func<IDocument, Task<bool>> func) => new DocumentPredicate((doc, _) => func(doc));

        // This just adds a space to the front of error details so it'll format nicely
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
