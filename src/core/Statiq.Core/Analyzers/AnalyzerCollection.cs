using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    // TODO: Convert to dictionary with analyzer names as the key, names cannot contains non alpha (no spaces) so a pattern "[name] [level]" can be used in settings
    internal class AnalyzerCollection : IAnalyzerCollection
    {
        private readonly List<IAnalyzer> _analyzers = new List<IAnalyzer>();

        private readonly Engine _engine;

        internal AnalyzerCollection(Engine engine)
        {
            _engine = engine;
        }

        public void Add(IAnalyzer analyzer)
        {
            if (analyzer is object)
            {
                _analyzers.Add(analyzer);
            }
        }

        internal async Task AnalyzeAsync(PipelinePhase pipelinePhase)
        {
            // TODO: Make different context implementations for document analyzers, use context to form message with name of analyzer

            // Run analyzers
            AnalyzerContext analyzerContext = new AnalyzerContext(_engine, pipelinePhase);
            await _analyzers
                .Where(v => v.Phases?.Contains(pipelinePhase.Phase) != false && v.Pipelines?.Contains(pipelinePhase.PipelineName, StringComparer.OrdinalIgnoreCase) != false)
                .ParallelForEachAsync(async v => await v.AnalyzeAsync(analyzerContext));

            // Log results
            bool fail = false;
            foreach (AnalyzerResult result in analyzerContext.Results)
            {
                string documentPart = result.Document is object
                    ? $" [{result.Document.ToSafeDisplayString()}]"
                    : string.Empty;
                _engine.Logger.Log(
                    result.LogLevel,
                    $"{pipelinePhase.PipelineName}/{pipelinePhase.Phase} » Analyzer{documentPart}: {result.Message}");
                if (result.LogLevel >= (_engine.Settings.GetBool(Keys.FailOnAnalyzerWarnings) ? LogLevel.Warning : LogLevel.Error))
                {
                    fail = true;
                }
            }

            // Throw if analysis failed
            if (fail)
            {
                throw new Exception($"Analyzer error result(s) for pipeline {pipelinePhase.PipelineName}/{pipelinePhase.Phase}");
            }
        }
    }
}
