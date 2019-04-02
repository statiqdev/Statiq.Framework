using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Modules;

namespace Wyam.Core.Modules.Control
{
    /// <summary>
    /// Takes the first X documents from the current pipeline and discards the rest.
    /// </summary>
    /// <category>Control</category>
    public class Take : IModule
    {
        private readonly int _x;

        /// <summary>
        /// Takes the first X documents from the current pipeline and discards the rest.
        /// </summary>
        /// <param name="x">An integer representing the number of documents to preserve from the current pipeline.</param>
        public Take(int x)
        {
            _x = x;
        }

        /// <inheritdoc />
        public Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return Task.FromResult(inputs.Take(_x));
        }
    }
}
