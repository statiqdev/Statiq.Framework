using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections;

namespace Statiq.Common
{
    public class Query<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> _items;

        internal Query(IEnumerable<T> items, IExecutionContext context)
        {
            _items = items;
            Context = context;
        }

        internal IExecutionContext Context { get; }

        internal Query<TResult> Chain<TResult>(Func<IEnumerable<T>, IEnumerable<TResult>> func) =>
            new Query<TResult>(func(_items), Context);

        internal AsyncQuery<TResult> ChainAsync<TResult>(Func<IEnumerable<T>, Task<IEnumerable<TResult>>> asyncFunc) =>
            new AsyncQuery<TResult>(asyncFunc(_items), Context);

        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
