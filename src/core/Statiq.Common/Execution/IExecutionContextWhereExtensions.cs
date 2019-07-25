using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Common
{
    public static class IExecutionContextWhereExtensions
    {
        /// <summary>
        /// Evaluates a LINQ <c>Where</c> method and traces any exceptions.
        /// </summary>
        /// <typeparam name="TSource">The type of item.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="predicate">The predicate function.</param>
        /// <returns>The result sequence.</returns>
        public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, IExecutionContext context, Func<TSource, bool> predicate) =>
            source.Where(x => context.CancelAndTrace(x, predicate));

        /// <summary>
        /// Evaluates a PLINQ <c>Where</c> method in parallel and traces any exceptions.
        /// </summary>
        /// <typeparam name="TSource">The type of item.</typeparam>
        /// <param name="query">The source query.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="predicate">The predicate function.</param>
        /// <returns>The result query.</returns>
        public static ParallelQuery<TSource> Where<TSource>(this ParallelQuery<TSource> query, IExecutionContext context, Func<TSource, bool> predicate) =>
            query.Where(x => context.CancelAndTrace(x, predicate));

        /// <summary>
        /// Evaluates a LINQ <c>Where</c> method and traces any exceptions.
        /// </summary>
        /// <typeparam name="TSource">The type of item.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="predicate">The predicate function.</param>
        /// <returns>The result sequence.</returns>
        public static Task<IEnumerable<TSource>> WhereAsync<TSource>(this IEnumerable<TSource> source, IExecutionContext context, Func<TSource, Task<bool>> predicate) =>
            source.WhereAsync(x => context.CancelAndTraceAsync(x, predicate), context.CancellationToken);

        /// <summary>
        /// Evaluates a LINQ <c>Where</c> method in parallel and traces any exceptions.
        /// </summary>
        /// <typeparam name="TSource">The type of item.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="predicate">The predicate function.</param>
        /// <returns>The result sequence.</returns>
        public static Task<IEnumerable<TSource>> ParallelWhereAsync<TSource>(this IEnumerable<TSource> source, IExecutionContext context, Func<TSource, Task<bool>> predicate) =>
            source.ParallelWhereAsync(x => context.CancelAndTraceAsync(x, predicate), context.CancellationToken);

        /// <summary>
        /// Evaluates a LINQ <c>Where</c> method on the input documents and traces any exceptions.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="predicate">The predicate function.</param>
        /// <returns>The result sequence.</returns>
        public static IEnumerable<IDocument> WhereInput(this IExecutionContext context, Func<IDocument, bool> predicate) =>
            context.Inputs.Where(context, predicate);

        /// <summary>
        /// Evaluates a PLINQ <c>Where</c> method on the input documents in parallel and traces any exceptions.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="predicate">The predicate function.</param>
        /// <returns>The result query.</returns>
        public static ParallelQuery<IDocument> ParallelWhereInput(this IExecutionContext context, Func<IDocument, bool> predicate) =>
            context.Inputs.AsParallel(context).Where(context, predicate);

        /// <summary>
        /// Evaluates a LINQ <c>Where</c> method on the input documents and traces any exceptions.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="predicate">The predicate function.</param>
        /// <returns>The result sequence.</returns>
        public static Task<IEnumerable<IDocument>> WhereInputAsync(this IExecutionContext context, Func<IDocument, Task<bool>> predicate) =>
            context.Inputs.WhereAsync(context, predicate);

        /// <summary>
        /// Evaluates a LINQ <c>Where</c> method on the input documents in parallel and traces any exceptions.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="predicate">The predicate function.</param>
        /// <returns>The result sequence.</returns>
        public static Task<IEnumerable<IDocument>> ParallelWhereInputAsync(this IExecutionContext context, Func<IDocument, Task<bool>> predicate) =>
            context.Inputs.ParallelWhereAsync(context, predicate);
    }
}
