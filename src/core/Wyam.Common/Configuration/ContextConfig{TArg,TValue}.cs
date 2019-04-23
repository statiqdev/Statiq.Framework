using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Common.Configuration
{
    public class ContextConfig<TArg, TValue> : DocumentConfig<TArg, TValue>
    {
        // Only created by the Config factory methods to ensure matching value types
        internal ContextConfig(Func<IExecutionContext, TArg, Task<TValue>> func)
            : base((_, ctx, arg) => func(ctx, arg))
        {
        }

        // Used the by the casting operators
        internal ContextConfig(Func<IDocument, IExecutionContext, TArg, Task<object>> func)
            : base(func)
        {
        }

        public override bool IsDocumentConfig => false;

        public static implicit operator ContextConfig<TArg, TValue>(TValue value) => new ContextConfig<TArg, TValue>((_, __) => Task.FromResult(value));

        public static implicit operator ContextConfig<TArg, TValue>(ContextConfig<TValue> config) => new ContextConfig<TArg, TValue>(async (doc, ctx, _) => await config.Delegate(doc, ctx));

        // These special casting operators for object variants ensure we don't accidentally "wrap" an existing ContextConfig/DocumentConfig

        public static implicit operator ContextConfig<TArg, IEnumerable<object>>(ContextConfig<TArg, TValue> contextConfig)
        {
            if (typeof(IEnumerable).IsAssignableFrom(typeof(TValue)))
            {
                return new ContextConfig<TArg, IEnumerable<object>>(async (doc, ctx, arg) => ((IEnumerable)await contextConfig.Delegate(doc, ctx, arg)).Cast<object>());
            }
            return new ContextConfig<TArg, IEnumerable<object>>(async (doc, ctx, arg) => new object[] { await contextConfig.Delegate(doc, ctx, arg) });
        }

        public static implicit operator DocumentConfig<TArg, IEnumerable<object>>(ContextConfig<TArg, TValue> contextConfig)
        {
            if (typeof(IEnumerable).IsAssignableFrom(typeof(TValue)))
            {
                return new ContextConfig<TArg, IEnumerable<object>>(async (doc, ctx, arg) => ((IEnumerable)await contextConfig.Delegate(doc, ctx, arg)).Cast<object>());
            }
            return new ContextConfig<TArg, IEnumerable<object>>(async (doc, ctx, arg) => new object[] { await contextConfig.Delegate(doc, ctx, arg) });
        }

        public static implicit operator ContextConfig<TArg, object>(ContextConfig<TArg, TValue> contextConfig) => new ContextConfig<TArg, object>(contextConfig.Delegate);

        public static implicit operator DocumentConfig<TArg, object>(ContextConfig<TArg, TValue> contextConfig) => new ContextConfig<TArg, object>(contextConfig.Delegate);

        public static implicit operator DocumentConfig<object>(ContextConfig<TArg, TValue> config) => throw new InvalidCastException($"Cannot cast from {nameof(ContextConfig<TArg, TValue>)} to {nameof(DocumentConfig<object>)}");

        public static implicit operator ContextConfig<object>(ContextConfig<TArg, TValue> config) => throw new InvalidCastException($"Cannot cast from {nameof(ContextConfig<TArg, TValue>)} to {nameof(ContextConfig<object>)}");
    }
}
