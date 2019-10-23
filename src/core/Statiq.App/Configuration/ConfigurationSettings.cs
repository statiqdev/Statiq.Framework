using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Statiq.Common;

namespace Statiq.App
{
    internal class ConfigurationSettings : ConfigurationMetadata, IConfigurationSettings
    {
        private readonly SettingsConfigurationProvider _settingsProvider;

        public ConfigurationSettings(SettingsConfigurationProvider settingsProvider, IConfiguration configuration)
            : base(configuration)
        {
            _settingsProvider = settingsProvider;
        }

        string IConfigurationSettings.this[string key]
        {
            get => TryGetValue(key, out string value) ? value : throw new KeyNotFoundException();
            set => _settingsProvider[key] = value;
        }

        string IDictionary<string, string>.this[string key]
        {
            get => ((IConfigurationSettings)this)[key];
            set => ((IConfigurationSettings)this)[key] = value;
        }

        ICollection<string> IConfigurationSettings.Keys => Keys.ToArray();

        ICollection<string> IDictionary<string, string>.Keys => ((IConfigurationSettings)this).Keys;

        ICollection<string> IConfigurationSettings.Values => ((IEnumerable<KeyValuePair<string, string>>)this).Select(x => x.Value).ToArray();

        ICollection<string> IDictionary<string, string>.Values => ((IConfigurationSettings)this).Values;

        public bool IsReadOnly => false;

        public void Add(string key, string value) => _settingsProvider.Add(key, value);

        public void Add(KeyValuePair<string, string> item) => _settingsProvider.Add(item);

        public bool Contains(KeyValuePair<string, string> item) => _settingsProvider.Contains(item);

        public void Clear() => throw new NotSupportedException();

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value) => TryGetValue<string>(key, out value);

        IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator() =>
            Keys.Select(x => new KeyValuePair<string, string>(x, ((IMetadata)this).GetString(x))).GetEnumerator();

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex) => throw new NotSupportedException();

        public bool Remove(string key) => throw new NotSupportedException();

        public bool Remove(KeyValuePair<string, string> item) => throw new NotSupportedException();
    }
}
