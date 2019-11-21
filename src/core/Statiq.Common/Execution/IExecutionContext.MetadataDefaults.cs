using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public partial interface IExecutionContext
    {
        // IMetadata

        bool IMetadata.TryGetRaw(string key, out object value) => Settings.TryGetRaw(key, out value);

        bool IMetadata.TryGetValue<TValue>(string key, out TValue value) => Settings.TryGetValue<TValue>(key, out value);

        IMetadata IMetadata.GetMetadata(params string[] keys) => Settings.GetMetadata(keys);

        // IReadOnlyDictionary<string, object>

        IEnumerable<string> IReadOnlyDictionary<string, object>.Keys => Settings.Keys;

        IEnumerable<object> IReadOnlyDictionary<string, object>.Values => Settings.Values;

        object IReadOnlyDictionary<string, object>.this[string key] => Settings[key];

        bool IReadOnlyDictionary<string, object>.ContainsKey(string key) => Settings.ContainsKey(key);

        bool IReadOnlyDictionary<string, object>.TryGetValue(string key, out object value) => Settings.TryGetValue(key, out value);

        // IReadOnlyCollection<KeyValuePair<string, object>>

        int IReadOnlyCollection<KeyValuePair<string, object>>.Count => Settings.Count;

        // IEnumerable<KeyValuePair<string, object>>

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator() => Settings.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Settings.GetEnumerator();
    }
}
