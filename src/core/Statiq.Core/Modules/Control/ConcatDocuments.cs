using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Concatenates documents in the current pipeline.
    /// </summary>
    /// <remarks>
    /// The resulting documents of this module are concatenated after the
    /// input documents and both are output.
    /// </remarks>
    /// <category>Control</category>
    public class ConcatDocuments : DocumentModule
    {
        public ConcatDocuments()
        {
        }

        /// <inheritdoc />
        public ConcatDocuments(params IModule[] modules)
            : base(modules)
        {
        }

        /// <inheritdoc />
        public ConcatDocuments(params string[] pipelines)
            : base(new GetDocuments(pipelines))
        {
        }

        protected override Task<IEnumerable<IDocument>> GetOutputDocumentsAsync(
            IReadOnlyList<IDocument> inputs,
            IReadOnlyList<IDocument> childOutputs) =>
            Task.FromResult(inputs.Concat(childOutputs));
    }
}