using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Common
{
    public static class IExecutionContextSelectManyExtensions
    {
        /// <summary>
        /// Evaluates a LINQ <c>SelectMany</c> method and traces any exceptions.
        /// </summary>
        /// <typeparam name="TSource">The type of item.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="selector">The selector function.</param>
        /// <returns>The result sequence.</returns>
        public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, IExecutionContext context, Func<TSource, IEnumerable<TResult>> selector) =>
            source.SelectMany(x => context.CancelAndTrace(x, selector));

        /// <summary>
        /// Evaluates a PLINQ <c>SelectMany</c> method in parallel and traces any exceptions.
        /// </summary>
        /// <typeparam name="TSource">The type of item.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="query">The source query.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="selector">The selector function.</param>
        /// <returns>The result query.</returns>
        public static ParallelQuery<TResult> SelectMany<TSource, TResult>(this ParallelQuery<TSource> query, IExecutionContext context, Func<TSource, IEnumerable<TResult>> selector) =>
            query.SelectMany(x => context.CancelAndTrace(x, selector));

        /// <summary>
        /// Evaluates a LINQ <c>SelectMany</c> method and traces any exceptions.
        /// </summary>
        /// <typeparam name="TSource">The type of item.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="selector">The selector function.</param>
        /// <returns>The result sequence.</returns>
        public static Task<IEnumerable<TResult>> SelectManyAsync<TSource, TResult>(this IEnumerable<TSource> source, IExecutionContext context, Func<TSource, Task<IEnumerable<TResult>>> selector) =>
            source.SelectManyAsync(async x => await context.CancelAndTraceAsync(x, selector), context.CancellationToken);

        /// <summary>
        /// Evaluates a PLINQ <c>SelectMany</c> method in parallel and traces any exceptions.
        /// </summary>
        /// <typeparam name="TSource">The type of item.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="selector">The selector function.</param>
        /// <returns>The result query.</returns>
        public static Task<IEnumerable<TResult>> ParallelSelectManyAsync<TSource, TResult>(this IEnumerable<TSource> source, IExecutionContext context, Func<TSource, Task<IEnumerable<TResult>>> selector) =>
            source.ParallelSelectManyAsync(x => context.CancelAndTraceAsync(x, selector), context.CancellationToken);

        /// <summary>
        /// Evaluates a LINQ <c>SelectMany</c> method on the input documents and traces any exceptions.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="context">The execution context.</param>
        /// <param name="selector">The selector function.</param>
        /// <returns>The result sequence.</returns>
        public static IEnumerable<TResult> SelectManyInput<TResult>(this IExecutionContext context, Func<IDocument, IEnumerable<TResult>> selector) =>
            context.Inputs.SelectMany(context, selector);

        /// <summary>
        /// Evaluates a PLINQ <c>SelectMany</c> method on the input documents in parallel and traces any exceptions.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="context">The execution context.</param>
        /// <param name="selector">The selector function.</param>
        /// <returns>The result query.</returns>
        public static ParallelQuery<TResult> ParallelSelectManyInput<TResult>(this IExecutionContext context, Func<IDocument, IEnumerable<TResult>> selector) =>
            context.Inputs.AsParallel(context).SelectMany(context, selector);

        /// <summary>
        /// Evaluates a LINQ <c>SelectMany</c> method on the input documents and traces any exceptions.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="context">The execution context.</param>
        /// <param name="selector">The selector function.</param>
        /// <returns>The result sequence.</returns>
        public static Task<IEnumerable<TResult>> SelectManyInputAsync<TResult>(this IExecutionContext context, Func<IDocument, Task<IEnumerable<TResult>>> selector) =>
            context.Inputs.SelectManyAsync(context, selector);

        /// <summary>
        /// Evaluates a PLINQ <c>SelectMany</c> method on the input documents in parallel over a sequence of <see cref="IDocument"/> and traces any exceptions.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="selector">The selector function.</param>
        /// <returns>The result query.</returns>
        public static Task<IEnumerable<TResult>> ParallelSelectManyInputAsync<TResult>(this IExecutionContext context, Func<IDocument, Task<IEnumerable<TResult>>> selector) =>
            context.Inputs.ParallelSelectManyAsync(context, selector);
    }
}
