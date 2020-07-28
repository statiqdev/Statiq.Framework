using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace Statiq.Common
{
    /// <summary>
    /// Use this type instead of <see cref="ConcurrentDictionary{TKey, TValue}"/> in caching scenarios.
    /// </summary>
    /// <remarks>
    /// See http://reedcopsey.com/2011/01/16/concurrentdictionarytkeytvalue-used-with-lazyt/
    /// and https://andrewlock.net/making-getoradd-on-concurrentdictionary-thread-safe-using-lazy/.
    /// </remarks>
    public class ConcurrentCache<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, Lazy<TValue>> _dictionary = new ConcurrentDictionary<TKey, Lazy<TValue>>();

        private static Lazy<TValue> GetValue(Func<TValue> valueFactory) =>
            new Lazy<TValue>(valueFactory, LazyThreadSafetyMode.ExecutionAndPublication);

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory) =>
            _dictionary.GetOrAdd(key, k => GetValue(() => valueFactory(k))).Value;

        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory) =>
            _dictionary.AddOrUpdate(key, k => GetValue(() => addValueFactory(k)), (k, v) => GetValue(() => updateValueFactory(k, v.Value))).Value;

        public bool TryAdd(TKey key, Func<TValue> valueFactory) => _dictionary.TryAdd(key, GetValue(valueFactory));

        public bool TryRemove(TKey key, out TValue value)
        {
            if (_dictionary.TryRemove(key, out Lazy<TValue> lazy))
            {
                value = lazy.Value;
                return true;
            }
            value = default;
            return false;
        }

        public void Clear() => _dictionary.Clear();

        public TValue this[TKey key]
        {
            get => _dictionary[key].Value;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (_dictionary.TryGetValue(key, out Lazy<TValue> lazy))
            {
                value = lazy.Value;
                return true;
            }
            value = default;
            return false;
        }

        public int Count => _dictionary.Count;

        public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

        public IEnumerable<TKey> Keys => _dictionary.Keys;

        public IEnumerable<TValue> Values => _dictionary.Values.Select(x => x.Value);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() =>
            _dictionary.Select(x => new KeyValuePair<TKey, TValue>(x.Key, x.Value.Value)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}