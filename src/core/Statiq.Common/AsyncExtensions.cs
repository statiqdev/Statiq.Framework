using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statiq.Common
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
            Func<TSource, Task<TResult>> asyncSelector)
        {
            // We need to iterate the items one-by-one and wait on each task as it's provided
            // Otherwise the tasks will all be created AND STARTED during the iteration, potentially in parallel
            // In other words, creating/getting a bunch of tasks and then calling Task.WhenAll() to wait on them _may_ run them in parallel
            List<TResult> results = new List<TResult>();
            foreach (TSource item in items)
            {
                Task<TResult> task = asyncSelector(item);  // Task may already be started by the time we get it
                if (task != null)
                {
                    results.Add(await task);
                }
            }
            return results;
        }

        public static async Task<IEnumerable<TResult>> SelectManyAsync<TSource, TResult>(
            this IEnumerable<TSource> items,
            Func<TSource, Task<IEnumerable<TResult>>> asyncSelector) =>
            (await items.SelectAsync(asyncSelector)).SelectMany(x => x);

        public static async Task<IEnumerable<TSource>> WhereAsync<TSource>(
            this IEnumerable<TSource> items,
            Func<TSource, Task<bool>> asyncPredicate)
        {
            List<TSource> results = new List<TSource>();
            foreach (TSource item in items)
            {
                Task<bool> task = asyncPredicate(item);  // Task may already be started by the time we get it
                if (await task)
                {
                    results.Add(item);
                }
            }
            return results;
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
            return default;
        }

        public static async Task<List<T>> ToListAsync<T>(this Task<IEnumerable<T>> task) => (await task).ToList();

        public static async Task<T[]> ToArrayAsync<T>(this Task<IEnumerable<T>> task) => (await task).ToArray();

        public static async Task<T> SingleAsync<T>(this Task<IEnumerable<T>> task) => (await task).Single();

        public static async Task<T> SingleAsync<T>(this Task<IReadOnlyList<T>> task) => (await task).Single();

        // See https://stackoverflow.com/a/15530170
        public static Task<TBase> FromDerived<TBase, TDerived>(this Task<TDerived> task)
            where TDerived : TBase
        {
            TaskCompletionSource<TBase> tcs = new TaskCompletionSource<TBase>();
            task.ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        tcs.TrySetException(t.Exception.InnerExceptions);
                    }
                    else if (t.IsCanceled)
                    {
                        tcs.TrySetCanceled();
                    }
                    else
                    {
                        tcs.TrySetResult(t.Result);
                    }
                },
                TaskContinuationOptions.ExecuteSynchronously);
            return tcs.Task;
        }
    }
}
