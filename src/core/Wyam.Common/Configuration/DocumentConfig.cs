using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Common.Configuration
{
    // Use the strongly typed version in most cases
    // Only use the untyped configs when you might need to convert the result to different types
    public class DocumentConfigNew : DocumentConfig<object>
    {
        internal DocumentConfigNew(Func<IDocument, IExecutionContext, Task<object>> func)
            : base(func)
        {
        }

        public async Task<T> GetValueAsync<T>(IDocument document, IExecutionContext context, string errorDetails = null)
        {
            object value = await GetValueAsync(document, context);
            if (!context.TryConvert(value, out T result))
            {
                throw new InvalidOperationException(
                    $"Could not convert from type {value?.GetType().Name ?? "null"} to type {typeof(T).Name}{Config.GetErrorDetails(errorDetails)}");
            }
            return result;
        }

        public async Task<T> TryGetValueAsync<T>(IDocument document, IExecutionContext context)
        {
            object value = await GetValueAsync(document, context);
            return context.TryConvert(value, out T result) ? result : default;
        }
    }
}
