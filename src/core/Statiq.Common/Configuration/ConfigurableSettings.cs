using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Statiq.Common
{
    public class ConfigurableSettings : IConfigurable
    {
        public ConfigurableSettings(ISettings settings)
        {
            Settings = settings;
        }

        public ISettings Settings { get; }
    }
}
