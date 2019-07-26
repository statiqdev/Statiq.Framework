using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections;

namespace Statiq.Common
{
    public static class QueryExtensions
    {
        public static Query<TSource> AsQuery<TSource>(this IEnumerable<TSource> source, IExecutionContext context) =>
            source is Query<TSource> query ? query : new Query<TSource>(source, context);

        public static Query<TSource> AsQuery<TSource>(this ParallelQuery<TSource> query) => new Query<TSource>(query, query.Context);

        public static Query<IDocument> QueryInputs(this IExecutionContext context) => context.Inputs.AsQuery(context);

        public static Query<TSource> Where<TSource>(this Query<TSource> source, Func<TSource, bool> predicate) =>
            source.Chain(items => items.Where(item => source.Context.CancelAndTrace(item, predicate)));

        public static Query<TResult> Select<TSource, TResult>(this Query<TSource> source, Func<TSource, TResult> selector) =>
            source.Chain(items => items.Select(item => source.Context.CancelAndTrace(item, selector)));

        public static Query<TResult> SelectMany<TSource, TResult>(this Query<TSource> source, Func<TSource, IEnumerable<TResult>> selector) =>
            source.Chain(items => items.SelectMany(item => source.Context.CancelAndTrace(item, selector)));

        public static void ForEach<TSource>(this Query<TSource> source, Action<TSource> action)
        {
            foreach (TSource item in source)
            {
                source.Context.CancelAndTrace(item, action);
            }
        }
    }
}
