using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Statiq.Common
{
    // Implements MetadataDictionary so we can call .GetMetadata() to get nested settings dictionaries
    internal class SettingsConfigurationDictionary : MetadataDictionary, ISettingsConfiguration
    {
        private readonly string _path;

        public SettingsConfigurationDictionary(string path)
        {
            _path = path;
        }

        public void ResolveScriptMetadataValues(string key, IExecutionState executionState)
        {
            // We need to enumerate a materialized list since we're changing it during enumeration
            foreach (KeyValuePair<string, object> item in this.GetEnumerable().ToArray())
            {
                if (ScriptMetadataValue.TryGetScriptMetadataValue(key, item.Value, executionState, out ScriptMetadataValue metadataValue))
                {
                    this[item.Key] = metadataValue;
                }
                else if (item.Value is ISettingsConfiguration configurationSettings)
                {
                    configurationSettings.ResolveScriptMetadataValues(key, executionState);
                }
            }
        }

        string IConfigurationSection.Path => _path;

        IEnumerable<IConfigurationSection> IConfiguration.GetChildren() =>
            this.Select(x => x.Value is IConfigurationSection section ? section : new SettingsConfigurationSection(x.Key, $"{_path}:{x.Key}", x.Value.ToString()));

        IConfigurationSection IConfiguration.GetSection(string key)
        {
            int firstSeparator = key.IndexOf(':');
            string rootKey = firstSeparator >= 0 ? key[..firstSeparator] : key;
            if (TryGetValue(rootKey, out object value))
            {
                // This was a valid key
                return value is IConfigurationSection section
                    ? (firstSeparator >= 0 ? section.GetSection(key[(key.LastIndexOf(':') + 1) ..]) : section)
                    : new SettingsConfigurationSection(key[(key.LastIndexOf(':') + 1) ..], $"{_path}:{key}", value.ToString());
            }

            // This isn't a valid key, so return a blank section
            return new SettingsConfigurationSection(key[(key.LastIndexOf(':') + 1) ..], $"{_path}:{key}", default);
        }
    }
}
