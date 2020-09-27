using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public interface IAnalyzer
    {
        /// <summary>
        /// Performs analysis.
        /// </summary>
        /// <param name="context">An analysis context that contains the documents to analyze as well as other state information.</param>
        Task AnalyzeAsync(IAnalyzerContext context);

        /// <summary>
        /// The pipelines this analyzer applies to, or null to apply to all pipelines.
        /// </summary>
        string[] Pipelines { get; }

        /// <summary>
        /// The phases this analyzer applies to, or null to apply to all phases.
        /// </summary>
        Phase[] Phases { get; }
    }
}
