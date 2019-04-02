using System;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Common.Configuration
{
    /// <summary>
    /// Extensions for dealing with config delegates.
    /// </summary>
    public static class ConfigExtensions
    {
        /// <summary>
        /// Invokes the delegate.
        /// </summary>
        /// <typeparam name="T">The desired result type.</typeparam>
        /// <param name="config">The delegate.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>A typed result from invoking the delegate.</returns>
        public static Task<T> InvokeAsync<T>(this AsyncContextConfig config, IExecutionContext context) =>
            InvokeAsync<T>(config, context, null);

        /// <summary>
        /// Invokes the delegate with additional information in the exception message if the conversion fails.
        /// </summary>
        /// <typeparam name="T">The desired result type.</typeparam>
        /// <param name="config">The delegate.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="errorDetails">A string to add to the exception message should the conversion fail.</param>
        /// <returns>A typed result from invoking the delegate.</returns>
        public static async Task<T> InvokeAsync<T>(this AsyncContextConfig config, IExecutionContext context, string errorDetails)
        {
            object value = await config(context);
            if (!context.TryConvert(value, out T result))
            {
                errorDetails = GetErrorDetails(errorDetails);
                throw new InvalidOperationException(
                    $"Could not convert from type {value?.GetType().Name ?? "null"} to type {typeof(T).Name}{errorDetails}");
            }
            return result;
        }

        /// <summary>
        /// Attempts to invoke the delegate and returns a default value of <typeparamref name="T"/> if the conversion fails.
        /// </summary>
        /// <typeparam name="T">The desired result type.</typeparam>
        /// <param name="config">The delegate.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>A typed result from invoking the delegate, or the default value of <typeparamref name="T"/> if the conversion fails.</returns>
        public static async Task<T> TryInvokeAsync<T>(this AsyncContextConfig config, IExecutionContext context)
        {
            object value = await config(context);
            return context.TryConvert(value, out T result) ? result : default(T);
        }

        /// <summary>
        /// Invokes the delegate.
        /// </summary>
        /// <typeparam name="T">The desired result type.</typeparam>
        /// <param name="config">The delegate.</param>
        /// <param name="document">The document.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>A typed result from invoking the delegate.</returns>
        public static Task<T> InvokeAsync<T>(this AsyncDocumentConfig config, IDocument document, IExecutionContext context) =>
            InvokeAsync<T>(config, document, context, null);

        /// <summary>
        /// Invokes the delegate with additional information in the exception message if the conversion fails.
        /// </summary>
        /// <typeparam name="T">The desired result type.</typeparam>
        /// <param name="config">The delegate.</param>
        /// <param name="document">The document.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="errorDetails">A string to add to the exception message should the conversion fail.</param>
        /// <returns>A typed result from invoking the delegate.</returns>
        public static async Task<T> InvokeAsync<T>(this AsyncDocumentConfig config, IDocument document, IExecutionContext context, string errorDetails)
        {
            object value = await config(document, context);
            if (!context.TryConvert(value, out T result))
            {
                errorDetails = GetErrorDetails(errorDetails);
                throw new InvalidOperationException(
                    $"Could not convert from type {value?.GetType().Name ?? "null"} to type {typeof(T).Name} for {document.SourceString()}{errorDetails}");
            }
            return result;
        }

        /// <summary>
        /// Attempts to invoke the delegate and returns a default value of <typeparamref name="T"/> if the conversion fails.
        /// </summary>
        /// <typeparam name="T">The desired result type.</typeparam>
        /// <param name="config">The delegate.</param>
        /// <param name="document">The document.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>A typed result from invoking the delegate, or the default value of <typeparamref name="T"/> if the conversion fails.</returns>
        public static async Task<T> TryInvokeAsync<T>(this AsyncDocumentConfig config, IDocument document, IExecutionContext context)
        {
            object value = await config(document, context);
            return context.TryConvert(value, out T result) ? result : default(T);
        }

        private static string GetErrorDetails(string errorDetails)
        {
            if (errorDetails?.StartsWith(" ") == false)
            {
                errorDetails = " " + errorDetails;
            }
            return errorDetails;
        }
    }
}
