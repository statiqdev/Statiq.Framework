using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Replaces documents in the current pipeline.
    /// </summary>
    /// <category>Control</category>
    public class ReplaceDocuments : ChildDocumentsModule
    {
        public ReplaceDocuments()
            : base(Array.Empty<IModule>())
        {
        }

        public ReplaceDocuments(params IModule[] modules)
            : base(modules)
        {
        }

        public ReplaceDocuments(params string[] pipelines)
            : base(new ExecuteConfig(Config.FromContext(ctx => ctx.Results.FromPipelines(pipelines))))
        {
        }

        protected override Task<IEnumerable<IDocument>> GetOutputDocumentsAsync(
            IReadOnlyList<IDocument> inputs,
            IReadOnlyList<IDocument> childOutputs,
            IExecutionContext context) =>
            Task.FromResult<IEnumerable<IDocument>>(childOutputs);
    }
}