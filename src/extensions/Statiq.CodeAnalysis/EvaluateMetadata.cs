using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Statiq.CodeAnalysis.Scripting;
using Statiq.Common;

namespace Statiq.CodeAnalysis
{
    /// <summary>
    /// Performs lazy evaluation on document metadata.
    /// </summary>
    /// <remarks>
    /// This module evaluates all metadata values that start with <c>=></c>. Other document metadata is available
    /// during the evaluation. Also note the evaluation is lazy: a compilation will be
    /// created for each evaluated metadata value at the time of requesting a value.
    /// </remarks>
    /// <category>Metadata</category>
    public class EvaluateMetadata : ParallelSyncModule
    {
        protected override IEnumerable<IDocument> ExecuteInput(IDocument input, IExecutionContext context)
        {
            MetadataItems evaluatedValues = null;

            // Look for string values with braces
            foreach (KeyValuePair<string, object> item in input)
            {
                if (item.Value is string value)
                {
                    value = value.TrimStart();
                    if (value.StartsWith("=>"))
                    {
                        value = value.Substring(2);
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            (evaluatedValues ?? (evaluatedValues = new MetadataItems()))
                                .Add(new MetadataItem(item.Key, new ScriptMetadataValue(value, context.ExecutionState)));
                        }
                    }
                }
            }

            // If any were found, clone the document
            yield return evaluatedValues == null ? input : input.Clone(evaluatedValues);
        }
    }
}
