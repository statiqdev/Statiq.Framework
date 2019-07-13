using Microsoft.Extensions.DependencyInjection;

namespace Statiq.Common
{
    public class ConfigurableServices : IConfigurable
    {
        public ConfigurableServices(IServiceCollection services) => Services = services;

        public IServiceCollection Services { get; }
    }
}
