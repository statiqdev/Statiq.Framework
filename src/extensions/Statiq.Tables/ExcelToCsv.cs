using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.IO;
using Statiq.Common.Modules;
using Statiq.Common.Tracing;

namespace Statiq.Tables
{
    /// <summary>
    /// Transforms Excel content to CSV.
    /// </summary>
    /// <remarks>
    /// This module reads the content of each input document as Excel OOXML and outputs CSV content.
    /// The output CSV content uses <c>,</c> as separator and encloses every value in <c>"</c>.
    /// </remarks>
    /// <category>Content</category>
    public class ExcelToCsv : IModule
    {
        /// <inheritdoc />
        public async Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return await inputs.ParallelSelectAsync(context, async input =>
            {
                try
                {
                    IEnumerable<IEnumerable<string>> records;
                    using (Stream stream = await input.GetStreamAsync())
                    {
                        records = ExcelFile.GetAllRecords(stream);
                    }

                    using (Stream contentStream = await context.GetContentStreamAsync())
                    {
                        CsvFile.WriteAllRecords(records, contentStream);
                        return input.Clone(context.GetContentProvider(contentStream));
                    }
                }
                catch (Exception e)
                {
                    Trace.Error($"An {e} occurred ({input.Source.ToDisplayString()}): {e.Message}");
                    return null;
                }
            });
        }
    }
}
