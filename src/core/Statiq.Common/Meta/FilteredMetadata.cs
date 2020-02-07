using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Statiq.Common
{
    public class FilteredMetadata : IMetadata
    {
        private readonly IMetadata _metadata;
        private readonly HashSet<string> _keys;

        public FilteredMetadata(IMetadata metadata, params string[] keys)
        {
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _keys = keys == null
                ? new HashSet<string>()
                : new HashSet<string>(keys.Where(x => metadata.ContainsKey(x)), StringComparer.OrdinalIgnoreCase);
        }

        public object this[string key]
        {
            get
            {
                _ = key ?? throw new ArgumentNullException(nameof(key));
                if (!_keys.Contains(key))
                {
                    throw new KeyNotFoundException();
                }
                return _metadata[key];
            }
        }

        public IEnumerable<string> Keys => _keys;

        public IEnumerable<object> Values => _keys.Select(x => _metadata[x]);

        public int Count => _keys.Count;

        public bool ContainsKey(string key)
        {
            _ = key ?? throw new ArgumentNullException(nameof(key));
            return _keys.Contains(key);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() =>
            _keys.Select(x => KeyValuePair.Create(x, _metadata[x])).GetEnumerator();

        public bool TryGetRaw(string key, out object value)
        {
            _ = key ?? throw new ArgumentNullException(nameof(key));
            value = default;
            return _keys.Contains(key) && _metadata.TryGetRaw(key, out value);
        }

        public bool TryGetValue<TValue>(string key, out TValue value)
        {
            _ = key ?? throw new ArgumentNullException(nameof(key));
            value = default;
            return _keys.Contains(key) && _metadata.TryGetValue(key, out value);
        }

        public bool TryGetValue(string key, out object value)
        {
            _ = key ?? throw new ArgumentNullException(nameof(key));
            value = default;
            return _keys.Contains(key) && _metadata.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
