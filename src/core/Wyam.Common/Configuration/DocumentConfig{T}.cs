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
    /// <summary>
    /// A union configuration value that can be either a delegate
    /// that uses a document and context or a simple value. Use the factory methods
    /// in the <see cref="Config"/> class to create one. Instances can also be created
    /// through implicit casting from the value type. Note that due to overload ambiguity,
    /// if a value type of object is used, then all overloads should also be <see cref="DocumentConfig{T}"/>.
    /// </summary>
    /// <typeparam name="T">The value type for this config data.</typeparam>
    public class DocumentConfig<T>
    {
        private readonly SemaphoreSlim _cacheMutex = new SemaphoreSlim(1);
        private WeakReference<IExecutionContext> _cacheContext = null;
        private T _cacheValue;

        protected Func<IDocument, IExecutionContext, Task<object>> Delegate { get; }

        // Only created by the Config factory methods to ensure matching value types
        internal DocumentConfig(Func<IDocument, IExecutionContext, Task<T>> func)
            : this((doc, ctx) => func(doc, ctx).FromDerived<object, T>())
        {
        }

        protected DocumentConfig(Func<IDocument, IExecutionContext, Task<object>> func) => Delegate = func;

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
                T value = (T)await Delegate(document, context);
                return transform == null ? value : transform(value);
            }
        }

        public static implicit operator DocumentConfig<T>(T value) => new ContextConfig<T>(_ => Task.FromResult(value));

        // These special casting operators for object variants ensure we don't accidentally "wrap" an existing ContextConfig/DocumentConfig

        public static implicit operator DocumentConfig<IEnumerable<object>>(DocumentConfig<T> documentConfig)
        {
            if (typeof(IEnumerable).IsAssignableFrom(typeof(T)))
            {
                return new DocumentConfig<IEnumerable<object>>(async (doc, ctx) => ((IEnumerable)await documentConfig.Delegate(doc, ctx)).Cast<object>());
            }
            return new DocumentConfig<IEnumerable<object>>(async (doc, ctx) => new object[] { await documentConfig.Delegate(doc, ctx) });
        }

        public static implicit operator DocumentConfig<object>(DocumentConfig<T> documentConfig) => new DocumentConfig<object>(documentConfig.Delegate);
    }
}
