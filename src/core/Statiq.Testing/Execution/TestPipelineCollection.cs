using System;
using System.Collections;
using System.Collections.Generic;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestPipelineCollection : IPipelineCollection
    {
        private readonly Dictionary<string, IPipeline> _pipelines = new Dictionary<string, IPipeline>();

        public IPipeline Add(string name)
        {
            IPipeline pipeline = new TestPipeline();
            Add(name, pipeline);
            return pipeline;
        }

        public IPipeline this[string name]
        {
            get => _pipelines[name];
            set => _pipelines[name] = value ?? throw new ArgumentNullException(nameof(value));
        }

        public ICollection<string> Keys => ((IDictionary<string, IPipeline>)_pipelines).Keys;

        public ICollection<IPipeline> Values => ((IDictionary<string, IPipeline>)_pipelines).Values;

        public int Count => _pipelines.Count;

        public bool IsReadOnly => ((IDictionary<string, IPipeline>)_pipelines).IsReadOnly;

        public void Add(string name, IPipeline value) =>
            _pipelines.Add(
                name ?? throw new ArgumentNullException(nameof(name)),
                value ?? throw new ArgumentNullException(nameof(value)));

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

        public bool ContainsKey(string name) => _pipelines.ContainsKey(name);

        public void CopyTo(KeyValuePair<string, IPipeline>[] array, int arrayIndex) =>
            ((IDictionary<string, IPipeline>)_pipelines).CopyTo(array, arrayIndex);

        public bool Remove(string name) => _pipelines.Remove(name);

        public bool Remove(KeyValuePair<string, IPipeline> item) =>
            ((IDictionary<string, IPipeline>)_pipelines).Remove(item);

        public bool TryGetValue(string name, out IPipeline value) =>
            _pipelines.TryGetValue(name, out value);

        public IEnumerator<KeyValuePair<string, IPipeline>> GetEnumerator() =>
            ((IDictionary<string, IPipeline>)_pipelines).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            ((IDictionary<string, IPipeline>)_pipelines).GetEnumerator();
    }
}
