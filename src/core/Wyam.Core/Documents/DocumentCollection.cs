using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Core.Execution;

namespace Wyam.Core.Documents
{
    internal class DocumentCollection : IDocumentCollection
    {
        private readonly ConcurrentDictionary<string, ImmutableArray<IDocument>> _documents;
        private readonly PipelinePhase _pipelinePhase;
        private readonly IPipelineCollection _pipelines;

        public DocumentCollection(ConcurrentDictionary<string, ImmutableArray<IDocument>> documents, PipelinePhase pipelinePhase, IPipelineCollection pipelines)
        {
            _documents = documents;
            _pipelinePhase = pipelinePhase;
            _pipelines = pipelines;
        }

        public IEnumerator<IDocument> GetEnumerator()
        {
            Check();
            return GetDependencies().SelectMany(x => x.Value).Distinct().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IReadOnlyDictionary<string, IEnumerable<IDocument>> ByPipeline()
        {
            Check();
            return GetDependencies().ToDictionary(x => x.Key, x => x.Value.Distinct());
        }

        public IEnumerable<IDocument> FromPipeline(string pipeline)
        {
            Check(pipeline);
            return _documents[pipeline].Distinct();
        }

        public IEnumerable<IDocument> ExceptPipeline(string pipeline)
        {
            Check(pipeline);
            return GetDependencies().Where(x => x.Key != pipeline).SelectMany(x => x.Value).Distinct();
        }

        public IEnumerable<IDocument> this[string pipline] => FromPipeline(pipline);

        // If in the process phase only get documents for dependencies
        private IEnumerable<KeyValuePair<string, ImmutableArray<IDocument>>> GetDependencies() =>
            _documents
                .Where(x =>
                    _pipelinePhase.Phase == Phase.Render
                    || _pipelinePhase.Dependencies.Any(d => d.Name.Equals(x.Key, StringComparison.OrdinalIgnoreCase)));

        private void Check(string pipeline)
        {
            Check();
            if (string.IsNullOrEmpty(pipeline))
            {
                throw new ArgumentException(nameof(pipeline));
            }
            if (!_pipelines.ContainsKey(pipeline))
            {
                throw new KeyNotFoundException($"The pipeline {pipeline} could not be found");
            }
            if (_pipelines[pipeline].Isolated)
            {
                throw new InvalidOperationException($"Cannot access documents in isolated pipeline {pipeline}");
            }
            if (_pipelinePhase.Phase == Phase.Process
                && !_pipelinePhase.Dependencies.Any(d => d.Name.Equals(pipeline, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Cannot access documents from pipeline {pipeline} without a dependency while in the {nameof(IPipeline.ProcessModules)} phase");
            }
        }

        private void Check()
        {
            if (_pipelines[_pipelinePhase.Name].Isolated)
            {
                throw new InvalidOperationException($"Cannot access documents from inside isolated pipeline {_pipelinePhase.Name}");
            }
            if (_pipelinePhase.Phase != Phase.Process && _pipelinePhase.Phase != Phase.Render)
            {
                throw new InvalidOperationException($"Cannot access the document collection during the {_pipelinePhase.Phase} phase");
            }
        }
    }
}
