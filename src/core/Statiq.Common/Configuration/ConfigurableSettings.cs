using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Statiq.Common
{
    public class ConfigurableSettings : IConfigurable
    {
        public ConfigurableSettings(IDictionary<string, string> settings)
        {
            Settings = settings;
        }

        public IDictionary<string, string> Settings { get; }
    }
}
