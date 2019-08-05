using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections;

namespace Statiq.Common
{
    public static class QueryLinqExtensions
    {
        public static Query<TSource> Where<TSource>(this Query<TSource> source, Func<TSource, bool> predicate) =>
            source.Then(items => items.Where(item => source.Context.CancelAndTrace(item, predicate)));

        public static Query<TResult> Select<TSource, TResult>(this Query<TSource> source, Func<TSource, TResult> selector) =>
            source.Then(items => items.Select(item => source.Context.CancelAndTrace(item, selector)));

        public static Query<TResult> SelectMany<TSource, TResult>(this Query<TSource> source, Func<TSource, IEnumerable<TResult>> selector) =>
            source.Then(items => items.SelectMany(item => source.Context.CancelAndTrace(item, selector)));

        public static void ForEach<TSource>(this Query<TSource> source, Action<TSource> action)
        {
            foreach (TSource item in source)
            {
                source.Context.CancelAndTrace(item, action);
            }
        }
    }
}
