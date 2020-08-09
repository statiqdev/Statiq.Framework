using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Statiq.Common
{
    /// <summary>
    /// A single setting expressed as a configuration section.
    /// </summary>
    internal class SettingsConfigurationSection : IConfigurationSection
    {
        private readonly string _value;

        public SettingsConfigurationSection(string key, string path, string value)
        {
            Key = key.ThrowIfNull(nameof(key));
            Path = path.ThrowIfNull(nameof(path));
            _value = value.ThrowIfNull(nameof(value));
        }

        public string this[string key]
        {
            get => default;
            set => throw new NotSupportedException();
        }

        public string Key { get; }

        public string Path { get; }

        public string Value
        {
            get => _value;
            set => throw new NotSupportedException();
        }

        public IEnumerable<IConfigurationSection> GetChildren() => Array.Empty<IConfigurationSection>();

        public IChangeToken GetReloadToken() => SettingsReloadToken.Instance;

        public IConfigurationSection GetSection(string key) =>
            new SettingsConfigurationSection(key[(key.LastIndexOf(':') + 1) ..], $"{Path}:{key}", default);
    }
}
