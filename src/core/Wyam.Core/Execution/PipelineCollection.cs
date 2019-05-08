using System;
using System.Collections;
using System.Collections.Generic;
using Wyam.Common.Execution;

namespace Wyam.Core.Execution
{
    // Implement dictionary explicitly so that we can disallow null pipelines
    internal class PipelineCollection : IPipelineCollection
    {
        private readonly Dictionary<string, IPipeline> _pipelines =
            new Dictionary<string, IPipeline>(StringComparer.OrdinalIgnoreCase);

        public IPipeline Add(string name)
        {
            IPipeline pipeline = new Pipeline();
            Add(name, pipeline);
            return pipeline;
        }

        public IPipeline this[string key]
        {
            get => _pipelines[key];
            set => _pipelines[key] = value ?? throw new ArgumentNullException(nameof(value));
        }

        public ICollection<string> Keys => ((IDictionary<string, IPipeline>)_pipelines).Keys;

        public ICollection<IPipeline> Values => ((IDictionary<string, IPipeline>)_pipelines).Values;

        public int Count => _pipelines.Count;

        public bool IsReadOnly => ((IDictionary<string, IPipeline>)_pipelines).IsReadOnly;

        public void Add(string key, IPipeline value) =>
            _pipelines.Add(key, value ?? throw new ArgumentNullException(nameof(value)));

        public void Add(KeyValuePair<string, IPipeline> item)
        {
            if (item.Value == null)
            {
                throw new ArgumentNullException(nameof(item.Value));
            }
            ((IDictionary<string, IPipeline>)_pipelines).Add(item);
        }

        public void Clear() => _pipelines.Clear();

        public bool Contains(KeyValuePair<string, IPipeline> item) => ((IDictionary<string, IPipeline>)_pipelines).Contains(item);

        public bool ContainsKey(string key) => _pipelines.ContainsKey(key);

        public void CopyTo(KeyValuePair<string, IPipeline>[] array, int arrayIndex) =>
            ((IDictionary<string, IPipeline>)_pipelines).CopyTo(array, arrayIndex);

        public bool Remove(string key) =>
            _pipelines.Remove(key);

        public bool Remove(KeyValuePair<string, IPipeline> item) =>
            ((IDictionary<string, IPipeline>)_pipelines).Remove(item);

        public bool TryGetValue(string key, out IPipeline value) =>
            _pipelines.TryGetValue(key, out value);

        public IEnumerator<KeyValuePair<string, IPipeline>> GetEnumerator() =>
            ((IDictionary<string, IPipeline>)_pipelines).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            ((IDictionary<string, IPipeline>)_pipelines).GetEnumerator();
    }
}
