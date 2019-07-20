using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Gets documents from other pipelines.
    /// </summary>
    /// <remarks>
    /// This module does not include the input documents as part of it's output.
    /// </remarks>
    /// <category>Control</category>
    public class GetDocuments : IModule
    {
        private readonly IEnumerable<string> _pipelines;

        public GetDocuments(params string[] pipelines)
            : this((IEnumerable<string>)pipelines)
        {
        }

        public GetDocuments(IEnumerable<string> pipelines)
        {
            _pipelines = pipelines ?? Array.Empty<string>();
        }

        public Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context) =>
            Task.FromResult(_pipelines.SelectMany(x => context.Documents[x]));
    }
}
