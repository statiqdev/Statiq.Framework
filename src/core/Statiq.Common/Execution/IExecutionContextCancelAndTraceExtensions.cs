using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Common
{
    /// <summary>
    /// Extensions to send exception messages to trace output with relevant context such as currently executing module and document.
    /// </summary>
    public static class IExecutionContextCancelAndTraceExtensions
    {
        /// <summary>
        /// If an exception is thrown within the action, an error messages will be sent to the trace output.
        /// The exception will also be re-thrown once the message has been sent to the trace listeners.
        /// </summary>
        /// <typeparam name="TItem">The type of item.</typeparam>
        /// <param name="context">The current execution context.</param>
        /// <param name="item">The item to be processed.</param>
        /// <param name="action">The action to evaluate with the item.</param>
        public static void CancelAndTrace<TItem>(this IExecutionContext context, TItem item, Action<TItem> action)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            try
            {
                action(item);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                TraceException(ex, item, context);
                throw;
            }
        }

        /// <summary>
        /// If an exception is thrown within the action, an error messages will be sent to the trace output.
        /// The exception will also be re-thrown once the message has been sent to the trace listeners.
        /// </summary>
        /// <typeparam name="TItem">The type of item.</typeparam>
        /// <param name="context">The current execution context.</param>
        /// <param name="item">The item to be processed.</param>
        /// <param name="action">The action to evaluate with the item.</param>
        public static async Task CancelAndTraceAsync<TItem>(this IExecutionContext context, TItem item, Func<TItem, Task> action)
        {
            try
            {
                await action(item);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                TraceException(ex, item, context);
                throw;
            }
        }

        /// <summary>
        /// If an exception is thrown within the action, an error messages will be sent to the trace output.
        /// The exception will also be re-thrown once the message has been sent to the trace listeners.
        /// </summary>
        /// <typeparam name="TItem">The type of item.</typeparam>
        /// <typeparam name="TResult">The return type of the function.</typeparam>
        /// <param name="context">The current execution context.</param>
        /// <param name="item">The item to be processed.</param>
        /// <param name="func">The function to evaluate with the item.</param>
        /// <returns>The result of the function.</returns>
        public static TResult CancelAndTrace<TItem, TResult>(this IExecutionContext context, TItem item, Func<TItem, TResult> func)
        {
            try
            {
                return func(item);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                TraceException(ex, item, context);
                throw;
            }
        }

        /// <summary>
        /// If an exception is thrown within the action, an error messages will be sent to the trace output.
        /// The exception will also be re-thrown once the message has been sent to the trace listeners.
        /// </summary>
        /// <typeparam name="TItem">The type of item.</typeparam>
        /// <typeparam name="TResult">The return type of the function.</typeparam>
        /// <param name="context">The current execution context.</param>
        /// <param name="item">The item to be processed.</param>
        /// <param name="func">The function to evaluate with the item.</param>
        /// <returns>The result of the function.</returns>
        public static async Task<TResult> CancelAndTraceAsync<TItem, TResult>(this IExecutionContext context, TItem item, Func<TItem, Task<TResult>> func)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            try
            {
                return await func(item);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                TraceException(ex, item, context);
                throw;
            }
        }

        private static void TraceException<TItem>(Exception ex, TItem item, IExecutionContext context)
        {
            string displayString = item is IDisplayable displayable ? $" [{displayable.ToSafeDisplayString()}]" : string.Empty;
            Trace.Error($"Exception while processing {item.GetType().Name}{displayString}: {ex.Message}");
        }
    }
}
