using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Util;

namespace Wyam.Common.Configuration
{
    public class DocumentConfig<T> : IDocumentConfig
    {
        private readonly Func<IDocument, IExecutionContext, Task<object>> _delegate;

        private readonly SemaphoreSlim _cacheMutex = new SemaphoreSlim(1);
        private WeakReference<IExecutionContext> _cacheContext = null;
        private T _cacheValue;

        // Only created by the Config factory methods to ensure matching value types
        internal DocumentConfig(Func<IDocument, IExecutionContext, Task<T>> func)
            : this((doc, ctx) => func(doc, ctx).FromDerived<object, T>())
        {
        }

        protected DocumentConfig(Func<IDocument, IExecutionContext, Task<object>> func) => _delegate = func;

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
                T value = (T)await _delegate(document, context);
                return transform == null ? value : transform(value);
            }
        }

        public static implicit operator DocumentConfig<T>(T value)
        {
            if (value is IContextConfig contextConfig)
            {
                if (!typeof(T).IsAssignableFrom(contextConfig.ValueType))
                {
                    throw new InvalidCastException("Could not cast value type of configuration delegate");
                }
                return new ContextConfig<T>(contextConfig.Delegate);
            }
            if (value is IDocumentConfig documentConfig)
            {
                if (!typeof(T).IsAssignableFrom(documentConfig.ValueType))
                {
                    throw new InvalidCastException("Could not cast value type of configuration delegate");
                }
                return new DocumentConfig<T>(documentConfig.Delegate);
            }
            return new ContextConfig<T>(_ => Task.FromResult(value));
        }

        Type IDocumentConfig.ValueType => typeof(T);

        Func<IDocument, IExecutionContext, Task<object>> IDocumentConfig.Delegate => _delegate;
    }
}
