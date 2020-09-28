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
        /// The pipelines this analyzer applies to, or null to apply to all pipelines.
        /// </summary>
        string[] Pipelines { get; }

        /// <summary>
        /// The phases this analyzer applies to, or null to apply to all phases.
        /// </summary>
        Phase[] Phases { get; }

        /// <summary>
        /// Performs analysis.
        /// </summary>
        /// <param name="documents">The documents to analyze.</param>
        /// <param name="context">An analysis context that contains the documents to analyze as well as other state information.</param>
        Task AnalyzeAsync(ImmutableArray<IDocument> documents, IAnalyzerContext context);
    }
}
