using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Cli;
using Wyam.App.Commands;
using Wyam.App.Configuration;
using Wyam.App.Tracing;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Modules;
using Wyam.Common.Shortcodes;

namespace Wyam.App
{
    public static class BootstrapperConfiguratorExtensions
    {
        public static IBootstrapper AddCommand<TCommand>(this IBootstrapper bootstrapper, string name)
            where TCommand : class, ICommand
        {
            bootstrapper.Configurators.Add(new AddCommandConfigurator<TCommand>(name));
            return bootstrapper;
        }

        public static IBootstrapper AddServices(this IBootstrapper bootstrapper, Action<IServiceCollection> action) =>
            bootstrapper.Configure<ConfigurableServices>(x => action(x.Services));

        public static IBootstrapper Configure<TConfigurable>(this IBootstrapper bootstrapper, Action<TConfigurable> action)
            where TConfigurable : IConfigurable
        {
            bootstrapper.Configurators.Add(action);
            return bootstrapper;
        }

        public static IBootstrapper AddConfigurator<TConfigurable, TConfigurator>(this IBootstrapper bootstrapper)
            where TConfigurable : IConfigurable
            where TConfigurator : Common.Configuration.IConfigurator<TConfigurable>
        {
            bootstrapper.Configurators.Add<TConfigurable, TConfigurator>();
            return bootstrapper;
        }

        public static IBootstrapper AddConfigurator<TConfigurable>(
            this IBootstrapper bootstrapper,
            Common.Configuration.IConfigurator<TConfigurable> configurator)
            where TConfigurable : IConfigurable
        {
            bootstrapper.Configurators.Add(configurator);
            return bootstrapper;
        }
    }
}
