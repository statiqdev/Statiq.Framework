using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common.Documents;
using Statiq.Common.Modules;
using Statiq.Common.Execution;

namespace Statiq.Core.Modules.IO
{
    /// <summary>
    /// Reads text provided to the application on startup.
    /// </summary>
    /// <remarks>
    /// This modules creates a single document from text provided to Statiq on startup. In most cases, this will be text or file contents
    /// "piped" to the Statiq.exe via the command line from a file or prior chained executable. Also known as "Standard Input" or "STDIN".
    /// </remarks>
    /// <category>Input/Output</category>
    public class ReadApplicationInput : IModule
    {
        /// <inheritdoc />
        public async Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            // If ApplicationInput is empty, return nothing
            if (string.IsNullOrWhiteSpace(context.ApplicationInput))
            {
                return Array.Empty<IDocument>();
            }

            return new[] { context.GetDocument(await context.GetContentProviderAsync(context.ApplicationInput)) };
        }
    }
}
