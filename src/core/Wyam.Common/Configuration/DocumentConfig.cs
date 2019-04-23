using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Util;

namespace Wyam.Common.Configuration
{
    public abstract class DocumentConfig
    {
        private readonly SemaphoreSlim _cacheMutex = new SemaphoreSlim(1);
        private WeakReference<IExecutionContext> _cacheContext = null;
        private object _cacheValue;

        public virtual bool IsDocumentConfig => true;

        public virtual bool HasArgument => false;

        protected async Task<TValue> GetAndCacheValueAsync<TValue>(Task<object> valueDelegate, IDocument document, IExecutionContext context, Func<TValue, TValue> transform = null)
        {
            // We can potentially cache the value if this was called with a null document
            // or is not a document config (in which case the document isn't used in the delegate)
            // and does not have an argument (because the argument could change even if the context and document does not)
            if ((document == null || !IsDocumentConfig) && !HasArgument)
            {
                await _cacheMutex.WaitAsync();
                try
                {
                    // Do we already have a cached value?
                    if (_cacheContext == null || !_cacheContext.TryGetTarget(out IExecutionContext cacheContext) || cacheContext != context)
                    {
                        // Cache miss, get the value
                        _cacheContext = new WeakReference<IExecutionContext>(context);
                        _cacheValue = await GetValueAsync();
                    }
                    return (TValue)_cacheValue;
                }
                finally
                {
                    _cacheMutex.Release();
                }
            }
            return await GetValueAsync();

            async Task<TValue> GetValueAsync()
            {
                TValue value = (TValue)await valueDelegate;
                return transform == null ? value : transform(value);
            }
        }
    }
}
