using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Common
{
    public static class IExecutionContextForEachExtensions
    {
        /// <summary>
        /// Invokes an action on each item.
        /// </summary>
        /// <param name="source">The items to be processed.</param>
        /// <param name="context">The current execution context.</param>
        /// <param name="action">The action to invoke.</param>
        public static void ForEach<TSource>(this IEnumerable<TSource> source, IExecutionContext context, Action<TSource> action)
        {
            foreach (TSource item in source)
            {
                context.CancelAndTrace(item, action);
            }
        }

        /// <summary>
        /// Invokes an action on each item.
        /// </summary>
        /// <param name="source">The items to be processed.</param>
        /// <param name="context">The current execution context.</param>
        /// <param name="action">The action to invoke.</param>
        public static void ParallelForEach<TSource>(this IEnumerable<TSource> source, IExecutionContext context, Action<TSource> action) =>
            Parallel.ForEach(source, context.GetParallelOptions(), item => context.CancelAndTrace(item, action));

        /// <summary>
        /// Invokes an action on each item.
        /// </summary>
        /// <param name="source">The items to be processed.</param>
        /// <param name="context">The current execution context.</param>
        /// <param name="action">The action to invoke.</param>
        public static async Task ForEachAsync<TSource>(this IEnumerable<TSource> source, IExecutionContext context, Func<TSource, Task> action)
        {
            foreach (TSource item in source)
            {
                await context.CancelAndTraceAsync(item, action);
            }
        }

        /// <summary>
        /// Invokes an action on each item.
        /// </summary>
        /// <param name="source">The items to be processed.</param>
        /// <param name="context">The current execution context.</param>
        /// <param name="action">The action to invoke.</param>
        public static Task ParallelForEachAsync<TSource>(this IEnumerable<TSource> source, IExecutionContext context, Func<TSource, Task> action) =>
            Task.WhenAll(source.Select(x => Task.Run(() => context.CancelAndTraceAsync(x, action), context.CancellationToken)));

        /// <summary>
        /// Invokes an action on each input document.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="action">The action to invoke.</param>
        public static void ForEachInput(this IExecutionContext context, Action<IDocument> action) =>
            context.Inputs.ForEach(context, action);

        /// <summary>
        /// Invokes an action on each input document.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="action">The action to invoke.</param>
        public static void ParallelForEachInput(this IExecutionContext context, Action<IDocument> action) =>
            context.Inputs.ParallelForEach(context, action);

        /// <summary>
        /// Invokes an action on each input document.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="action">The action to invoke.</param>
        public static Task ForEachInputAsync(this IExecutionContext context, Func<IDocument, Task> action) =>
            context.Inputs.ForEachAsync(context, action);

        /// <summary>
        /// Invokes an action on each input document.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="action">The action to invoke.</param>
        public static Task ParallelForEachInputAsync(this IExecutionContext context, Func<IDocument, Task> action) =>
            context.Inputs.ParallelForEachAsync(context, action);
    }
}
