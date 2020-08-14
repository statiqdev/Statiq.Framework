using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Statiq.Common
{
    // Implements MetadataDictionary so we can call .GetMetadata() to get nested settings dictionaries
    internal class SettingsConfigurationDictionary : MetadataDictionary, ISettingsConfiguration
    {
        public SettingsConfigurationDictionary()
        {
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
    }
}
