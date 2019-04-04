using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Common.Configuration
{
    public class DocumentConfig<T>
    {
        private readonly Func<IDocument, IExecutionContext, Task<T>> _delegate;

        private readonly SemaphoreSlim _cacheMutex = new SemaphoreSlim(1);
        private WeakReference<IExecutionContext> _cacheContext = null;
        private T _cacheValue;

        internal DocumentConfig(Func<IDocument, IExecutionContext, Task<T>> func) => _delegate = func;

        public static implicit operator DocumentConfig<T>(T value) => new ContextConfig<T>(_ => Task.FromResult(value));

        public virtual bool IsDocumentConfig => true;

        public async Task<T> GetValueAsync(IDocument document, IExecutionContext context, Func<T, T> transform = null)
        {
            // We can potentially cache the value if this was called with a null document
            // or is not a document config (in which case the document isn't used in the delegate)
            if (document == null || !IsDocumentConfig)
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
                    return _cacheValue;
                }
                finally
                {
                    _cacheMutex.Release();
                }
            }
            return await GetValueAsync();

            async Task<T> GetValueAsync()
            {
                T value = await _delegate(document, context);
                return transform == null ? value : transform(value);
            }
        }
    }
}
