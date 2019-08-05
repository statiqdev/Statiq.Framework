using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections;

namespace Statiq.Common
{
    public static class QueryExtensions
    {
        /// <summary>
        /// Creates a query that can operate on the source enumerable.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <param name="source">The source enumerable.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>A query.</returns>
        public static Query<TSource> Query<TSource>(this IEnumerable<TSource> source, IExecutionContext context) =>
            source is Query<TSource> query ? query : new Query<TSource>(source, context);

        /// <summary>
        /// Makes all following chained operations evaluate items in parallel and unordered.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <param name="query">The source query.</param>
        /// <returns>A parallel query.</returns>
        public static ParallelQuery<TSource> Parallel<TSource>(this Query<TSource> query) => query.Parallel(false);

        /// <summary>
        /// Makes all following chained operations evaluate items in parallel and ordered.
        /// </summary>
        /// <param name="query">The source query.</param>
        /// <returns>A parallel query.</returns>
        public static ParallelQuery<IDocument> Parallel(this Query<IDocument> query) => query.Parallel(true);

        /// <summary>
        /// Makes all following chained operations evaluate items in parallel and with the specified ordering behavior.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <param name="query">The source query.</param>
        /// <param name="ordered"><c>true</c> to order the results based on their initial order, <c>false</c> otherwise.</param>
        /// <returns>A parallel query.</returns>
        public static ParallelQuery<TSource> Parallel<TSource>(this Query<TSource> query, bool ordered)
        {
            _ = query ?? throw new ArgumentNullException(nameof(query));
            return ordered
                ? new ParallelQuery<TSource>(query.AsParallel().AsOrdered().WithCancellation(query.Context.CancellationToken), query.Context)
                : new ParallelQuery<TSource>(query.AsParallel().WithCancellation(query.Context.CancellationToken), query.Context);
        }

        /// <summary>
        /// Makes all following chained operations evaluate items sequentially.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <param name="query">The source query.</param>
        /// <returns>A sequential query.</returns>
        public static Query<TSource> Sequential<TSource>(this ParallelQuery<TSource> query) => new Query<TSource>(query, query.Context);
    }
}
