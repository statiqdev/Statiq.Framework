using System.Collections.Generic;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Replaces documents in the current pipeline.
    /// </summary>
    /// <category>Control</category>
    public class ReplaceDocuments : DocumentModule<ReplaceDocuments>
    {
        /// <inheritdoc />
        public ReplaceDocuments(params IModule[] modules)
            : base(modules)
        {
        }

        /// <inheritdoc />
        public ReplaceDocuments(IEnumerable<IModule> modules)
            : base(modules)
        {
        }

        /// <inheritdoc />
        public ReplaceDocuments(params string[] pipelines)
            : base(pipelines)
        {
        }

        /// <inheritdoc />
        public ReplaceDocuments(IEnumerable<string> pipelines)
            : base(pipelines)
        {
        }

        /// <inheritdoc />
        public ReplaceDocuments(DocumentConfig<IEnumerable<IDocument>> documents)
            : base(documents)
        {
        }

        protected override IEnumerable<IDocument> GetOutputDocuments(IEnumerable<IDocument> inputs, IEnumerable<IDocument> results) => results;
    }
}