using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections;

namespace Statiq.Common
{
    public class ParallelAsyncQuery<T>
    {
        private readonly bool _ordered;

        internal ParallelAsyncQuery(Task<IEnumerable<T>> task, IExecutionContext context, bool ordered)
        {
            Task = task;
            Context = context;
            _ordered = ordered;
        }

        public Task<IEnumerable<T>> Task { get; }

        internal IExecutionContext Context { get; }

        internal ParallelAsyncQuery<TResult> ChainAsync<TResult>(Func<IEnumerable<T>, Task<IEnumerable<TResult>>> asyncFunc) =>
            new ParallelAsyncQuery<TResult>(GetTaskAsync(this, asyncFunc), Context, _ordered);

        private static async Task<IEnumerable<TResult>> GetTaskAsync<TResult>(ParallelAsyncQuery<T> query, Func<IEnumerable<T>, Task<IEnumerable<TResult>>> asyncFunc) => await asyncFunc(await query);

        public ParallelAsyncQueryAwaiter<T> GetAwaiter() => new ParallelAsyncQueryAwaiter<T>(Task, Context, _ordered);
    }
}
