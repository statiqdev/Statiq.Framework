using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Statiq.Common
{
    public class ConfigurableServices : IConfigurable
    {
        public ConfigurableServices(IServiceCollection services, IConfigurationRoot configuration)
        {
            Services = services;
            Configuration = configuration;
        }

        public IServiceCollection Services { get; }

        public IConfigurationRoot Configuration { get; }
    }
}
