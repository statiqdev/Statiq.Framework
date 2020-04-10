using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Statiq.Common;

namespace Statiq.Core
{
    // Implement dictionary explicitly so that we can disallow null pipelines
    // This resets the pipeline phases collection in the engine on changes
    internal class PipelineCollection : IPipelineCollection
    {
        private readonly Dictionary<string, IPipeline> _pipelines =
            new Dictionary<string, IPipeline>(StringComparer.OrdinalIgnoreCase);
        private readonly Engine _engine;

        public PipelineCollection(Engine engine)
        {
            _engine = engine;
        }

        public IPipeline Add(string name)
        {
            IPipeline pipeline = new Pipeline();
            Add(name, pipeline);
            _engine.ResetPipelinePhases();
            return pipeline;
        }

        public IPipeline this[string name]
        {
            get => _pipelines[name];
            set
            {
                _pipelines[name] = value ?? throw new ArgumentNullException(nameof(value));
                _engine.ResetPipelinePhases();
            }
        }

        public ICollection<string> Keys => ((IDictionary<string, IPipeline>)_pipelines).Keys;

        public ICollection<IPipeline> Values => ((IDictionary<string, IPipeline>)_pipelines).Values;

        public int Count => _pipelines.Count;

        public bool IsReadOnly => ((IDictionary<string, IPipeline>)_pipelines).IsReadOnly;

        public void Add(string name, IPipeline value)
        {
            _pipelines.Add(
                name ?? throw new ArgumentNullException(nameof(name)),
                value ?? throw new ArgumentNullException(nameof(value)));
            _engine.ResetPipelinePhases();
        }

        public void Add(KeyValuePair<string, IPipeline> item)
        {
            if (item.Value == null)
            {
                throw new ArgumentNullException(nameof(item.Value));
            }
            ((IDictionary<string, IPipeline>)_pipelines).Add(item);
            _engine.ResetPipelinePhases();
        }

        public void Clear()
        {
            _pipelines.Clear();
            _engine.ResetPipelinePhases();
        }

        public bool Contains(KeyValuePair<string, IPipeline> item) => ((IDictionary<string, IPipeline>)_pipelines).Contains(item);

        public bool ContainsKey(string name) => _pipelines.ContainsKey(name);

        public void CopyTo(KeyValuePair<string, IPipeline>[] array, int arrayIndex) =>
            ((IDictionary<string, IPipeline>)_pipelines).CopyTo(array, arrayIndex);

        public bool Remove(string name)
        {
            if (_pipelines.Remove(name))
            {
                _engine.ResetPipelinePhases();
                return true;
            }
            return false;
        }

        public bool Remove(KeyValuePair<string, IPipeline> item)
        {
            if (((IDictionary<string, IPipeline>)_pipelines).Remove(item))
            {
                _engine.ResetPipelinePhases();
                return true;
            }
            return false;
        }

        public bool TryGetValue(string name, out IPipeline value) =>
            _pipelines.TryGetValue(name, out value);

        public IEnumerator<KeyValuePair<string, IPipeline>> GetEnumerator() =>
            ((IDictionary<string, IPipeline>)_pipelines).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            ((IDictionary<string, IPipeline>)_pipelines).GetEnumerator();

        // IReadOnlyPipelineCollection

        IEnumerable<string> IReadOnlyDictionary<string, IReadOnlyPipeline>.Keys => Keys;

        IEnumerable<IReadOnlyPipeline> IReadOnlyDictionary<string, IReadOnlyPipeline>.Values => Values;

        IReadOnlyPipeline IReadOnlyDictionary<string, IReadOnlyPipeline>.this[string key] => this[key];

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out IReadOnlyPipeline value)
        {
            if (TryGetValue(key, out IPipeline pipeline))
            {
                value = pipeline;
                return true;
            }
            value = default;
            return false;
        }

        IEnumerator<KeyValuePair<string, IReadOnlyPipeline>> IEnumerable<KeyValuePair<string, IReadOnlyPipeline>>.GetEnumerator() =>
            _pipelines.Select(x => new KeyValuePair<string, IReadOnlyPipeline>(x.Key, x.Value)).GetEnumerator();
    }
}
