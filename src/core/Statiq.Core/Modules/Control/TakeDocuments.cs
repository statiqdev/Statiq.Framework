using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Takes the first X documents from the current pipeline and discards the rest.
    /// </summary>
    /// <category>Control</category>
    public class TakeDocuments : Module
    {
        private readonly int _x;

        /// <summary>
        /// Takes the first X documents from the current pipeline and discards the rest.
        /// </summary>
        /// <param name="x">An integer representing the number of documents to preserve from the current pipeline.</param>
        public TakeDocuments(int x)
        {
            _x = x;
        }

        /// <inheritdoc />
        public override Task<IEnumerable<IDocument>> ExecuteAsync(IExecutionContext context) => Task.FromResult(context.Inputs.Take(_x));
    }
}
