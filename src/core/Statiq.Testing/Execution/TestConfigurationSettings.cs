using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestConfigurationSettings : ConfigurationMetadata, IConfigurationProvider, IConfigurationSource, IConfigurationSettings
    {
        private readonly Dictionary<string, object> _settings = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        private ConfigurationReloadToken _reloadToken = new ConfigurationReloadToken();

        public TestConfigurationSettings()
            : base(new ConfigurationRoot(Array.Empty<IConfigurationProvider>()))
        {
            Configuration = new ConfigurationBuilder().Add(this).Build();
        }

        public void SetConfiguration(IConfigurationBuilder builder) =>
            Configuration = builder.Add(this).Build();

        // IConfigurationProvider (implementations from ConfigurationProvider)

        public bool TryGet(string key, out string value)
        {
            if (_settings.TryGetValue(key, out object objValue))
            {
                value = objValue.ToString();
                return true;
            }
            value = default;
            return false;
        }

        public void Set(string key, string value) => _settings[key] = value;

        public IChangeToken GetReloadToken() => _reloadToken;

        public void Load()
        {
        }

        public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string parentPath)
        {
            string prefix = (parentPath == null) ? string.Empty : (parentPath + ConfigurationPath.KeyDelimiter);
            return _settings
                .Where((KeyValuePair<string, object> kv) => kv.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Select((KeyValuePair<string, object> kv) => Segment(kv.Key, prefix.Length))
                .Concat(earlierKeys)
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

        public void Add(KeyValuePair<string, object> item) => Add(item.Key, item.Value);

        public void Add(string key, object value)
        {
            _ = key ?? throw new ArgumentNullException(nameof(key));

            if (ContainsKey(key))
            {
                throw new ArgumentException($"The key {key} already exists");
            }
            this[key] = value;
        }

        /// <inheritdoc/>
        public new object this[string key]
        {
            get => base[key];
            set
            {
                _ = key ?? throw new ArgumentNullException(nameof(key));
                _settings[key] = value;
            }
        }

        public override IEnumerable<string> Keys => _settings.Keys;

        public IConfigurationProvider Build(IConfigurationBuilder builder) => this;

        ICollection<string> IDictionary<string, object>.Keys => Keys.ToArray();

        ICollection<object> IDictionary<string, object>.Values => Values.ToArray();

        ICollection<string> IConfigurationSettings.Keys => Keys.ToArray();

        ICollection<object> IConfigurationSettings.Values => Values.ToArray();

        public bool IsReadOnly => false;

        // Not supported because the value of the item is raw vs. the expanded values presented by the dictionary
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => throw new NotSupportedException();

        // Not supported because the value of the item is raw vs. the expanded values presented by the dictionary
        public bool Contains(KeyValuePair<string, object> item) => throw new NotSupportedException();

        public void Clear() => throw new NotSupportedException();

        public bool Remove(string key) => throw new NotSupportedException();

        public bool Remove(KeyValuePair<string, object> item) => throw new NotSupportedException();
    }
}
