using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.Util
{
    public static class AsyncLinqExtensions
    {
        public static async Task<IEnumerable<TResult>> SelectAsync<TSource, TResult>(
            this IEnumerable<TSource> items,
            Func<TSource, Task<TResult>> asyncSelector) =>
            await Task.WhenAll(items.Select(asyncSelector));

        public static async Task<IEnumerable<TResult>> SelectAsync<TSource, TResult>(
            this Task<IEnumerable<TSource>> items,
            Func<TSource, Task<TResult>> asyncSelector) =>
            await Task.WhenAll((await items).Select(asyncSelector));

        public static async Task<IEnumerable<TResult>> SelectAsync<TSource, TResult>(
            this Task<IEnumerable<TSource>> items,
            Func<TSource, TResult> selector) =>
            (await items).Select(selector);

        public static async Task<IEnumerable<TSource>> WhereAsync<TSource>(
            this IEnumerable<TSource> items,
            Func<TSource, Task<bool>> asyncPredicate)
        {
            IEnumerable<(TSource x, Task<bool>)> tasks = items.Select(x => (x, asyncPredicate(x)));
            await Task.WhenAll(tasks.Select(x => x.Item2));
            return tasks.Where(x => x.Item2.Result).Select(x => x.Item1);
        }

        public static async Task<IEnumerable<TSource>> WhereAsync<TSource>(
            this Task<IEnumerable<TSource>> items,
            Func<TSource, Task<bool>> asyncPredicate)
        {
            IEnumerable<(TSource x, Task<bool>)> tasks = (await items).Select(x => (x, asyncPredicate(x)));
            await Task.WhenAll(tasks.Select(x => x.Item2));
            return tasks.Where(x => x.Item2.Result).Select(x => x.Item1);
        }

        public static async Task<IEnumerable<TSource>> WhereAsync<TSource>(
            this Task<IEnumerable<TSource>> items,
            Func<TSource, bool> predicate) =>
            (await items).Where(predicate);

        public static async Task<TSource> FirstOrDefaultAsync<TSource>(
            this Task<IEnumerable<TSource>> items) =>
            (await items).FirstOrDefault();

        public static async Task<TSource[]> ToArrayAsync<TSource>(
            this Task<IEnumerable<TSource>> items) =>
            (await items).ToArray();
    }
}
