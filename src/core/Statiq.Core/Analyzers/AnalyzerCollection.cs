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
            analyzer.ThrowIfNull(nameof(analyzer));

            if (name.Any(c => char.IsWhiteSpace(c)))
            {
                throw new ArgumentException("Analyzer names must not contain whitespace", nameof(name));
            }
            if (_analyzers.ContainsKey(name))
            {
                throw new ArgumentException($"An analyzer with the name {name} already exists", nameof(name));
            }
            _analyzers.Add(name, analyzer);
        }

        internal void LogResults(IEnumerable<AnalyzerResult> results)
        {
            if (results is object)
            {
                // Log the document results first
                foreach (IGrouping<IDocument, AnalyzerResult> documentGroup in results.Where(x => x.Document is object).GroupBy(x => x.Document).OrderBy(x => x.Key.ToSafeDisplayString()))
                {
                    _engine.Logger.LogInformation(documentGroup.Key.ToDisplayString());
                    LogResults(documentGroup.Key, documentGroup);
                }

                // Then log general results
                AnalyzerResult[] globalResults = results.Where(x => x.Document is null).ToArray();
                if (globalResults.Length > 0)
                {
                    _engine.Logger.LogInformation("Global Results");
                    LogResults(null, globalResults);
                }
            }
        }

        private void LogResults(IDocument document, IEnumerable<AnalyzerResult> results)
        {
            foreach (AnalyzerResult result in results.OrderBy(x => x.AnalyzerName))
            {
                _engine.Logger.Log(result.LogLevel, $"{result.Message} ({result.AnalyzerName})");
                if (result.LogLevel == LogLevel.Warning)
                {
                    _engine.LogBuildServerWarning(document, $"{result.Message} ({result.AnalyzerName})");
                }
                else if (result.LogLevel != LogLevel.None && result.LogLevel >= LogLevel.Error)
                {
                    _engine.LogBuildServerError(document, $"{result.Message} ({result.AnalyzerName})");
                }
            }
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
