using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections;

namespace Statiq.Common
{
    public static class ParallelQueryExtensions
    {
        public static ParallelQuery<TSource> AsParallel<TSource>(this IEnumerable<TSource> source, IExecutionContext context) =>
            source is ParallelQuery<TSource> query ? query : source.AsParallel(context, false);

        public static ParallelQuery<TSource> AsParallel<TSource>(this IEnumerable<TSource> source, IExecutionContext context, bool ordered) =>
            source is ParallelQuery<TSource> query
                ? query
                : ordered
                    ? new ParallelQuery<TSource>(source.AsParallel().AsOrdered().WithCancellation(context.CancellationToken), context)
                    : new ParallelQuery<TSource>(source.AsParallel().WithCancellation(context.CancellationToken), context);

        public static ParallelQuery<IDocument> AsParallel(this IEnumerable<IDocument> documents, IExecutionContext context) => documents.AsParallel(context, true);

        public static ParallelQuery<IDocument> ParallelQueryInputs(this IExecutionContext context) => context.Inputs.AsParallel(context);

        public static ParallelQuery<TSource> Where<TSource>(this ParallelQuery<TSource> source, Func<TSource, bool> predicate) =>
            source.Then(items => items.Where(item => source.Context.CancelAndTrace(item, predicate)));

        public static ParallelQuery<TResult> Select<TSource, TResult>(this ParallelQuery<TSource> source, Func<TSource, TResult> selector) =>
            source.Then(items => items.Select(item => source.Context.CancelAndTrace(item, selector)));

        public static ParallelQuery<TResult> SelectMany<TSource, TResult>(this ParallelQuery<TSource> source, Func<TSource, IEnumerable<TResult>> selector) =>
            source.Then(items => items.SelectMany(item => source.Context.CancelAndTrace(item, selector)));

        public static void ForEach<TSource>(this ParallelQuery<TSource> source, Action<TSource> action) =>
            Parallel.ForEach(source, new ParallelOptions { CancellationToken = source.Context.CancellationToken }, item => source.Context.CancelAndTrace(item, action));
    }
}
