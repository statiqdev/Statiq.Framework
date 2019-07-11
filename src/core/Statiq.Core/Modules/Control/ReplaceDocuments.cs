using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;
using Statiq.Common.Configuration;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.Modules;

namespace Statiq.Core.Modules.Control
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