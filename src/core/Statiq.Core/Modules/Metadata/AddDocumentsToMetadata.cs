using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Adds the result document(s) to metadata of the input documents.
    /// </summary>
    /// <remarks>
    /// If more than one result document is produced, it will be added as an array of documents
    /// to the specified metadata key.
    /// </remarks>
    /// <category>Metadata</category>
    public class AddDocumentsToMetadata : DocumentModule
    {
        private readonly string _key;

        public AddDocumentsToMetadata(string key)
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));
        }

        /// <inheritdoc />
        public AddDocumentsToMetadata(string key, params IModule[] modules)
            : base(modules)
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));
        }

        /// <inheritdoc />
        public AddDocumentsToMetadata(string key, params string[] pipelines)
            : base(new GetDocuments(pipelines))
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));
        }

        protected override Task<IEnumerable<IDocument>> GetOutputDocumentsAsync(
            IReadOnlyList<IDocument> inputs,
            IReadOnlyList<IDocument> childOutputs) =>
            Task.FromResult(childOutputs.Count == 0
                ? inputs
                : inputs.Select(input => input.Clone(new MetadataItems
                {
                    { _key, childOutputs.Count == 1 ? (object)childOutputs[0] : childOutputs }
                })));
    }
}