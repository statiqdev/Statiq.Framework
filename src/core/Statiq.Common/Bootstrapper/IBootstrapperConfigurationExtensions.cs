using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Statiq.Common
{
    public static class IBootstrapperConfigurationExtensions
    {
        public static TBootstrapper ConfigureSettings<TBootstrapper>(this TBootstrapper bootstrapper, Action<ISettings> action)
            where TBootstrapper : IBootstrapper
        {
            bootstrapper.ThrowIfNull(nameof(bootstrapper));
            bootstrapper.Configurators.Add<ConfigurableSettings>(x => action(x.Settings));
            return bootstrapper;
        }

        public static TBootstrapper BuildConfiguration<TBootstrapper>(this TBootstrapper bootstrapper, Action<IConfigurationBuilder> action)
            where TBootstrapper : IBootstrapper
        {
            bootstrapper.ThrowIfNull(nameof(bootstrapper));
            bootstrapper.Configurators.Add<ConfigurableConfiguration>(x => action(x.Builder));
            return bootstrapper;
        }

        public static TBootstrapper ConfigureServices<TBootstrapper>(this TBootstrapper bootstrapper, Action<IServiceCollection> action)
            where TBootstrapper : IBootstrapper
        {
            bootstrapper.ThrowIfNull(nameof(bootstrapper));
            bootstrapper.Configurators.Add<ConfigurableServices>(x => action(x.Services));
            return bootstrapper;
        }

        public static TBootstrapper ConfigureServices<TBootstrapper>(this TBootstrapper bootstrapper, Action<IServiceCollection, ISettings> action)
            where TBootstrapper : IBootstrapper
        {
            bootstrapper.ThrowIfNull(nameof(bootstrapper));
            bootstrapper.Configurators.Add<ConfigurableServices>(x => action(x.Services, x.Settings));
            return bootstrapper;
        }

        public static TBootstrapper ConfigureEngine<TBootstrapper>(this TBootstrapper bootstrapper, Action<IEngine> action)
            where TBootstrapper : IBootstrapper
        {
            bootstrapper.ThrowIfNull(nameof(bootstrapper));
            bootstrapper.Configurators.Add<IEngine>(x => action(x));
            return bootstrapper;
        }
    }
}
