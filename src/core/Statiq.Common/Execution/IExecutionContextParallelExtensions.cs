using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Common
{
    public static class IExecutionContextParallelExtensions
    {
        /// <summary>
        /// Returns a <see cref="ParallelQuery{TSource}"/> that
        /// supports cancellation using the context cancellation token.
        /// </summary>
        /// <remarks>
        /// By default the parallel query is not ordered. Use
        /// <see cref="AsParallel{TSource}(IEnumerable{TSource}, IExecutionContext, bool)"/>
        /// to specify ordering behavior.
        /// </remarks>
        /// <typeparam name="TSource">The type of elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">An enumerable to convert to a parallel query.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>A default <see cref="ParallelQuery{TSource}"/>.</returns>
        public static ParallelQuery<TSource> AsParallel<TSource>(this IEnumerable<TSource> source, IExecutionContext context) => source.AsParallel(context, false);

        /// <summary>
        /// Returns a <see cref="ParallelQuery{TSource}"/> that
        /// supports cancellation using the context cancellation token.
        /// </summary>
        /// <typeparam name="TSource">The type of elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">An enumerable to convert to a parallel query.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="ordered"><c>true</c> to order the results of the parallel query, <c>false</c> otherwise.</param>
        /// <returns>A default <see cref="ParallelQuery{TSource}"/>.</returns>
        public static ParallelQuery<TSource> AsParallel<TSource>(this IEnumerable<TSource> source, IExecutionContext context, bool ordered)
        {
            return ordered
                ? source.AsParallel().AsOrdered().WithCancellation(context.CancellationToken)
                : source.AsParallel().WithCancellation(context.CancellationToken);
        }

        /// <summary>
        /// Returns a <see cref="ParallelQuery{TSource}"/> that is ordered and
        /// supports cancellation using the context cancellation token.
        /// </summary>
        /// <remarks>
        /// Queries that operate on <see cref="IDocument"/> are ordered by default. Use
        /// <see cref="AsParallel{TSource}(IEnumerable{TSource}, IExecutionContext, bool)"/>
        /// to specify ordering behavior.
        /// </remarks>
        /// <param name="source">An enumerable to convert to a parallel query.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>A default <see cref="ParallelQuery{IDocument}"/>.</returns>
        public static ParallelQuery<IDocument> AsParallel(this IEnumerable<IDocument> source, IExecutionContext context) => source.AsParallel(context, true);

        /// <summary>
        /// Gets a <see cref="ParallelOptions"/> instance populated with the <see cref="CancellationToken"/>
        /// from the execution context.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <returns>A <see cref="ParallelOptions"/> instance.</returns>
        public static ParallelOptions GetParallelOptions(this IExecutionContext context) =>
            new ParallelOptions { CancellationToken = context.CancellationToken };
    }
}
