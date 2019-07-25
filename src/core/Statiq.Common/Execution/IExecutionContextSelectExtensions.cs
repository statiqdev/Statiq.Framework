using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Common
{
    public static class IExecutionContextSelectExtensions
    {
        /// <summary>
        /// Evaluates a LINQ <c>Select</c> method and traces any exceptions.
        /// </summary>
        /// <typeparam name="TSource">The type of item.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="selector">The selector function.</param>
        /// <returns>The result sequence.</returns>
        public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, IExecutionContext context, Func<TSource, TResult> selector) =>
            source.Select(x => context.CancelAndTrace(x, selector));

        /// <summary>
        /// Evaluates a PLINQ <c>Select</c> method in parallel and traces any exceptions.
        /// </summary>
        /// <typeparam name="TSource">The type of item.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="query">The source query.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="selector">The selector function.</param>
        /// <returns>The result query.</returns>
        public static ParallelQuery<TResult> Select<TSource, TResult>(this ParallelQuery<TSource> query, IExecutionContext context, Func<TSource, TResult> selector) =>
            query.Select(x => context.CancelAndTrace(x, selector));

        /// <summary>
        /// Evaluates a LINQ <c>Select</c> method and traces any exceptions.
        /// </summary>
        /// <typeparam name="TSource">The type of item.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="selector">The selector function.</param>
        /// <returns>The result sequence.</returns>
        public static Task<IEnumerable<TResult>> SelectAsync<TSource, TResult>(this IEnumerable<TSource> source, IExecutionContext context, Func<TSource, Task<TResult>> selector) =>
            source.SelectAsync(x => context.CancelAndTraceAsync(x, selector), context.CancellationToken);

        /// <summary>
        /// Evaluates a LINQ <c>Select</c> method in parallel and traces any exceptions.
        /// </summary>
        /// <typeparam name="TSource">The type of item.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="selector">The selector function.</param>
        /// <returns>The result sequence.</returns>
        public static Task<IEnumerable<TResult>> ParallelSelectAsync<TSource, TResult>(this IEnumerable<TSource> source, IExecutionContext context, Func<TSource, Task<TResult>> selector) =>
            source.ParallelSelectAsync(x => context.CancelAndTraceAsync(x, selector), context.CancellationToken);

        /// <summary>
        /// Evaluates a LINQ <c>Select</c> method on the input documents and traces any exceptions.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="context">The execution context.</param>
        /// <param name="selector">The selector function.</param>
        /// <returns>The result sequence.</returns>
        public static IEnumerable<TResult> SelectInput<TResult>(this IExecutionContext context, Func<IDocument, TResult> selector) =>
            context.Inputs.Select(context, selector);

        /// <summary>
        /// Evaluates a PLINQ <c>Select</c> method on the input documents and traces any exceptions.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="context">The execution context.</param>
        /// <param name="selector">The selector function.</param>
        /// <returns>The result query.</returns>
        public static ParallelQuery<TResult> ParallelSelectInput<TResult>(this IExecutionContext context, Func<IDocument, TResult> selector) =>
            context.Inputs.AsParallel(context).Select(context, selector);

        /// <summary>
        /// Evaluates a LINQ <c>Select</c> method on the input documents and traces any exceptions.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="context">The execution context.</param>
        /// <param name="selector">The selector function.</param>
        /// <returns>The result sequence.</returns>
        public static Task<IEnumerable<TResult>> SelectInputAsync<TResult>(this IExecutionContext context, Func<IDocument, Task<TResult>> selector) =>
            context.Inputs.SelectAsync(context, selector);

        /// <summary>
        /// Evaluates a LINQ <c>Select</c> method on the input documents in parallel and traces any exceptions.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="context">The execution context.</param>
        /// <param name="selector">The selector function.</param>
        /// <returns>The result sequence.</returns>
        public static Task<IEnumerable<TResult>> ParallelSelectInputAsync<TResult>(this IExecutionContext context, Func<IDocument, Task<TResult>> selector) =>
            context.Inputs.ParallelSelectAsync(context, selector);
    }
}
