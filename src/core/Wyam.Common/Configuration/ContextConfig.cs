using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Common.Configuration
{
    public class ContextConfigNew : DocumentConfigNew
    {
        internal ContextConfigNew(Func<IExecutionContext, Task<object>> func)
            : base((_, ctx) => func(ctx))
        {
        }

        public Task<object> GetValueAsync(IExecutionContext context) => GetValueAsync(null, context);

        public async Task<T> GetValueAsync<T>(IExecutionContext context, string errorDetails = null)
        {
            object value = await GetValueAsync(null, context);
            if (!context.TryConvert(value, out T result))
            {
                throw new InvalidOperationException(
                    $"Could not convert from type {value?.GetType().Name ?? "null"} to type {typeof(T).Name}{Config.GetErrorDetails(errorDetails)}");
            }
            return result;
        }

        public async Task<T> TryGetValueAsync<T>(IExecutionContext context)
        {
            object value = await GetValueAsync(null, context);
            return context.TryConvert(value, out T result) ? result : default;
        }

        public override bool IsDocumentConfig => false;
    }
}
