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
    public static class AsyncQueryExtensions
    {
        public static AsyncQuery<TSource> WhereAsync<TSource>(this Query<TSource> source, Func<TSource, Task<bool>> asyncPredicate) =>
            source.ThenAsync(items => items.WhereAsync(item => source.Context.CancelAndTraceAsync(item, asyncPredicate), source.Context.CancellationToken));

        public static AsyncQuery<TSource> WhereAsync<TSource>(this AsyncQuery<TSource> source, Func<TSource, Task<bool>> asyncPredicate) =>
            source.ThenAsync(items => items.WhereAsync(item => source.Context.CancelAndTraceAsync(item, asyncPredicate), source.Context.CancellationToken));

        public static AsyncQuery<TResult> SelectAsync<TSource, TResult>(this Query<TSource> source, Func<TSource, Task<TResult>> asyncPredicate) =>
            source.ThenAsync(items => items.SelectAsync(item => source.Context.CancelAndTraceAsync(item, asyncPredicate), source.Context.CancellationToken));

        public static AsyncQuery<TResult> SelectAsync<TSource, TResult>(this AsyncQuery<TSource> source, Func<TSource, Task<TResult>> asyncPredicate) =>
            source.ThenAsync(items => items.SelectAsync(item => source.Context.CancelAndTraceAsync(item, asyncPredicate), source.Context.CancellationToken));

        public static AsyncQuery<TResult> SelectManyAsync<TSource, TResult>(this Query<TSource> source, Func<TSource, Task<IEnumerable<TResult>>> asyncPredicate) =>
            source.ThenAsync(items => items.SelectManyAsync(item => source.Context.CancelAndTraceAsync(item, asyncPredicate), source.Context.CancellationToken));

        public static AsyncQuery<TResult> SelectManyAsync<TSource, TResult>(this AsyncQuery<TSource> source, Func<TSource, Task<IEnumerable<TResult>>> asyncPredicate) =>
            source.ThenAsync(items => items.SelectManyAsync(item => source.Context.CancelAndTraceAsync(item, asyncPredicate), source.Context.CancellationToken));

        public static async Task ForEachAsync<TSource>(this Query<TSource> source, Func<TSource, Task> action)
        {
            foreach (TSource item in source)
            {
                await source.Context.CancelAndTraceAsync(item, action);
            }
        }

        public static async Task ForEachAsync<TSource>(this AsyncQuery<TSource> source, Func<TSource, Task> action)
        {
            foreach (TSource item in await source)
            {
                await source.Context.CancelAndTraceAsync(item, action);
            }
        }

        public static Task<List<TSource>> ToListAsync<TSource>(this AsyncQuery<TSource> source) =>
            source.ThenAsync(x => x.ToList());

        public static Task<TSource[]> ToArrayAsync<TSource>(this AsyncQuery<TSource> source) =>
            source.ThenAsync(x => x.ToArray());

        public static Task<ImmutableArray<TSource>> ToImmutableArrayAsync<TSource>(this AsyncQuery<TSource> source) =>
            source.ThenAsync(x => x.ToImmutableArray());

        public static async Task<TResult> ThenAsync<TSource, TResult>(this AsyncQuery<TSource> source, Func<IEnumerable<TSource>, Task<TResult>> func) => await func(await source);

        public static async Task<TResult> ThenAsync<TSource, TResult>(this AsyncQuery<TSource> source, Func<IEnumerable<TSource>, TResult> func) => func(await source);
    }
}
