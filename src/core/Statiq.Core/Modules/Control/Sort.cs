using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.Modules;

namespace Statiq.Core.Modules.Control
{
    /// <summary>
    /// Sorts the input documents based on the specified comparison delegate.
    /// </summary>
    /// <remarks>
    /// The sorted documents are output as the result of this module. This is similar
    /// to the <see cref="OrderBy"/> module but gives greater control over the sorting
    /// process.
    /// </remarks>
    /// <category>Control</category>
    public class Sort : IModule
    {
        private readonly Comparison<IDocument> _sort;

        /// <summary>
        /// Creates a sort module.
        /// </summary>
        /// <param name="sort">The sorting delegate to use.</param>
        public Sort(Comparison<IDocument> sort)
        {
            _sort = sort ?? throw new ArgumentNullException(nameof(sort));
        }

        /// <inheritdoc />
        public Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            IDocument[] inputArray = inputs.ToArray();
            Array.Sort(inputArray, _sort);
            return Task.FromResult<IEnumerable<IDocument>>(inputArray);
        }
    }
}