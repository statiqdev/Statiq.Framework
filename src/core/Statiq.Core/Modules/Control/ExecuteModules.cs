using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common.Configuration;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.Modules;

namespace Statiq.Core.Modules.Control
{
    /// <summary>
    /// Executes the specified modules.
    /// </summary>
    /// <remarks>
    /// This module can control whether the input documents are passed to the child modules
    /// and the way in which the output documents from the child modules are treated.
    /// </remarks>
    /// <category>Control</category>
    public class ExecuteModules : ContainerModule
    {
        private bool _withInputs;
        private ExecuteModuleResults _results = ExecuteModuleResults.Replace;

        /// <summary>
        /// Executes the specified modules.
        /// </summary>
        /// <param name="modules">The modules to execute.</param>
        public ExecuteModules(params IModule[] modules)
            : this((IEnumerable<IModule>)modules)
        {
        }

        /// <summary>
        /// Executes the specified modules.
        /// </summary>
        /// <param name="modules">The modules to execute.</param>
        public ExecuteModules(IEnumerable<IModule> modules)
            : base(modules)
        {
        }

        /// <summary>
        /// Controls whether the input documents are passed to the child modules.
        /// </summary>
        /// <param name="withInputs"><c>true</c> to pass input documents to the child modules, <c>false</c> otherwise.</param>
        /// <returns>The current module instance.</returns>
        public ExecuteModules WithInputs(bool withInputs = true)
        {
            _withInputs = withInputs;
            return this;
        }

        /// <summary>
        /// Specifies what to do with the output documents from the child modules.
        /// </summary>
        /// <param name="results">A <see cref="ExecuteModuleResults"/> value indicating what to do with the output documents.</param>
        /// <returns>The current module instance.</returns>
        public ExecuteModules WithResults(ExecuteModuleResults results)
        {
            _results = results;
            return this;
        }

        /// <inheritdoc />
        public override async Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            IReadOnlyList<IDocument> results = Children.Count > 0
                ? await context.ExecuteAsync(Children, _withInputs ? inputs : null)
                : ImmutableArray<IDocument>.Empty;
            switch (_results)
            {
                case ExecuteModuleResults.Replace:
                    return results;
                case ExecuteModuleResults.Concat:
                    return inputs.Concat(results);
                case ExecuteModuleResults.Merge:
                    return inputs.SelectMany(
                        context,
                        input => results.Select(result => input.Clone(result, result.ContentProvider)));
            }
            return ImmutableArray<IDocument>.Empty;
        }
    }
}
