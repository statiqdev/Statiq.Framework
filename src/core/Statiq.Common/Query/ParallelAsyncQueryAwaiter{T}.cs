using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections;

namespace Statiq.Common
{
    public class ParallelAsyncQueryAwaiter<T> : INotifyCompletion
    {
        private readonly Task<IEnumerable<T>> _task;
        private readonly IExecutionContext _context;
        private readonly bool _ordered;

        internal ParallelAsyncQueryAwaiter(Task<IEnumerable<T>> task, IExecutionContext context, bool ordered)
        {
            _task = task;
            _context = context;
            _ordered = ordered;
        }

        public void OnCompleted(Action continuation) => new Task(continuation).Start();

        public bool IsCompleted => _task.IsCompleted;

        public ParallelQuery<T> GetResult() => _task.Result.AsParallel(_context, _ordered);
    }
}
