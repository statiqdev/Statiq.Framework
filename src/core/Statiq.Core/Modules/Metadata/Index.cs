using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.Meta;
using Statiq.Common.Modules;

namespace Statiq.Core.Modules.Metadata
{
    /// <summary>
    /// Adds a one-based index to every document as metadata.
    /// </summary>
    /// <metadata cref="Keys.Index" usage="Output" />
    /// <category>Metadata</category>
    public class Index : IModule
    {
        /// <inheritdoc />
        public Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context) =>
            Task.FromResult(inputs.Select((x, i) => context.GetDocument(x, new MetadataItems { { Keys.Index, i + 1 } })));
    }
}
