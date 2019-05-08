using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Util;

namespace Wyam.Core.Modules.Metadata
{
    /// <summary>
    /// Adds a one-based index to every document as metadata.
    /// </summary>
    /// <metadata cref="Keys.Index" usage="Output" />
    /// <category>Metadata</category>
    public class Index : IModule
    {
        /// <inheritdoc />
        public async Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            List<IDocument> outputs = new List<IDocument>();
            int i = 0;
            foreach (IDocument input in inputs)
            {
                outputs.Add(await context.NewGetDocumentAsync(input, metadata: new MetadataItems { { Keys.Index, i + 1 } }));
                i++;
            }
            return outputs;
        }
    }
}
