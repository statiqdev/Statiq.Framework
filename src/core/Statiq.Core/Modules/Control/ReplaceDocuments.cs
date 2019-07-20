using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Replaces documents in the current pipeline.
    /// </summary>
    /// <category>Control</category>
    public class ReplaceDocuments : DocumentModule
    {
        public ReplaceDocuments()
        {
        }

        /// <inheritdoc />
        public ReplaceDocuments(params IModule[] modules)
            : base(modules)
        {
        }

        /// <inheritdoc />
        public ReplaceDocuments(params string[] pipelines)
            : base(new GetDocuments(pipelines))
        {
        }

        protected override Task<IEnumerable<IDocument>> GetOutputDocumentsAsync(
            IReadOnlyList<IDocument> inputs,
            IReadOnlyList<IDocument> childOutputs) =>
            Task.FromResult<IEnumerable<IDocument>>(childOutputs);
    }
}