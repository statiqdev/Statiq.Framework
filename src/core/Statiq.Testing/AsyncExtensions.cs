using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// Convenience extensions to handle sync-over-async scenarios in tests. These extensions should
    /// not be used in production code due to being slower and hacky.
    /// </summary>
    public static class AsyncExtensions
    {
        public static Task<T> SingleAsync<T>(this Task<IEnumerable<T>> task) => task.ThenAsync(x => x.Single());

        public static Task<T> SingleAsync<T>(this Task<ImmutableArray<T>> task) => task.ThenAsync(x => x.Single());

        public static async Task<TResult> ThenAsync<T, TResult>(this Task<T> task, Func<T, TResult> func) => func(await task);
    }
}
