using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.Util
{
    public static class AsyncExtensions
    {
        // https://devblogs.microsoft.com/pfxteam/implementing-a-simple-foreachasync-part-2/
        public static async Task<IEnumerable<TResult>> ParallelSelectAsync<TSource, TResult>(
           this IEnumerable<TSource> items,
           Func<TSource, Task<TResult>> asyncSelector) =>
           await Task.WhenAll(items.Select(x => Task.Run(() => asyncSelector(x))));

        public static async Task<IEnumerable<TResult>> ParallelSelectManyAsync<TSource, TResult>(
           this IEnumerable<TSource> items,
           Func<TSource, Task<IEnumerable<TResult>>> asyncSelector) =>
           (await Task.WhenAll(items.Select(x => Task.Run(() => asyncSelector(x))))).SelectMany(x => x);

        public static async Task<IEnumerable<TSource>> ParallelWhereAsync<TSource>(
          this IEnumerable<TSource> items,
          Func<TSource, Task<bool>> asyncPredicate) =>
            (await Task.WhenAll(items.Select(x => Task.Run(async () => (x, await asyncPredicate(x))))))
                .Where(x => x.Item2)
                .Select(x => x.Item1);

        public static Task ParallelForEachAsync<TSource>(
           this IEnumerable<TSource> items,
           Func<TSource, Task> asyncFunc) =>
           Task.WhenAll(items.Select(x => Task.Run(() => asyncFunc(x))));

        public static async Task<IEnumerable<TResult>> SelectAsync<TSource, TResult>(
            this IEnumerable<TSource> items,
            Func<TSource, Task<TResult>> asyncSelector) =>
            await Task.WhenAll(items.Select(asyncSelector));

        public static async Task<IEnumerable<TResult>> SelectManyAsync<TSource, TResult>(
            this IEnumerable<TSource> items,
            Func<TSource, Task<IEnumerable<TResult>>> asyncSelector) =>
            (await Task.WhenAll(items.Select(asyncSelector))).SelectMany(x => x);

        public static async Task<IEnumerable<TSource>> WhereAsync<TSource>(
            this IEnumerable<TSource> items,
            Func<TSource, Task<bool>> asyncPredicate)
        {
            IEnumerable<(TSource x, Task<bool>)> tasks = items.Select(x => (x, asyncPredicate(x)));
            await Task.WhenAll(tasks.Select(x => x.Item2));
            return tasks.Where(x => x.Item2.Result).Select(x => x.Item1);
        }

        public static async Task<bool> AnyAsync<TSource>(
            this IEnumerable<TSource> items,
            Func<TSource, Task<bool>> asyncPredicate)
        {
            foreach (TSource item in items)
            {
                if (await asyncPredicate(item))
                {
                    return true;
                }
            }
            return false;
        }

        public static async Task<TItem> FindAsync<TItem>(
            this List<TItem> list,
            Func<TItem, Task<bool>> predicate)
        {
            foreach (TItem item in list)
            {
                if (await predicate(item))
                {
                    return item;
                }
            }
            return default(TItem);
        }
    }
}
