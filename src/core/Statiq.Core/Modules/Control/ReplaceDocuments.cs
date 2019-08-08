using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Replaces documents in the current pipeline.
    /// </summary>
    /// <category>Control</category>
    public class ReplaceDocuments : SyncChildDocumentsModule
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
            : base(new ExecuteConfig(Config.FromContext(ctx => ctx.Outputs.FromPipelines(pipelines))))
        {
        }

        protected override IEnumerable<IDocument> Execute(
            IExecutionContext context,
            ImmutableArray<IDocument> childOutputs) =>
            childOutputs;
    }
}