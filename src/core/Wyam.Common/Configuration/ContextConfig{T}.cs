using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Common.Configuration
{
    public class ContextConfig<T> : DocumentConfig<T>
    {
        internal ContextConfig(Func<IExecutionContext, Task<T>> func)
            : base((_, ctx) => func(ctx))
        {
        }

        public static implicit operator ContextConfig<T>(T value) => new ContextConfig<T>(_ => Task.FromResult(value));

        public Task<T> GetValueAsync(IExecutionContext context) => GetValueAsync(null, context);

        public override bool IsDocumentConfig => false;
    }
}
