using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections;

namespace Statiq.Common
{
    public static class AsyncQueryExtensions
    {
        public static AsyncQuery<TSource> WhereAsync<TSource>(this Query<TSource> source, Func<TSource, Task<bool>> asyncPredicate) =>
            source.ChainAsync(items => items.WhereAsync(item => source.Context.CancelAndTraceAsync(item, asyncPredicate), source.Context.CancellationToken));

        public static AsyncQuery<TSource> WhereAsync<TSource>(this AsyncQuery<TSource> source, Func<TSource, Task<bool>> asyncPredicate) =>
            source.ChainAsync(items => items.WhereAsync(item => source.Context.CancelAndTraceAsync(item, asyncPredicate), source.Context.CancellationToken));

        public static AsyncQuery<TResult> SelectAsync<TSource, TResult>(this Query<TSource> source, Func<TSource, Task<TResult>> asyncPredicate) =>
            source.ChainAsync(items => items.SelectAsync(item => source.Context.CancelAndTraceAsync(item, asyncPredicate), source.Context.CancellationToken));

        public static AsyncQuery<TResult> SelectAsync<TSource, TResult>(this AsyncQuery<TSource> source, Func<TSource, Task<TResult>> asyncPredicate) =>
            source.ChainAsync(items => items.SelectAsync(item => source.Context.CancelAndTraceAsync(item, asyncPredicate), source.Context.CancellationToken));

        public static AsyncQuery<TResult> SelectManyAsync<TSource, TResult>(this Query<TSource> source, Func<TSource, Task<IEnumerable<TResult>>> asyncPredicate) =>
            source.ChainAsync(items => items.SelectManyAsync(item => source.Context.CancelAndTraceAsync(item, asyncPredicate), source.Context.CancellationToken));

        public static AsyncQuery<TResult> SelectManyAsync<TSource, TResult>(this AsyncQuery<TSource> source, Func<TSource, Task<IEnumerable<TResult>>> asyncPredicate) =>
            source.ChainAsync(items => items.SelectManyAsync(item => source.Context.CancelAndTraceAsync(item, asyncPredicate), source.Context.CancellationToken));

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
    }
}
