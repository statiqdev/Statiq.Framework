using System;
using System.Collections.Generic;
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
