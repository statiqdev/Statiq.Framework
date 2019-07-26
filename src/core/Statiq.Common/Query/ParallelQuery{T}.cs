using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections;

namespace Statiq.Common
{
    public class ParallelQuery<T> : IEnumerable<T>
    {
        private readonly System.Linq.ParallelQuery<T> _query;

        internal ParallelQuery(System.Linq.ParallelQuery<T> query, IExecutionContext context)
        {
            _query = query;
            Context = context;
        }

        internal IExecutionContext Context { get; }

        internal ParallelQuery<TResult> Chain<TResult>(Func<System.Linq.ParallelQuery<T>, System.Linq.ParallelQuery<TResult>> func) =>
            new ParallelQuery<TResult>(func(_query), Context);

        internal ParallelAsyncQuery<TResult> ChainAsync<TResult>(Func<IEnumerable<T>, Task<IEnumerable<TResult>>> asyncFunc) =>
            new ParallelAsyncQuery<TResult>(asyncFunc(_query), Context, _query is OrderedParallelQuery<T>);

        public IEnumerator<T> GetEnumerator() => _query.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
