using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Flattens a tree structure given child documents are stored in the
    /// default <see cref="Keys.Children"/> metadata key.
    /// </summary>
    /// <remarks>
    /// The documents will be returned in no particular order and only distinct
    /// documents will be returned (I.e., if a document exists as a
    /// child of more than one parent, it will only appear once in the result set).
    /// </remarks>
    /// <metadata cref="Keys.Children" usage="Input"/>
    /// <category>Metadata</category>
    public class FlattenDocuments : SyncModule
    {
        protected override IEnumerable<IDocument> Execute(IExecutionContext context)
        {
            HashSet<IDocument> results = new HashSet<IDocument>();
            foreach (IDocument input in context.Inputs)
            {
                results.AddRange(input.GetDescendantsAndSelf());
            }
            return results;
        }
    }
}