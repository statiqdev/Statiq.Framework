using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Common
{
    public static class TaskExtensions
    {
        // See https://stackoverflow.com/a/15530170
        public static Task<TBase> FromDerivedAsync<TBase, TDerived>(this Task<TDerived> task)
            where TDerived : TBase
        {
            TaskCompletionSource<TBase> tcs = new TaskCompletionSource<TBase>();
            task.ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        tcs.TrySetException(t.Exception.InnerExceptions);
                    }
                    else if (t.IsCanceled)
                    {
                        tcs.TrySetCanceled();
                    }
                    else
                    {
                        tcs.TrySetResult(t.GetAwaiter().GetResult());
                    }
                },
                TaskContinuationOptions.ExecuteSynchronously);
            return tcs.Task;
        }
    }
}
