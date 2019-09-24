using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Statiq.Common;

namespace Statiq.Common
{
    /// <summary>
    /// A linked list of metadata items.
    /// </summary>
    public class Metadata : IMetadata
    {
        private readonly IMetadata _previous;

        protected IDictionary<string, object> Dictionary { get; }

        public Metadata(IMetadata previous, IEnumerable<KeyValuePair<string, object>> items = null)
            : this(items)
        {
            _previous = previous;
        }

        // null items will mean a Dictionary doesn't get created, pass in an empty items array to ensure a Dictionary
        public Metadata(IEnumerable<KeyValuePair<string, object>> items = null)
        {
            if (items != null)
            {
                Dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                foreach (KeyValuePair<string, object> item in items)
                {
                    Dictionary[item.Key] = item.Value;
                }
            }
        }

        public bool ContainsKey(string key)
        {
            _ = key ?? throw new ArgumentNullException(nameof(key));
            return (Dictionary?.ContainsKey(key) ?? false) || (_previous?.ContainsKey(key) ?? false);
        }

        public bool TryGetRaw(string key, out object value)
        {
            _ = key ?? throw new ArgumentNullException(nameof(key));
            value = default;
            return (Dictionary?.TryGetValue(key, out value) ?? false) || (_previous?.TryGetRaw(key, out value) ?? false);
        }

        public bool TryGetValue<TValue>(string key, out TValue value)
        {
            value = default;
            if (key != null && TryGetRaw(key, out object raw))
            {
                object expanded = GetValue(raw);
                return TypeHelper.TryConvert(expanded, out value);
            }
            return false;
        }

        public bool TryGetValue(string key, out object value) => TryGetValue<object>(key, out value);

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
        }

        public IEnumerable<string> Keys => this.Select(x => x.Key);

        public IEnumerable<object> Values => this.Select(x => x.Value);

        // The Select ensures LINQ optimizations won't turn this into a recursive call to Count
        public int Count => this.Select(_ => (object)null).Count();

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            if (Dictionary != null)
            {
                foreach (KeyValuePair<string, object> item in Dictionary)
                {
                    yield return GetItem(item);
                }
            }
            if (_previous != null)
            {
                foreach (KeyValuePair<string, object> previousItem in _previous)
                {
                    if (!Dictionary.ContainsKey(previousItem.Key))
                    {
                        yield return previousItem;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IMetadata GetMetadata(params string[] keys) =>
            new Metadata(this.Where(x => keys.Contains(x.Key, StringComparer.OrdinalIgnoreCase)));

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