using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Common.Configuration
{
    public class DocumentPredicate : DocumentConfig<bool>
    {
        internal DocumentPredicate(Func<IDocument, IExecutionContext, Task<bool>> func)
            : base(func)
        {
        }

        public static implicit operator DocumentPredicate(bool value) => new ContextPredicate(_ => Task.FromResult(value));
    }
}
