using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Statiq.Common;

namespace Statiq.Core
{
    internal class ReadOnlyConfigurationSettings : ConfigurationMetadata, IReadOnlyConfigurationSettings
    {
        private readonly IDictionary<string, object> _valueOverrides;

        // Passes initial value overrides (presumably from ConfigurationSettings)
        public ReadOnlyConfigurationSettings(
            IExecutionState executionState,
            IConfiguration configuration,
            IDictionary<string, object> valueOverrides)
            : base(configuration)
        {
            _ = executionState ?? throw new ArgumentNullException(nameof(executionState));

            _valueOverrides = valueOverrides ?? new Dictionary<string, object>();

            // Iterate over value overrides to convert any of them that are script metadata
            foreach (KeyValuePair<string, object> item in _valueOverrides.ToArray())
            {
                if (ScriptMetadataValue.TryGetScriptMetadataValue(item.Key, item.Value, executionState, out ScriptMetadataValue metadataValue))
                {
                    _valueOverrides[item.Key] = metadataValue;
                }
            }

            // Iterate over configuration and convert them, but only if we don't already have an override for that key
            foreach (KeyValuePair<string, string> item in configuration.AsEnumerable())
            {
                if (!_valueOverrides.ContainsKey(item.Key)
                    && ScriptMetadataValue.TryGetScriptMetadataValue(item.Key, item.Value, executionState, out ScriptMetadataValue metadataValue))
                {
                    _valueOverrides[item.Key] = metadataValue;
                }
            }
        }

        // Used for creating sections
        private ReadOnlyConfigurationSettings(IConfiguration configuration, IDictionary<string, object> valueOverrides)
            : base(configuration)
        {
            _valueOverrides = valueOverrides ?? throw new ArgumentNullException(nameof(valueOverrides));
        }

        protected override object GetSectionMetadata(IConfigurationSection section) =>
            new ReadOnlyConfigurationSettings(section, _valueOverrides);

        public override bool ContainsKey(string key) => base.ContainsKey(key) || _valueOverrides.ContainsKey(key);

        // Enumerate the keys seperatly so we don't evaluate values
        public override IEnumerable<string> Keys
        {
            get
            {
                foreach (string key in _valueOverrides.Keys)
                {
                    yield return key;
                }
                foreach (string baseKey in base.Keys)
                {
                    if (!_valueOverrides.ContainsKey(baseKey))
                    {
                        yield return baseKey;
                    }
                }
            }
        }

        public override IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            foreach (KeyValuePair<string, object> item in _valueOverrides)
            {
                yield return TypeHelper.ExpandKeyValuePair(item, this);
            }
            IEnumerator<KeyValuePair<string, object>> baseEnumerator = base.GetEnumerator();
            while (baseEnumerator.MoveNext())
            {
                if (!_valueOverrides.ContainsKey(baseEnumerator.Current.Key))
                {
                    yield return baseEnumerator.Current;
                }
            }
        }

        public override IEnumerator<KeyValuePair<string, object>> GetRawEnumerator()
        {
            foreach (KeyValuePair<string, object> item in _valueOverrides)
            {
                yield return item;
            }
            IEnumerator<KeyValuePair<string, object>> baseEnumerator = base.GetEnumerator();
            while (baseEnumerator.MoveNext())
            {
                if (!_valueOverrides.ContainsKey(baseEnumerator.Current.Key))
                {
                    yield return baseEnumerator.Current;
                }
            }
        }

        public override bool TryGetRaw(string key, out object value)
        {
            _ = key ?? throw new ArgumentNullException(nameof(key));
            if (_valueOverrides.TryGetValue(key, out object metadataValue))
            {
                value = metadataValue;
                return true;
            }
            return base.TryGetRaw(key, out value);
        }
    }
}
