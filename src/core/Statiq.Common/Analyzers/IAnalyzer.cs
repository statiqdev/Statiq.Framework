using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    public interface IAnalyzer
    {
        /// <summary>
        /// The level at which this analyzer should log.
        /// </summary>
        LogLevel LogLevel { get; set; }

        /// <summary>
        /// The pipelines and phases this analyzer will be run after.
        /// </summary>
        IEnumerable<KeyValuePair<string, Phase>> PipelinePhases { get; }

        /// <summary>
        /// Performs analysis.
        /// </summary>
        /// <param name="context">An analysis context that contains the documents to analyze as well as other state information.</param>
        Task AnalyzeAsync(IAnalyzerContext context);
    }
}
