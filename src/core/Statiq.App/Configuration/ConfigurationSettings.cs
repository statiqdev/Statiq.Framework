using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Statiq.Common;

namespace Statiq.App
{
    internal class ConfigurationSettings : ConfigurationMetadata, IConfigurationSettings
    {
        public ConfigurationSettings(IConfigurationRoot configuration)
            : base(configuration)
        {
        }

        public IDictionary<string, object> Dictionary { get; } = new Dictionary<string, object>();

        public override bool ContainsKey(string key)
        {
            _ = key ?? throw new ArgumentNullException(nameof(key));
            return Dictionary.ContainsKey(key) || base.ContainsKey(key);
        }

        public override bool TryGetRaw(string key, out object value)
        {
            _ = key ?? throw new ArgumentNullException(nameof(key));
            return Dictionary.TryGetValue(key, out value) || base.TryGetRaw(key, out value);
        }

        // Enumerate the keys separately so we don't evaluate values
        public override IEnumerable<string> Keys
        {
            get
            {
                foreach (string key in Dictionary.Keys)
                {
                    yield return key;
                }
                foreach (string previousKey in base.Keys)
                {
                    if (!Dictionary.ContainsKey(previousKey))
                    {
                        yield return previousKey;
                    }
                }
            }
        }

        public override IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            foreach (KeyValuePair<string, object> item in Dictionary)
            {
                yield return TypeHelper.ExpandKeyValuePair(item, this);
            }
            IEnumerator<KeyValuePair<string, object>> baseEnumerator = base.GetEnumerator();
            while (baseEnumerator.MoveNext())
            {
                if (!Dictionary.ContainsKey(baseEnumerator.Current.Key))
                {
                    yield return baseEnumerator.Current;
                }
            }
        }

        public override IEnumerator<KeyValuePair<string, object>> GetRawEnumerator()
        {
            foreach (KeyValuePair<string, object> item in Dictionary)
            {
                yield return item;
            }
            IEnumerator<KeyValuePair<string, object>> baseEnumerator = base.GetEnumerator();
            while (baseEnumerator.MoveNext())
            {
                if (!Dictionary.ContainsKey(baseEnumerator.Current.Key))
                {
                    yield return baseEnumerator.Current;
                }
            }
        }

        object IDictionary<string, object>.this[string key]
        {
            get => this[key];
            set => Dictionary[key] = value;
        }

        public bool IsReadOnly => false;

        ICollection<string> IDictionary<string, object>.Keys => Keys.ToArray();

        ICollection<object> IDictionary<string, object>.Values => Values.ToArray();

        ICollection<string> IConfigurationSettings.Keys => Keys.ToArray();

        ICollection<object> IConfigurationSettings.Values => Values.ToArray();

        object IConfigurationSettings.this[string key]
        {
            get => this[key];
            set => Dictionary[key] = value;
        }

        public void Add(string key, object value) => Dictionary[key] = value;

        public void Add(KeyValuePair<string, object> item) => Dictionary.Add(item);

        // Not supported because the value of the item is raw vs. the expanded values presented by the dictionary
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => throw new NotSupportedException();

        // Not supported because the value of the item is raw vs. the expanded values presented by the dictionary
        public bool Contains(KeyValuePair<string, object> item) => throw new NotSupportedException();

        public void Clear() => throw new NotSupportedException();

        public bool Remove(string key) => throw new NotSupportedException();

        public bool Remove(KeyValuePair<string, object> item) => throw new NotSupportedException();
    }
}
