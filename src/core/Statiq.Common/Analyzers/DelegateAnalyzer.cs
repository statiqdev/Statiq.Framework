using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// An analyzer that runs a delegate.
    /// </summary>
    public class DelegateAnalyzer : IAnalyzer
    {
        private readonly Func<IAnalyzerContext, Task> _analyzeFunc;

        public DelegateAnalyzer(IEnumerable<string> pipelines, IEnumerable<Phase> phases, Func<IAnalyzerContext, Task> analyzeFunc)
        {
            Pipelines = pipelines?.ToArray();
            Phases = phases?.ToArray();
            _analyzeFunc = analyzeFunc.ThrowIfNull(nameof(analyzeFunc));
        }

        /// <inheritdoc/>
        public string[] Pipelines { get; }

        /// <inheritdoc/>
        public Phase[] Phases { get; }

        public async Task AnalyzeAsync(IAnalyzerContext context) => await _analyzeFunc(context);
    }
}
