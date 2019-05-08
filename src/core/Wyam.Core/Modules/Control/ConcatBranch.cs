using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Modules;

namespace Wyam.Core.Modules.Control
{
    /// <summary>
    /// Executes a sequence of modules against the input documents and concatenates their results and the original input.
    /// This is similar to <see cref="Branch"/> except that the results of the specified modules are concatenated with the
    /// original input documents instead of being forgotten.
    /// </summary>
    /// <category>Control</category>
    public class ConcatBranch : ContainerModule
    {
        private DocumentConfig<bool> _predicate;

        /// <summary>
        /// Evaluates the specified modules with each input document as the initial
        /// document and then outputs the original input documents without modification concatenated with the result documents
        /// from the specified modules.
        /// </summary>
        /// <param name="modules">The modules to execute.</param>
        public ConcatBranch(params IModule[] modules)
            : this((IEnumerable<IModule>)modules)
        {
        }

        /// <summary>
        /// Evaluates the specified modules with each input document as the initial
        /// document and then outputs the original input documents without modification concatenated with the result documents
        /// from the specified modules.
        /// </summary>
        /// <param name="modules">The modules to execute.</param>
        public ConcatBranch(IEnumerable<IModule> modules)
            : base(modules)
        {
        }

        /// <summary>
        /// Limits the documents passed to the child modules to those that satisfy the
        /// supplied predicate. All original input documents are output without
        /// modification regardless of whether they satisfy the predicate.
        /// </summary>
        /// <param name="predicate">A delegate that should return a <c>bool</c>.</param>
        /// <returns>The current module instance.</returns>
        public ConcatBranch Where(DocumentConfig<bool> predicate)
        {
            _predicate = _predicate.CombineWith(predicate);
            return this;
        }

        /// <inheritdoc />
        public override async Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            IEnumerable<IDocument> documents = await inputs.FilterAsync(_predicate, context);
            return inputs.Concat(await context.ExecuteAsync(this, documents));
        }
    }
}
