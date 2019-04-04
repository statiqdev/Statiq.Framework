using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Common.Configuration
{
    public class ContextPredicate : DocumentPredicate
    {
        internal ContextPredicate(Func<IExecutionContext, Task<bool>> func)
            : base((_, ctx) => func(ctx))
        {
        }

        public static implicit operator ContextPredicate(bool value) => new ContextPredicate(_ => Task.FromResult(value));

        public Task<bool> GetValueAsync(IExecutionContext context) => GetValueAsync(null, context);

        public override bool IsDocumentConfig => false;
    }
}
