using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestSettings : ConfigurationMetadata, IConfigurationProvider, IConfigurationSource, IDictionary<string, string>, ISettings
    {
        private readonly Dictionary<string, string> _settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private ConfigurationReloadToken _reloadToken = new ConfigurationReloadToken();

        public TestSettings()
            : base(null)
        {
            Configuration = new ConfigurationBuilder().Add(this).Build();
        }

        public void SetConfiguration(IConfigurationBuilder builder) =>
            Configuration = builder.Add(this).Build();

        // IConfigurationProvider (implementations from ConfigurationProvider)

        public bool TryGet(string key, out string value) => _settings.TryGetValue(key, out value);

        public void Set(string key, string value) => _settings[key] = value;

        public IChangeToken GetReloadToken() => _reloadToken;

        public void Load()
        {
        }

        public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string parentPath)
        {
            string prefix = (parentPath == null) ? string.Empty : (parentPath + ConfigurationPath.KeyDelimiter);
            return _settings.Where((KeyValuePair<string, string> kv) => kv.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).Select((KeyValuePair<string, string> kv) => Segment(kv.Key, prefix.Length)).Concat(earlierKeys)
                .OrderBy((string k) => k, ConfigurationKeyComparer.Instance);
        }

        private static string Segment(string key, int prefixLength)
        {
            int num = key.IndexOf(ConfigurationPath.KeyDelimiter, prefixLength, StringComparison.OrdinalIgnoreCase);
            if (num >= 0)
            {
                return key.Substring(prefixLength, num - prefixLength);
            }
            return key.Substring(prefixLength);
        }

        // IConfigurationSource

        public IConfigurationProvider Build(IConfigurationBuilder builder) => this;

        // IDictionary

        public new string this[string key] { get => _settings[key]; set => _settings[key] = value; }

        public bool IsReadOnly => ((IDictionary<string, string>)_settings).IsReadOnly;

        ICollection<string> IDictionary<string, string>.Keys => ((IDictionary<string, string>)_settings).Keys;

        ICollection<string> IDictionary<string, string>.Values => ((IDictionary<string, string>)_settings).Values;

        public void Add(string key, string value) => _settings.Add(key, value);

        public void Add(KeyValuePair<string, string> item) => ((IDictionary<string, string>)_settings).Add(item);

        public void Clear() => _settings.Clear();

        public bool Contains(KeyValuePair<string, string> item) => ((IDictionary<string, string>)_settings).Contains(item);

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex) => ((IDictionary<string, string>)_settings).CopyTo(array, arrayIndex);

        public bool Remove(string key) => _settings.Remove(key);

        public bool Remove(KeyValuePair<string, string> item) => ((IDictionary<string, string>)_settings).Remove(item);

        bool IDictionary<string, string>.TryGetValue(string key, out string value) => _settings.TryGetValue(key, out value);

        IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator() => ((IDictionary<string, string>)_settings).GetEnumerator();
    }
}
