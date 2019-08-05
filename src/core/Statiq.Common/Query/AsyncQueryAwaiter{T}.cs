using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections;

namespace Statiq.Common
{
    public class AsyncQueryAwaiter<T> : INotifyCompletion
    {
        private readonly Task<IEnumerable<T>> _task;
        private readonly IExecutionContext _context;

        internal AsyncQueryAwaiter(Task<IEnumerable<T>> task, IExecutionContext context)
        {
            _task = task;
            _context = context;
        }

        public void OnCompleted(Action continuation) => new Task(continuation).Start();

        public bool IsCompleted => _task.IsCompleted;

        public Query<T> GetResult() => _task.Result.Query(_context);
    }
}
