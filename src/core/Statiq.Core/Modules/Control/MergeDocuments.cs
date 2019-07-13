using System.Collections.Generic;
using System.Linq;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Replaces the content and merges the metadata of each input document with the result documents.
    /// </summary>
    /// <remarks>
    /// If more than one result document is produced, it will be merged with every input document and the
    /// total number of output documents will be input * result.
    /// </remarks>
    public class MergeDocuments : DocumentModule<MergeDocuments>
    {
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

        protected override IEnumerable<IDocument> GetOutputDocuments(IEnumerable<IDocument> inputs, IEnumerable<IDocument> results) =>
            inputs.SelectMany(input => results.Select(result => input.Clone(result, result.ContentProvider)));
    }
}