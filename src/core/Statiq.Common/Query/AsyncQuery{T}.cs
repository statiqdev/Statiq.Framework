using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections;

namespace Statiq.Common
{
    public class AsyncQuery<T>
    {
        internal AsyncQuery(Task<IEnumerable<T>> task, IExecutionContext context)
        {
            Task = task;
            Context = context;
        }

        public Task<IEnumerable<T>> Task { get; }

        internal IExecutionContext Context { get; }

        internal AsyncQuery<TResult> ChainAsync<TResult>(Func<IEnumerable<T>, Task<IEnumerable<TResult>>> asyncFunc) =>
            new AsyncQuery<TResult>(GetTaskAsync(this, asyncFunc), Context);

        private static async Task<IEnumerable<TResult>> GetTaskAsync<TResult>(AsyncQuery<T> query, Func<IEnumerable<T>, Task<IEnumerable<TResult>>> asyncFunc) => await asyncFunc(await query);

        public AsyncQueryAwaiter<T> GetAwaiter() => new AsyncQueryAwaiter<T>(Task, Context);
    }
}
