using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    /// <summary>
    /// An analyzer that runs a delegate.
    /// </summary>
    public class DelegateAnalyzer : IAnalyzer
    {
        private readonly Func<ImmutableArray<IDocument>, IAnalyzerContext, Task> _analyzeFunc;

        public DelegateAnalyzer(
            LogLevel logLevel,
            IEnumerable<string> pipelines,
            IEnumerable<Phase> phases,
            Func<ImmutableArray<IDocument>, IAnalyzerContext, Task> analyzeFunc)
        {
            LogLevel = logLevel;
            Pipelines = pipelines?.ToArray();
            Phases = phases?.ToArray();
            _analyzeFunc = analyzeFunc.ThrowIfNull(nameof(analyzeFunc));
        }

        /// <inheritdoc/>
        public LogLevel LogLevel { get; set; }

        /// <inheritdoc/>
        public string[] Pipelines { get; }

        /// <inheritdoc/>
        public Phase[] Phases { get; }

        public async Task AnalyzeAsync(ImmutableArray<IDocument> documents, IAnalyzerContext context) => await _analyzeFunc(documents, context);
    }
}
