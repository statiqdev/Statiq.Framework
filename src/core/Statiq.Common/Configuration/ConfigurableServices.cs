using Microsoft.Extensions.DependencyInjection;

namespace Statiq.Common.Configuration
{
    public class ConfigurableServices : IConfigurable
    {
        public ConfigurableServices(IServiceCollection services) => Services = services;

        public IServiceCollection Services { get; }
    }
}
