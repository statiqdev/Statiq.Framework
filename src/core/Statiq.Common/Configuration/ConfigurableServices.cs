using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Statiq.Common
{
    public class ConfigurableServices : IConfigurable
    {
        public ConfigurableServices(IServiceCollection services, ISettings settings)
        {
            Services = services;
            Settings = settings;
        }

        public IServiceCollection Services { get; }

        public ISettings Settings { get; }
    }
}
