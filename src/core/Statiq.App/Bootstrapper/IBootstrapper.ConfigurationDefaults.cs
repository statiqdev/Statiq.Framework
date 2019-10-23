using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Cli;
using Statiq.Common;

namespace Statiq.App
{
    public partial interface IBootstrapper
    {
        public IBootstrapper ConfigureCommands(Action<IConfigurator> action) =>
            Configure<ConfigurableCommands>(x => action(x.Configurator));

        public IBootstrapper ConfigureSettings(Action<IConfigurationSettings> action) =>
            Configure<ConfigurableSettings>(x => action(x.Settings));

        public IBootstrapper BuildConfiguration(Action<IConfigurationBuilder> action) =>
            Configure<ConfigurableConfiguration>(x => action(x.Builder));

        public IBootstrapper ConfigureServices(Action<IServiceCollection> action) =>
            Configure<ConfigurableServices>(x => action(x.Services));

        public IBootstrapper ConfigureServices(Action<IServiceCollection, IConfigurationRoot> action) =>
            Configure<ConfigurableServices>(x => action(x.Services, x.Configuration));

        public IBootstrapper ConfigureEngine(Action<IEngine> action) =>
            Configure<IEngine>(x => action(x));

        public IBootstrapper Configure<TConfigurable>(Action<TConfigurable> action)
            where TConfigurable : IConfigurable
        {
            Configurators.Add(action);
            return this;
        }

        public IBootstrapper AddConfigurator<TConfigurable, TConfigurator>()
            where TConfigurable : IConfigurable
            where TConfigurator : Common.IConfigurator<TConfigurable>
        {
            Configurators.Add<TConfigurable, TConfigurator>();
            return this;
        }

        public IBootstrapper AddConfigurator<TConfigurable>(Common.IConfigurator<TConfigurable> configurator)
            where TConfigurable : IConfigurable
        {
            Configurators.Add(configurator);
            return this;
        }
    }
}
