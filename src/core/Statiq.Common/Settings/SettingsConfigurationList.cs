using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Statiq.Common
{
    internal class SettingsConfigurationList : List<object>, ISettingsConfiguration
    {
        private readonly string _path;

        public SettingsConfigurationList(string path)
        {
            _path = path;
        }

        public void ResolveScriptMetadataValues(string key, IExecutionState executionState)
        {
            for (int c = 0; c < Count; c++)
            {
                if (ScriptMetadataValue.TryGetScriptMetadataValue(key, this[c], executionState, out ScriptMetadataValue metadataValue))
                {
                    this[c] = metadataValue;
                }
                else if (this[c] is ISettingsConfiguration configurationSettings)
                {
                    configurationSettings.ResolveScriptMetadataValues(key, executionState);
                }
            }
        }

        string IConfigurationSection.Path => _path;

        IEnumerable<IConfigurationSection> IConfiguration.GetChildren() =>
            this.Select((x, i) => x is IConfigurationSection section ? section : new SettingsConfigurationSection(i.ToString(), $"{_path}:{i}", x.ToString()));

        IConfigurationSection IConfiguration.GetSection(string key)
        {
            int firstSeparator = key.IndexOf(':');
            string rootKey = firstSeparator >= 0 ? key[..firstSeparator] : key;
            if (int.TryParse(rootKey, out int index) && index < Count)
            {
                // This was a valid key
                object value = this[index];
                return value is IConfigurationSection section
                    ? (firstSeparator >= 0 ? section.GetSection(key[(key.LastIndexOf(':') + 1) ..]) : section)
                    : new SettingsConfigurationSection(key[(key.LastIndexOf(':') + 1) ..], $"{_path}:{key}", value.ToString());
            }

            // This isn't a valid key, so return a blank section
            return new SettingsConfigurationSection(key[(key.LastIndexOf(':') + 1) ..], $"{_path}:{key}", default);
        }
    }
}
