using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Immutable;

namespace Statiq.Common
{
    public static class ParallelAsyncQueryLinqExtensions
    {
        public static ParallelAsyncQuery<TSource> WhereAsync<TSource>(this ParallelQuery<TSource> source, Func<TSource, Task<bool>> asyncPredicate) =>
            source.ThenAsync(items => items.ParallelWhereAsync(item => source.Context.CancelAndTraceAsync(item, asyncPredicate), source.Context.CancellationToken));

        public static ParallelAsyncQuery<TSource> WhereAsync<TSource>(this ParallelAsyncQuery<TSource> source, Func<TSource, Task<bool>> asyncPredicate) =>
            source.ThenAsync(items => items.ParallelWhereAsync(item => source.Context.CancelAndTraceAsync(item, asyncPredicate), source.Context.CancellationToken));

        public static ParallelAsyncQuery<TResult> SelectAsync<TSource, TResult>(this ParallelQuery<TSource> source, Func<TSource, Task<TResult>> asyncPredicate) =>
            source.ThenAsync(items => items.ParallelSelectAsync(item => source.Context.CancelAndTraceAsync(item, asyncPredicate), source.Context.CancellationToken));

        public static ParallelAsyncQuery<TResult> SelectAsync<TSource, TResult>(this ParallelAsyncQuery<TSource> source, Func<TSource, Task<TResult>> asyncPredicate) =>
            source.ThenAsync(items => items.ParallelSelectAsync(item => source.Context.CancelAndTraceAsync(item, asyncPredicate), source.Context.CancellationToken));

        public static ParallelAsyncQuery<TResult> SelectManyAsync<TSource, TResult>(this ParallelQuery<TSource> source, Func<TSource, Task<IEnumerable<TResult>>> asyncPredicate) =>
            source.ThenAsync(items => items.ParallelSelectManyAsync(item => source.Context.CancelAndTraceAsync(item, asyncPredicate), source.Context.CancellationToken));

        public static ParallelAsyncQuery<TResult> SelectManyAsync<TSource, TResult>(this ParallelAsyncQuery<TSource> source, Func<TSource, Task<IEnumerable<TResult>>> asyncPredicate) =>
            source.ThenAsync(items => items.ParallelSelectManyAsync(item => source.Context.CancelAndTraceAsync(item, asyncPredicate), source.Context.CancellationToken));

        public static Task ForEachAsync<TSource>(this ParallelQuery<TSource> source, Func<TSource, Task> action) =>
            Task.WhenAll(source.Select(x => Task.Run(() => source.Context.CancelAndTraceAsync(x, action), source.Context.CancellationToken)));

        public static async Task ForEachAsync<TSource>(this ParallelAsyncQuery<TSource> source, Func<TSource, Task> action) =>
            await Task.WhenAll((await source).Select(x => Task.Run(() => source.Context.CancelAndTraceAsync(x, action), source.Context.CancellationToken)));

        public static Task<List<TSource>> ToListAsync<TSource>(this ParallelAsyncQuery<TSource> source) =>
            source.ThenAsync(x => x.ToList());

        public static Task<TSource[]> ToArrayAsync<TSource>(this ParallelAsyncQuery<TSource> source) =>
            source.ThenAsync(x => x.ToArray());

        public static Task<ImmutableArray<TSource>> ToImmutableArrayAsync<TSource>(this ParallelAsyncQuery<TSource> source) =>
            source.ThenAsync(x => x.ToImmutableArray());

        public static async Task<TResult> ThenAsync<TSource, TResult>(this ParallelAsyncQuery<TSource> source, Func<IEnumerable<TSource>, Task<TResult>> func) => await func(await source);

        public static async Task<TResult> ThenAsync<TSource, TResult>(this ParallelAsyncQuery<TSource> source, Func<IEnumerable<TSource>, TResult> func) => func(await source);
    }
}
