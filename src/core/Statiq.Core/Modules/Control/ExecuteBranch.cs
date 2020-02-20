using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Executes "branches" of modules with the input documents and concatenates their outputs.
    /// </summary>
    /// <category>Control</category>
    public class ExecuteBranch : Module
    {
        private readonly List<IEnumerable<IModule>> _branches = new List<IEnumerable<IModule>>();

        public ExecuteBranch(params IModule[] modules)
            : this((IEnumerable<IModule>)modules)
        {
        }

        public ExecuteBranch(IEnumerable<IModule> modules)
        {
            Branch(modules);
        }

        public ExecuteBranch Branch(params IModule[] modules) => Branch((IEnumerable<IModule>)modules);

        public ExecuteBranch Branch(IEnumerable<IModule> modules)
        {
            _branches.Add(modules ?? throw new ArgumentNullException(nameof(modules)));
            return this;
        }

        /// <inheritdoc />
        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            List<IDocument> results = new List<IDocument>();
            foreach (IEnumerable<IModule> modules in _branches)
            {
                results.AddRange(await context.ExecuteModulesAsync(modules, context.Inputs));
            }
            return results;
        }
    }
}
