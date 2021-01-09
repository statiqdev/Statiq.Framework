using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Statiq.Common;

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
    public class ConvertExcelToCsv : ParallelSyncModule
    {
        protected override IEnumerable<IDocument> ExecuteInput(IDocument input, IExecutionContext context)
        {
            IEnumerable<IEnumerable<string>> records;
            using (Stream stream = input.GetContentStream())
            {
                records = ExcelFile.GetAllRecords(stream);
            }

            using (Stream contentStream = context.GetContentStream())
            {
                CsvFile.WriteAllRecords(records, contentStream);
                return input.Clone(context.GetContentProvider(contentStream, MediaTypes.Get(".csv"))).Yield();
            }
        }
    }
}
