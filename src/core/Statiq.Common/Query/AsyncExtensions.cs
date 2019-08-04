using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// General extensions for LINQ-like async operations on arbitrary objects.
    /// </summary>
    public static class AsyncExtensions
    {
        // https://devblogs.microsoft.com/pfxteam/implementing-a-simple-foreachasync-part-2/

        public static async Task<IEnumerable<TResult>> SelectAsync<TSource, TResult>(
            this IEnumerable<TSource> items,
            Func<TSource, Task<TResult>> asyncSelector,
            CancellationToken cancellationToken)
        {
            // We need to iterate the items one-by-one and wait on each task as it's provided
            // Otherwise the tasks will all be created AND STARTED during the iteration, potentially in parallel
            // In other words, creating/getting a bunch of tasks and then calling Task.WhenAll() to wait on them _may_ run them in parallel
            List<TResult> results = new List<TResult>();
            foreach (TSource item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Task<TResult> task = asyncSelector(item);  // Task may already be started by the time we get it
                if (task != null)
                {
                    results.Add(await task);
                }
            }
            return results;
        }

        public static Task<IEnumerable<TResult>> SelectAsync<TSource, TResult>(
            this IEnumerable<TSource> items,
            Func<TSource, Task<TResult>> asyncSelector) =>
                items.SelectAsync(asyncSelector, default);

        public static async Task<IEnumerable<TResult>> SelectManyAsync<TSource, TResult>(
            this IEnumerable<TSource> items,
            Func<TSource, Task<IEnumerable<TResult>>> asyncSelector,
            CancellationToken cancellationToken) =>
                (await items.SelectAsync(asyncSelector, cancellationToken)).SelectMany(x => x);

        public static Task<IEnumerable<TResult>> SelectManyAsync<TSource, TResult>(
            this IEnumerable<TSource> items,
            Func<TSource, Task<IEnumerable<TResult>>> asyncSelector) =>
                items.SelectManyAsync(asyncSelector, default);

        public static async Task<IEnumerable<TSource>> WhereAsync<TSource>(
            this IEnumerable<TSource> items,
            Func<TSource, Task<bool>> asyncPredicate,
            CancellationToken cancellationToken)
        {
            List<TSource> results = new List<TSource>();
            foreach (TSource item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Task<bool> task = asyncPredicate(item);  // Task may already be started by the time we get it
                if (task != null && await task)
                {
                    results.Add(item);
                }
            }
            return results;
        }

        public static Task<IEnumerable<TSource>> WhereAsync<TSource>(
            this IEnumerable<TSource> items,
            Func<TSource, Task<bool>> asyncPredicate) =>
                items.WhereAsync(asyncPredicate, default);

        public static async Task ForEachAsync<TSource>(
            this IEnumerable<TSource> items,
            Func<TSource, Task> asyncFunc,
            CancellationToken cancellationToken)
        {
            // We need to iterate the items one-by-one and wait on each task as it's provided
            // Otherwise the tasks will all be created AND STARTED during the iteration, potentially in parallel
            // In other words, creating/getting a bunch of tasks and then calling Task.WhenAll() to wait on them _may_ run them in parallel
            foreach (TSource item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Task task = asyncFunc(item);  // Task may already be started by the time we get it
                if (task != null)
                {
                    await task;
                }
            }
        }

        public static Task ForEachAsync<TSource>(
            this IEnumerable<TSource> items,
            Func<TSource, Task> asyncFunc) =>
                items.ForEachAsync(asyncFunc, default);

        public static async Task<bool> AnyAsync<TSource>(
            this IEnumerable<TSource> items,
            Func<TSource, Task<bool>> asyncPredicate,
            CancellationToken cancellationToken)
        {
            foreach (TSource item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Task<bool> task = asyncPredicate(item);  // Task may already be started by the time we get it
                if (task != null && await task)
                {
                    return true;
                }
            }
            return false;
        }

        public static Task<bool> AnyAsync<TSource>(
            this IEnumerable<TSource> items,
            Func<TSource, Task<bool>> asyncPredicate) =>
                items.AnyAsync(asyncPredicate, default);

        public static async Task<TItem> FindAsync<TItem>(
            this IEnumerable<TItem> items,
            Func<TItem, Task<bool>> predicate,
            CancellationToken cancellationToken)
        {
            foreach (TItem item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Task<bool> task = predicate(item);
                if (task != null && await task)
                {
                    return item;
                }
            }
            return default;
        }

        public static Task<TItem> FindAsync<TItem>(
            this IEnumerable<TItem> items,
            Func<TItem, Task<bool>> predicate) =>
                items.FindAsync(predicate, default);

        public static Task<List<T>> ToListAsync<T>(this Task<IEnumerable<T>> task) => task.ThenAsync(x => x.ToList());

        public static Task<T[]> ToArrayAsync<T>(this Task<IEnumerable<T>> task) => task.ThenAsync(x => x.ToArray());

        public static Task<ImmutableArray<T>> ToImmutableArrayAsync<T>(this Task<IEnumerable<T>> task) => task.ThenAsync(x => x.ToImmutableArray());

        public static Task<T> SingleAsync<T>(this Task<IEnumerable<T>> task) => task.ThenAsync(x => x.Single());

        public static Task<T> SingleAsync<T>(this Task<ImmutableArray<T>> task) => task.ThenAsync(x => x.Single());

        public static Task<T> SingleOrDefaultAsync<T>(this Task<IEnumerable<T>> task) => task.ThenAsync(x => x.SingleOrDefault());

        public static Task<T> SingleOrDefaultAsync<T>(this Task<ImmutableArray<T>> task) => task.ThenAsync(x => x.SingleOrDefault());

        public static async Task<TResult> ThenAsync<T, TResult>(this Task<T> task, Func<T, Task<TResult>> func) => await func(await task);

        public static async Task<TResult> ThenAsync<T, TResult>(this Task<T> task, Func<T, TResult> func) => func(await task);

        public static Task<IEnumerable<TDocument>> AsTask<TDocument>(this IEnumerable<TDocument> documents)
            where TDocument : IDocument =>
                Task.FromResult(documents);

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
