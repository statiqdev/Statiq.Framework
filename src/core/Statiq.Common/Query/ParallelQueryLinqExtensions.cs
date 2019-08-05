using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections;

namespace Statiq.Common
{
    public static class ParallelQueryLinqExtensions
    {
        public static ParallelQuery<TSource> Where<TSource>(this ParallelQuery<TSource> source, Func<TSource, bool> predicate) =>
            source.Then(items => items.Where(item => source.Context.CancelAndTrace(item, predicate)));

        public static ParallelQuery<TResult> Select<TSource, TResult>(this ParallelQuery<TSource> source, Func<TSource, TResult> selector) =>
            source.Then(items => items.Select(item => source.Context.CancelAndTrace(item, selector)));

        public static ParallelQuery<TResult> SelectMany<TSource, TResult>(this ParallelQuery<TSource> source, Func<TSource, IEnumerable<TResult>> selector) =>
            source.Then(items => items.SelectMany(item => source.Context.CancelAndTrace(item, selector)));

        public static void ForEach<TSource>(this ParallelQuery<TSource> source, Action<TSource> action) =>
            System.Threading.Tasks.Parallel.ForEach(source, new ParallelOptions { CancellationToken = source.Context.CancellationToken }, item => source.Context.CancelAndTrace(item, action));
    }
}
