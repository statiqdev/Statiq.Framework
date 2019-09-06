using System;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Cli;
using Statiq.Common;

namespace Statiq.App
{
    public partial interface IBootstrapper
    {
        public IBootstrapper AddCommand<TCommand>(string name)
            where TCommand : class, ICommand
        {
            Configurators.Add(new AddCommandConfigurator<TCommand>(name));
            return this;
        }

        public IBootstrapper AddServices(Action<IServiceCollection> action) =>
            Configure<ConfigurableServices>(x => action(x.Services));

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
