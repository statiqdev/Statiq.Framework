using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.Modules;

namespace Statiq.CodeAnalysis
{
    public class CompileContent : IModule
    {
        public Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
