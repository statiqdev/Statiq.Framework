using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Common.Configuration
{
    public static class DocumentConfigExtensions
    {
        public static async Task<T> GetValueAsync<T>(this DocumentConfig<object> config, IDocument document, IExecutionContext context, string errorDetails = null)
        {
            object value = await config.GetValueAsync(document, context);
            if (!context.TryConvert(value, out T result))
            {
                throw new InvalidOperationException(
                    $"Could not convert from type {value?.GetType().Name ?? "null"} to type {typeof(T).Name}{Config.GetErrorDetails(errorDetails)}");
            }
            return result;
        }

        public static async Task<T> TryGetValueAsync<T>(this DocumentConfig<object> config, IDocument document, IExecutionContext context)
        {
            object value = await config.GetValueAsync(document, context);
            return context.TryConvert(value, out T result) ? result : default;
        }
    }
}
