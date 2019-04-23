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
    public class DocumentConfig<TArg, TValue> : DocumentConfig
    {
        protected Func<IDocument, IExecutionContext, TArg, Task<object>> Delegate { get; }

        // Only created by the Config factory methods to ensure matching value types
        internal DocumentConfig(Func<IDocument, IExecutionContext, TArg, Task<TValue>> func)
            : this((doc, ctx, arg) => func(doc, ctx, arg).FromDerived<object, TValue>())
        {
        }

        public override bool HasArgument => true;

        protected DocumentConfig(Func<IDocument, IExecutionContext, TArg, Task<object>> func) => Delegate = func;

        // This should only be accessed via the extension method(s) that guard against null so that null coalescing operators can be used
        // See the discussion at https://github.com/dotnet/roslyn/issues/7171
        internal async Task<TValue> GetAndCacheValueAsync(IDocument document, IExecutionContext context, TArg arg, Func<TValue, TValue> transform = null) =>
            await GetAndCacheValueAsync(Delegate(document, context, arg), document, context, transform);

        public static implicit operator DocumentConfig<TArg, TValue>(TValue value) => new ContextConfig<TArg, TValue>((_, __) => Task.FromResult(value));

        public static implicit operator DocumentConfig<TArg, TValue>(DocumentConfig<TValue> config) => new DocumentConfig<TArg, TValue>(async (doc, ctx, _) => await config.Delegate(doc, ctx));

        // These special casting operators for object variants ensure we don't accidentally "wrap" an existing ContextConfig/DocumentConfig

        public static implicit operator DocumentConfig<TArg, IEnumerable<object>>(DocumentConfig<TArg, TValue> documentConfig)
        {
            if (typeof(IEnumerable).IsAssignableFrom(typeof(TValue)))
            {
                return new DocumentConfig<TArg, IEnumerable<object>>(async (doc, ctx, arg) => ((IEnumerable)await documentConfig.Delegate(doc, ctx, arg)).Cast<object>());
            }
            return new DocumentConfig<TArg, IEnumerable<object>>(async (doc, ctx, arg) => new object[] { await documentConfig.Delegate(doc, ctx, arg) });
        }

        public static implicit operator DocumentConfig<TArg, object>(DocumentConfig<TArg, TValue> documentConfig) => new DocumentConfig<TArg, object>(documentConfig.Delegate);

        public static implicit operator ContextConfig<TArg, object>(DocumentConfig<TArg, TValue> documentConfig) => new DocumentConfig<TArg, object>(documentConfig.Delegate);

        public static implicit operator DocumentConfig<object>(DocumentConfig<TArg, TValue> config) => throw new InvalidCastException($"Cannot cast from {nameof(DocumentConfig<TArg, TValue>)} to {nameof(DocumentConfig<object>)}");

        public static implicit operator ContextConfig<object>(DocumentConfig<TArg, TValue> config) => throw new InvalidCastException($"Cannot cast from {nameof(DocumentConfig<TArg, TValue>)} to {nameof(ContextConfig<object>)}");
    }
}
