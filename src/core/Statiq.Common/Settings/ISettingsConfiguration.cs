using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Statiq.Common
{
    internal interface ISettingsConfiguration : IConfigurationSection
    {
        public abstract void ResolveScriptMetadataValues(string key, IExecutionState executionState);

        string IConfiguration.this[string key]
        {
            get => GetSection(key).Value;
            set => throw new NotSupportedException();
        }

        string IConfigurationSection.Key => Path[(Path.LastIndexOf(':') + 1) ..];

        string IConfigurationSection.Value
        {
            get => default;
            set => throw new NotSupportedException();
        }

        IChangeToken IConfiguration.GetReloadToken() => SettingsReloadToken.Instance;
    }
}
