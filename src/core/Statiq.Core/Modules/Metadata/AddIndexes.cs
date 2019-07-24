using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Adds a one-based index to every document as metadata.
    /// </summary>
    /// <metadata cref="Keys.Index" usage="Output" />
    /// <category>Metadata</category>
    public class AddIndexes : IModule
    {
        /// <inheritdoc />
        public Task<IEnumerable<IDocument>> ExecuteAsync(IExecutionContext context) =>
            Task.FromResult(context.Inputs.Select((x, i) => x.Clone(new MetadataItems { { Keys.Index, i + 1 } })));
    }
}
