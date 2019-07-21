using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Clones each input document with the metadata from each result document.
    /// </summary>
    /// <remarks>
    /// If more than one result document is produced, it will be merged with every input document and the
    /// total number of output documents will be input * result. If you want to maintain a 1-to-1 relationship
    /// between input documents and child module results, wrap with a <see cref="ForEachDocument"/> module
    /// or use the <see cref="IModuleExtensions.ForEachDocument(IModule)"/> extension.
    /// </remarks>
    /// <category>Metadata</category>
    public class MergeMetadata : DocumentModule
    {
        private bool _reverse;

        public MergeMetadata()
            : base(Array.Empty<IModule>())
        {
        }

        public MergeMetadata(params IModule[] modules)
            : base(modules)
        {
        }

        public MergeMetadata(params string[] pipelines)
            : base(new ExecuteConfig(Config.FromContext(ctx => ctx.Documents.FromPipelines(pipelines))))
        {
        }

        /// <summary>
        /// The default behavior of this module is to clone each input document with the metadata
        /// from each result document. This method reverses that logic by cloning each child result document with the
        /// metadata from each input document (keeping the content from the child result document).
        /// </summary>
        /// <param name="reverse"><c>true</c> to reverse the merge direction, <c>false</c> otherwise.</param>
        /// <returns>The current module instance.</returns>
        public MergeMetadata Reverse(bool reverse = true)
        {
            _reverse = reverse;
            return this;
        }

        protected override Task<IEnumerable<IDocument>> GetOutputDocumentsAsync(
            IReadOnlyList<IDocument> inputs,
            IReadOnlyList<IDocument> childOutputs) =>
            Task.FromResult(_reverse
                ? childOutputs.SelectMany(result => inputs.Select(input => result.Clone(input)))
                : inputs.SelectMany(input => childOutputs.Select(result => input.Clone(result))));
    }
}