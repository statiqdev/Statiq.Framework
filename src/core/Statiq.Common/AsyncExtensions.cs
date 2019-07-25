using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static class AsyncExtensions
    {
        // https://devblogs.microsoft.com/pfxteam/implementing-a-simple-foreachasync-part-2/

        /// <summary>
        /// Invokes an async selector in parallel.
        /// </summary>
        /// <remarks>
        /// This method will preserve ordering.
        /// </remarks>
        /// <typeparam name="TSource">The type of the elements of <paramref name="items"/>.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by <paramref name="asyncSelector"/>.</typeparam>
        /// <param name="items">The items to select from.</param>
        /// <param name="asyncSelector">The selector to apply on each element of <paramref name="items"/>.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
        /// <returns>The result of invoking the <paramref name="asyncSelector"/> on each element of <paramref name="items"/>.</returns>
        public static async Task<IEnumerable<TResult>> ParallelSelectAsync<TSource, TResult>(
            this IEnumerable<TSource> items,
            Func<TSource, Task<TResult>> asyncSelector,
            CancellationToken cancellationToken) =>
                await Task.WhenAll(items.Select(x => Task.Run(() => asyncSelector(x), cancellationToken)));

        /// <summary>
        /// Invokes an async selector in parallel.
        /// </summary>
        /// <remarks>
        /// This method will preserve ordering.
        /// </remarks>
        /// <typeparam name="TSource">The type of the elements of <paramref name="items"/>.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by <paramref name="asyncSelector"/>.</typeparam>
        /// <param name="items">The items to select from.</param>
        /// <param name="asyncSelector">The selector to apply on each element of <paramref name="items"/>.</param>
        /// <returns>The result of invoking the <paramref name="asyncSelector"/> on each element of <paramref name="items"/>.</returns>
        public static Task<IEnumerable<TResult>> ParallelSelectAsync<TSource, TResult>(
            this IEnumerable<TSource> items,
            Func<TSource, Task<TResult>> asyncSelector) =>
                items.ParallelSelectAsync(asyncSelector, default);

        /// <summary>
        /// Invokes an async selector that returns multiple results in parallel.
        /// </summary>
        /// <remarks>
        /// This method will preserve ordering.
        /// </remarks>
        /// <typeparam name="TSource">The type of the elements of <paramref name="items"/>.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by <paramref name="asyncSelector"/>.</typeparam>
        /// <param name="items">The items to select from.</param>
        /// <param name="asyncSelector">The selector to apply on each element of <paramref name="items"/>.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
        /// <returns>The result of invoking the <paramref name="asyncSelector"/> on each element of <paramref name="items"/>.</returns>
        public static async Task<IEnumerable<TResult>> ParallelSelectManyAsync<TSource, TResult>(
            this IEnumerable<TSource> items,
            Func<TSource, Task<IEnumerable<TResult>>> asyncSelector,
            CancellationToken cancellationToken) =>
                (await Task.WhenAll(items.Select(x => Task.Run(() => asyncSelector(x), cancellationToken)))).SelectMany(x => x);

        /// <summary>
        /// Invokes an async selector that returns multiple results in parallel.
        /// </summary>
        /// <remarks>
        /// This method will preserve ordering.
        /// </remarks>
        /// <typeparam name="TSource">The type of the elements of <paramref name="items"/>.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by <paramref name="asyncSelector"/>.</typeparam>
        /// <param name="items">The items to select from.</param>
        /// <param name="asyncSelector">The selector to apply on each element of <paramref name="items"/>.</param>
        /// <returns>The result of invoking the <paramref name="asyncSelector"/> on each element of <paramref name="items"/>.</returns>
        public static Task<IEnumerable<TResult>> ParallelSelectManyAsync<TSource, TResult>(
            this IEnumerable<TSource> items,
            Func<TSource, Task<IEnumerable<TResult>>> asyncSelector) =>
                items.ParallelSelectManyAsync(asyncSelector, default);

        public static async Task<IEnumerable<TSource>> ParallelWhereAsync<TSource>(
            this IEnumerable<TSource> items,
            Func<TSource, Task<bool>> asyncPredicate,
            CancellationToken cancellationToken) =>
                (await Task.WhenAll(items.Select(x => Task.Run(async () => (x, await asyncPredicate(x)), cancellationToken))))
                    .Where(x => x.Item2)
                    .Select(x => x.Item1);

        public static Task<IEnumerable<TSource>> ParallelWhereAsync<TSource>(
            this IEnumerable<TSource> items,
            Func<TSource, Task<bool>> asyncPredicate) =>
                items.ParallelWhereAsync(asyncPredicate, default);

        public static Task ParallelForEachAsync<TSource>(
            this IEnumerable<TSource> items,
            Func<TSource, Task> asyncFunc,
            CancellationToken cancellationToken) =>
                Task.WhenAll(items.Select(x => Task.Run(() => asyncFunc(x), cancellationToken)));

        public static Task ParallelForEachAsync<TSource>(
            this IEnumerable<TSource> items,
            Func<TSource, Task> asyncFunc) =>
                items.ParallelForEachAsync(asyncFunc, default);

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
