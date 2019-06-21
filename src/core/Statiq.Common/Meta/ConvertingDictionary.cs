using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Statiq.Common.Execution;

namespace Statiq.Common.Meta
{
    /// <summary>
    /// A dictionary with metadata type conversion superpowers.
    /// </summary>
    /// <remarks>
    /// This class wraps an underlying <see cref="Dictionary{TKey, TValue}"/> but
    /// uses the provided <see cref="IExecutionContext"/> to perform type conversions
    /// when requesting values.
    /// </remarks>
    public class ConvertingDictionary : IMetadataDictionary
    {
        private readonly Dictionary<string, object> _dictionary;

        public ConvertingDictionary()
        {
            _dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public ConvertingDictionary(IDictionary<string, object> dictionary)
        {
            _dictionary = new Dictionary<string, object>(dictionary, StringComparer.OrdinalIgnoreCase);
        }

        public ConvertingDictionary(IEnumerable<KeyValuePair<string, object>> items)
        {
            _dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            // Copy over in case there are duplicate keys
            foreach (KeyValuePair<string, object> item in items)
            {
                _dictionary[item.Key] = item.Value;
            }
        }

        /// <inheritdoc />
        public object this[string key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }
                if (!TryGetValue(key, out object value))
                {
                    throw new KeyNotFoundException("The key " + key + " was not found in metadata, use Get() to provide a default value.");
                }
                return value;
            }

            set => _dictionary[key] = value;
        }

        /// <inheritdoc />
        object IReadOnlyDictionary<string, object>.this[string key] => this[key];

        /// <inheritdoc />
        public int Count => _dictionary.Count;

        /// <inheritdoc />
        public ICollection<string> Keys => _dictionary.Keys;

        /// <inheritdoc />
        public ICollection<object> Values => _dictionary.Values.Select(GetValue).ToArray();

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        IEnumerable<string> IReadOnlyDictionary<string, object>.Keys => Keys;

        /// <inheritdoc />
        IEnumerable<object> IReadOnlyDictionary<string, object>.Values => Values;

        /// <inheritdoc />
        public void Add(string key, object value) => _dictionary.Add(key, value);

        /// <inheritdoc />
        public void Add(KeyValuePair<string, object> item) =>
            ((IDictionary<string, object>)_dictionary).Add(item);

        /// <inheritdoc />
        public void Clear() => _dictionary.Clear();

        /// <inheritdoc />
        public bool Contains(KeyValuePair<string, object> item) =>
            _dictionary.Select(GetItem).Contains(item);

        /// <inheritdoc />
        public bool ContainsKey(string key) => _dictionary.ContainsKey(key);

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) =>
            _dictionary.Select(GetItem).ToArray().CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _dictionary.Select(GetItem).GetEnumerator();

        /// <inheritdoc />
        public bool TryGetRaw(string key, out object value) => _dictionary.TryGetValue(key, out value);

        /// <inheritdoc />
        public bool Remove(string key) => _dictionary.Remove(key);

        /// <inheritdoc />
        public bool Remove(KeyValuePair<string, object> item) =>
            ((IDictionary<string, object>)_dictionary).Remove(item);

        /// <inheritdoc />
        public bool TryGetValue<TValue>(string key, out TValue value)
        {
            value = default;
            if (!_dictionary.TryGetValue(key, out object rawValue))
            {
                return false;
            }
            rawValue = GetValue(rawValue);
            return TypeHelper.TryConvert(rawValue, out value);
        }

        /// <inheritdoc />
        public bool TryGetValue(string key, out object value) => TryGetValue<object>(key, out value);

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public IMetadata GetMetadata(params string[] keys) =>
            throw new NotSupportedException();

        /// <summary>
        /// This resolves the metadata value by recursively expanding IMetadataValue.
        /// </summary>
        private object GetValue(object originalValue) =>
            originalValue is IMetadataValue metadataValue ? GetValue(metadataValue.Get(this)) : originalValue;

        /// <summary>
        /// This resolves the metadata value by expanding IMetadataValue.
        /// </summary>
        private KeyValuePair<string, object> GetItem(KeyValuePair<string, object> item) =>
            item.Value is IMetadataValue metadataValue
                ? new KeyValuePair<string, object>(item.Key, GetValue(metadataValue.Get(this)))
                : item;
    }
}
