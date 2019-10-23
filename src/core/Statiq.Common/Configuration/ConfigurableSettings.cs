using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Statiq.Common
{
    public class ConfigurableSettings : IConfigurable
    {
        public ConfigurableSettings(IConfigurationSettings settings)
        {
            Settings = settings;
        }

        public IConfigurationSettings Settings { get; }
    }
}
