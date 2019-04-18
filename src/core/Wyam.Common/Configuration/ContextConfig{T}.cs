using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Common.Configuration
{
    public class ContextConfig<T> : DocumentConfig<T>, IContextConfig
    {
        // Only created by the Config factory methods to ensure matching value types
        internal ContextConfig(Func<IExecutionContext, Task<T>> func)
            : base((_, ctx) => func(ctx))
        {
        }

        // Used the by the casting operators
        internal ContextConfig(Func<IDocument, IExecutionContext, Task<object>> func)
            : base(func)
        {
        }

        // This forces a cast to DocumentConfig<T> to ensure nested DocumentConfig<T> objects get unwrapped
        // then casts to the desired ContextConfig<T>
        public static implicit operator ContextConfig<T>(T value)
        {
            if (value is IContextConfig contextConfig)
            {
                if (!typeof(T).IsAssignableFrom(contextConfig.ValueType))
                {
                    throw new InvalidCastException("Could not cast value type of configuration delegate");
                }
                return new ContextConfig<T>(contextConfig.Delegate);
            }
            if (value is IDocumentConfig)
            {
                throw new InvalidCastException("Can not cast a document config to a context config");
            }
            return new ContextConfig<T>(_ => Task.FromResult(value));
        }

        public Task<T> GetValueAsync(IExecutionContext context) => GetValueAsync(null, context);

        public override bool IsDocumentConfig => false;
    }
}
