using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.Meta;
using Statiq.Common.Modules;

namespace Statiq.Core.Modules.Metadata
{
    /// <summary>
    /// Flattens a tree structure given child documents are stored in a given metadata key ("Children" by default).
    /// The flattened documents are returned in no particular order.
    /// </summary>
    /// <metadata cref="Keys.Children" usage="Input"/>
    /// <category>Metadata</category>
    public class Flatten : IModule
    {
        private readonly string _childrenKey = Keys.Children;

        /// <summary>
        /// Creates a new flatten module.
        /// </summary>
        public Flatten()
        {
        }

        /// <summary>
        /// Creates a new flatten module with the specified children key.
        /// </summary>
        /// <param name="childrenKey">The metadata key that contains the children.</param>
        public Flatten(string childrenKey)
        {
            _childrenKey = childrenKey;
        }

        /// <inheritdoc />
        public Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            HashSet<IDocument> results = new HashSet<IDocument>();
            foreach (IDocument input in inputs)
            {
                input.Flatten(results, _childrenKey);
            }
            return Task.FromResult<IEnumerable<IDocument>>(results);
        }
    }
}