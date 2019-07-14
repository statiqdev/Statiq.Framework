using System.Collections.Generic;
using System.Linq;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Clones each input document with the content and metadata from each result document.
    /// </summary>
    /// <remarks>
    /// If more than one result document is produced, it will be merged with every input document and the
    /// total number of output documents will be input * result.
    /// </remarks>
    public class MergeDocuments : DocumentModule<MergeDocuments>
    {
        private bool _reverse;

        /// <inheritdoc />
        public MergeDocuments(params IModule[] modules)
            : base(modules)
        {
        }

        /// <inheritdoc />
        public MergeDocuments(IEnumerable<IModule> modules)
            : base(modules)
        {
        }

        /// <inheritdoc />
        public MergeDocuments(params string[] pipelines)
            : base(pipelines)
        {
        }

        /// <inheritdoc />
        public MergeDocuments(IEnumerable<string> pipelines)
            : base(pipelines)
        {
        }

        /// <inheritdoc />
        public MergeDocuments(DocumentConfig<IEnumerable<IDocument>> documents)
            : base(documents)
        {
        }

        /// <summary>
        /// The default behavior of this module is to clone each input document with the content and metadata
        /// from each result document. This method reverses that logic by cloning each result document with the
        /// content and metadata from each input document.
        /// </summary>
        /// <param name="reverse"><c>true</c> to reverse the merge direction, <c>false</c> otherwise.</param>
        /// <returns>The current module instance.</returns>
        public MergeDocuments Reverse(bool reverse = true)
        {
            _reverse = reverse;
            return this;
        }

        protected override IEnumerable<IDocument> GetOutputDocuments(IEnumerable<IDocument> inputs, IEnumerable<IDocument> results) =>
            _reverse
                ? results.SelectMany(result => inputs.Select(input => result.Clone(input, input.ContentProvider)))
                : inputs.SelectMany(input => results.Select(result => input.Clone(result, result.ContentProvider)));
    }
}