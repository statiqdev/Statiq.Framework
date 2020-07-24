using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Statiq.Common;

namespace Statiq.Core
{
    internal class ConfigurationSettings : ConfigurationMetadata, IConfigurationSettings
    {
        private readonly IExecutionState _executionState;

        // Settings override the configuration (and may include converted configuration
        // values that override the original ones if the configuration included script metadata)
        private readonly IDictionary<string, object> _settings;

        // Passes initial value overrides (presumably from ConfigurationSettings)
        public ConfigurationSettings(
            IExecutionState executionState,
            IConfiguration configuration,
            IEnumerable<KeyValuePair<string, object>> settings)
            : base(configuration)
        {
            _executionState = executionState ?? throw new ArgumentNullException(nameof(executionState));

            _settings = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            if (settings != null)
            {
                foreach (KeyValuePair<string, object> setting in settings)
                {
                    this[setting.Key] = setting.Value;
                }
            }

            // Iterate over configuration and convert them, but only if we don't already have a setting for that key
            foreach (KeyValuePair<string, string> item in configuration.AsEnumerable())
            {
                if (!_settings.ContainsKey(item.Key)
                    && ScriptMetadataValue.TryGetScriptMetadataValue(item.Key, item.Value, executionState, out ScriptMetadataValue metadataValue))
                {
                    _settings[item.Key] = metadataValue;
                }
            }
        }

        // Used for creating sections
        private ConfigurationSettings(IConfiguration configuration, IDictionary<string, object> valueOverrides)
            : base(configuration)
        {
            _settings = valueOverrides ?? throw new ArgumentNullException(nameof(valueOverrides));
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

                _settings[key] = ScriptMetadataValue.TryGetScriptMetadataValue(key, value, _executionState, out ScriptMetadataValue metadataValue)
                    ? metadataValue
                    : value;
            }
        }

        protected override object GetSectionMetadata(IConfigurationSection section) =>
            new ConfigurationSettings(section, _settings);

        public override bool ContainsKey(string key) => base.ContainsKey(key) || _settings.ContainsKey(key);

        // Enumerate the keys separately so we don't evaluate values
        public override IEnumerable<string> Keys
        {
            get
            {
                foreach (string key in _settings.Keys)
                {
                    yield return key;
                }
                foreach (string baseKey in base.Keys)
                {
                    if (!_settings.ContainsKey(baseKey))
                    {
                        yield return baseKey;
                    }
                }
            }
        }

        public override IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            foreach (KeyValuePair<string, object> item in _settings)
            {
                yield return TypeHelper.ExpandKeyValuePair(item, this);
            }
            IEnumerator<KeyValuePair<string, object>> baseEnumerator = base.GetEnumerator();
            while (baseEnumerator.MoveNext())
            {
                if (!_settings.ContainsKey(baseEnumerator.Current.Key))
                {
                    yield return baseEnumerator.Current;
                }
            }
        }

        public override IEnumerator<KeyValuePair<string, object>> GetRawEnumerator()
        {
            foreach (KeyValuePair<string, object> item in _settings)
            {
                yield return SettingsValue.Get(item);
            }
            IEnumerator<KeyValuePair<string, object>> baseEnumerator = base.GetEnumerator();
            while (baseEnumerator.MoveNext())
            {
                if (!_settings.ContainsKey(baseEnumerator.Current.Key))
                {
                    yield return SettingsValue.Get(baseEnumerator.Current);
                }
            }
        }

        public override bool TryGetRaw(string key, out object value)
        {
            _ = key ?? throw new ArgumentNullException(nameof(key));
            if (_settings.TryGetValue(key, out value))
            {
                value = SettingsValue.Get(value);
                return true;
            }
            if (base.TryGetRaw(key, out value))
            {
                value = SettingsValue.Get(value);
                return true;
            }
            return false;
        }

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
