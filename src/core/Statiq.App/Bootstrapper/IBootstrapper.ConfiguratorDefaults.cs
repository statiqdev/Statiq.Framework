using System;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Cli;
using Statiq.Common;

namespace Statiq.App
{
    public partial interface IBootstrapper
    {
        public IBootstrapper ConfigureSettings(Action<ISettings> action) =>
            Configure<ISettings>(x => action(x));

        public IBootstrapper ConfigureServices(Action<IServiceCollection> action) =>
            Configure<ConfigurableServices>(x => action(x.Services));

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
