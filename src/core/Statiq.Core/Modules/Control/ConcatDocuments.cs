using System;
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
    public class ConcatDocuments : ChildDocumentsModule
    {
        public ConcatDocuments()
            : base(Array.Empty<IModule>())
        {
        }

        public ConcatDocuments(params IModule[] modules)
            : base(modules)
        {
        }

        public ConcatDocuments(params string[] pipelines)
            : base(new ExecuteConfig(Config.FromContext(ctx => ctx.Outputs.FromPipelines(pipelines))))
        {
        }

        protected override Task<IEnumerable<IDocument>> ExecuteAsync(
            IExecutionContext context,
            IReadOnlyList<IDocument> childOutputs) =>
            Task.FromResult(context.Inputs.Concat(childOutputs));
    }
}