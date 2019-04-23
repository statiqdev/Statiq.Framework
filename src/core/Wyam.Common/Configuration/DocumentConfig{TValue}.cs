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
    /// <typeparam name="TValue">The value type for this config data.</typeparam>
    public class DocumentConfig<TValue> : DocumentConfig
    {
        internal Func<IDocument, IExecutionContext, Task<object>> Delegate { get; }

        internal DocumentConfig(Func<IDocument, IExecutionContext, Task<TValue>> func)
            : this((doc, ctx) => func(doc, ctx).FromDerived<object, TValue>())
        {
        }

        internal DocumentConfig(Func<IDocument, IExecutionContext, Task<object>> func) => Delegate = func;

        // This should only be accessed via the extension method(s) that guard against null so that null coalescing operators can be used
        // See the discussion at https://github.com/dotnet/roslyn/issues/7171
        internal async Task<TValue> GetAndCacheValueAsync(IDocument document, IExecutionContext context, Func<TValue, TValue> transform = null) =>
            await GetAndCacheValueAsync(Delegate(document, context), document, context, transform);

        public static implicit operator DocumentConfig<TValue>(TValue value) => new ContextConfig<TValue>(_ => Task.FromResult(value));

        // These special casting operators for object variants ensure we don't accidentally "wrap" an existing ContextConfig/DocumentConfig

        public static implicit operator DocumentConfig<IEnumerable<object>>(DocumentConfig<TValue> documentConfig)
        {
            if (typeof(IEnumerable).IsAssignableFrom(typeof(TValue)))
            {
                return new DocumentConfig<IEnumerable<object>>(async (doc, ctx) => ((IEnumerable)await documentConfig.Delegate(doc, ctx)).Cast<object>());
            }
            return new DocumentConfig<IEnumerable<object>>(async (doc, ctx) => new object[] { await documentConfig.Delegate(doc, ctx) });
        }

        public static implicit operator DocumentConfig<object>(DocumentConfig<TValue> documentConfig) => new DocumentConfig<object>(documentConfig.Delegate);
    }
}
