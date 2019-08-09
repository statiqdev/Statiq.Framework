using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Statiq.CodeAnalysis.Scripting;
using Statiq.Common;

namespace Statiq.CodeAnalysis
{
    /// <summary>
    /// Performs string interpolation on document metadata.
    /// </summary>
    /// <remarks>
    /// This module uses <see cref="IDocumentExtensions.Interpolate(IDocument, string, IExecutionContext)"/>
    /// to interpolate all string metadata values that contain braces. Other document metadata is available
    /// to the interpolation, though the order in which metadata values are interpolated is undefined so
    /// on interpolated value shouldn't reference another.
    /// </remarks>
    /// <category>Metadata</category>
    public class InterpolateMetadata : ParallelSyncModule
    {
        protected override IEnumerable<IDocument> Execute(IDocument input, IExecutionContext context)
        {
            MetadataItems interpolatedValues = null;

            // Look for string values with braces
            foreach (KeyValuePair<string, object> item in input)
            {
                if (item.Value is string value && value.Contains('{') && value.Contains('}'))
                {
                    (interpolatedValues ?? (interpolatedValues = new MetadataItems()))
                        .Add(new MetadataItem(item.Key, input.Interpolate(value, context)));
                }
            }

            // If any were found, clone the document
            yield return interpolatedValues == null ? input : input.Clone(interpolatedValues);
        }
    }
}
