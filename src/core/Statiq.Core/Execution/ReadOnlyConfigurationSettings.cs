using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Statiq.Common;

namespace Statiq.Core
{
    internal class ReadOnlyConfigurationSettings : ConfigurationMetadata, IReadOnlyConfigurationSettings
    {
        private readonly Dictionary<string, IMetadataValue> _valueOverrides;

        public ReadOnlyConfigurationSettings(IConfiguration configuration, IExecutionState executionState)
            : base(configuration)
        {
            _ = executionState ?? throw new ArgumentNullException(nameof(executionState));
            _valueOverrides = new Dictionary<string, IMetadataValue>();
            foreach (KeyValuePair<string, string> item in configuration.AsEnumerable())
            {
                if (ScriptMetadataValue.TryGetMetadataValue(item.Value, executionState, out ScriptMetadataValue metadataValue))
                {
                    _valueOverrides[item.Key] = metadataValue;
                }
            }
        }

        private ReadOnlyConfigurationSettings(IConfiguration configuration, Dictionary<string, IMetadataValue> valueOverrides)
            : base(configuration)
        {
            _valueOverrides = valueOverrides ?? throw new ArgumentNullException(nameof(valueOverrides));
        }

        protected override object GetSectionMetadata(IConfigurationSection section) =>
            new ReadOnlyConfigurationSettings(section, _valueOverrides);

        public override bool TryGetRaw(string key, out object value)
        {
            _ = key ?? throw new ArgumentNullException(nameof(key));
            if (_valueOverrides.TryGetValue(key, out IMetadataValue metadataValue))
            {
                value = metadataValue;
                return true;
            }
            return base.TryGetRaw(key, out value);
        }
    }
}
