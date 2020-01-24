using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Cli;
using Statiq.Common;

namespace Statiq.App
{
    public static class BootstrapperConfigurationExtensions
    {
        public static Bootstrapper ConfigureCommands(this Bootstrapper bootstrapper, Action<IConfigurator> action) =>
            bootstrapper.Configure<ConfigurableCommands>(x => action(x.Configurator));

        public static Bootstrapper ConfigureSettings(this Bootstrapper bootstrapper, Action<IConfigurationSettings> action) =>
            bootstrapper.Configure<ConfigurableSettings>(x => action(x.Settings));

        public static Bootstrapper BuildConfiguration(this Bootstrapper bootstrapper, Action<IConfigurationBuilder> action) =>
            bootstrapper.Configure<ConfigurableConfiguration>(x => action(x.Builder));

        public static Bootstrapper ConfigureServices(this Bootstrapper bootstrapper, Action<IServiceCollection> action) =>
            bootstrapper.Configure<ConfigurableServices>(x => action(x.Services));

        public static Bootstrapper ConfigureServices(this Bootstrapper bootstrapper, Action<IServiceCollection, IConfigurationRoot> action) =>
            bootstrapper.Configure<ConfigurableServices>(x => action(x.Services, x.Configuration));

        public static Bootstrapper ConfigureEngine(this Bootstrapper bootstrapper, Action<IEngine> action) =>
            bootstrapper.Configure<IEngine>(x => action(x));

        public static Bootstrapper Configure<TConfigurable>(this Bootstrapper bootstrapper, Action<TConfigurable> action)
            where TConfigurable : IConfigurable
        {
            _ = bootstrapper ?? throw new ArgumentNullException(nameof(bootstrapper));
            bootstrapper.Configurators.Add(action);
            return bootstrapper;
        }

        public static Bootstrapper AddConfigurator<TConfigurable, TConfigurator>(
            this Bootstrapper bootstrapper)
            where TConfigurable : IConfigurable
            where TConfigurator : Common.IConfigurator<TConfigurable>
        {
            _ = bootstrapper ?? throw new ArgumentNullException(nameof(bootstrapper));
            bootstrapper.Configurators.Add<TConfigurable, TConfigurator>();
            return bootstrapper;
        }

        public static Bootstrapper AddConfigurator<TConfigurable>(
            this Bootstrapper bootstrapper,
            Common.IConfigurator<TConfigurable> configurator)
            where TConfigurable : IConfigurable
        {
            _ = bootstrapper ?? throw new ArgumentNullException(nameof(bootstrapper));
            bootstrapper.Configurators.Add(configurator);
            return bootstrapper;
        }
    }
}
