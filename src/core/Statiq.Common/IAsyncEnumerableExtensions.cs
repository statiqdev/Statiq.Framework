using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static class IAsyncEnumerableExtensions
    {
        public static async ValueTask<ImmutableArray<TSource>> ToImmutableArrayAsync<TSource>(
            this IAsyncEnumerable<TSource> source,
            CancellationToken cancellationToken = default) =>
            (await source.ToArrayAsync(cancellationToken)).ToImmutableArray();
    }
}
