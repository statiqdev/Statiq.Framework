using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    internal class AnalyzerCollection : IAnalyzerCollection
    {
        private readonly Dictionary<string, IAnalyzer> _analyzers = new Dictionary<string, IAnalyzer>(StringComparer.OrdinalIgnoreCase);

        private readonly Engine _engine;

        internal AnalyzerCollection(Engine engine)
        {
            _engine = engine;
        }

        public void Add(string name, IAnalyzer analyzer)
        {
            name.ThrowIfNullOrWhiteSpace(nameof(name));
            if (name.Any(c => char.IsWhiteSpace(c)))
            {
                throw new ArgumentException("Analyzer names must not contain whitespace", nameof(name));
            }
            if (_analyzers.ContainsKey(name))
            {
                throw new ArgumentException($"An analyzer with the name {name} already exists", nameof(name));
            }
            _analyzers.Add(name, analyzer.ThrowIfNull(nameof(analyzer)));
        }

        internal async Task<ConcurrentBag<AnalyzerResult>> AnalyzeAsync(PipelinePhase pipelinePhase)
        {
            // Don't check log level here since each document could override it
            ConcurrentBag<AnalyzerResult> results = new ConcurrentBag<AnalyzerResult>();
            await _analyzers
                .Where(x => x.Value.Phases?.Contains(pipelinePhase.Phase) != false
                    && x.Value.Pipelines?.Contains(pipelinePhase.PipelineName, StringComparer.OrdinalIgnoreCase) != false)
                .ParallelForEachAsync(async v => await v.Value.AnalyzeAsync(pipelinePhase.Outputs, new AnalyzerContext(_engine, pipelinePhase, v, results)));
            return results;
        }

        public IAnalyzer this[string key] => _analyzers[key];

        public IEnumerable<string> Keys => _analyzers.Keys;

        public IEnumerable<IAnalyzer> Values => _analyzers.Values;

        public int Count => _analyzers.Count;

        public bool ContainsKey(string key) => _analyzers.ContainsKey(key);

        public IEnumerator<KeyValuePair<string, IAnalyzer>> GetEnumerator() => _analyzers.GetEnumerator();

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out IAnalyzer value) => _analyzers.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => _analyzers.GetEnumerator();
    }
}
