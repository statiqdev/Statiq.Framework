using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Spectre.Cli;
using Statiq.Common;
using Statiq.Core;

namespace Statiq.App
{
    public class Bootstrapper : IBootstrapper
    {
        private readonly ClassCatalog _classCatalog = new ClassCatalog();

        private readonly ConfiguratorCollection _configurators = new ConfiguratorCollection();

        private Func<CommandServiceTypeRegistrar, ICommandApp> _getCommandApp = x => new CommandApp(x);

        public Bootstrapper(string[] args)
        {
            Args = args ?? throw new ArgumentNullException(nameof(args));
        }

        public IClassCatalog ClassCatalog => _classCatalog;

        public IConfiguratorCollection Configurators => _configurators;

        public string[] Args { get; }

        public IBootstrapper SetDefaultCommand<TCommand>()
            where TCommand : class, ICommand
        {
            _getCommandApp = x => new CommandApp<TCommand>(x);
            return this;
        }

        public async Task<int> RunAsync()
        {
            // Remove the synchronization context
            await default(SynchronizationContextRemover);

            // Populate the class catalog (if we haven't already)
            _classCatalog.Populate();

            // Run bootstrapper configurators first
            _configurators.Configure<IConfigurableBootstrapper>(this);
            _configurators.Configure<IBootstrapper>(this);

            // Create the service collection
            IServiceCollection serviceCollection = CreateServiceCollection() ?? new ServiceCollection();
            serviceCollection.TryAddSingleton<IConfigurableBootstrapper>(this);
            serviceCollection.TryAddSingleton<IBootstrapper>(this);
            serviceCollection.TryAddSingleton(_classCatalog);  // The class catalog is retrieved later for deferred logging once a service provider is built

            // Run configurators on the service collection
            ConfigurableServices configurableServices = new ConfigurableServices(serviceCollection);
            _configurators.Configure(configurableServices);

            // Ensure required engine services are available after running service configurators
            serviceCollection.AddEngineServices();

            // Create the stand-alone command line service container and register a few types needed for the CLI
            CommandServiceTypeRegistrar registrar = new CommandServiceTypeRegistrar();
            registrar.RegisterInstance(typeof(IServiceCollection), serviceCollection);
            registrar.RegisterInstance(typeof(IConfiguratorCollection), _configurators);

            // Create the command line parser and run the command
            ICommandApp app = _getCommandApp(registrar);
            app.Configure(x =>
            {
                x.ValidateExamples();
                ConfigurableCommands configurableCommands = new ConfigurableCommands(x);
                _configurators.Configure(configurableCommands);
            });
            return await app.RunAsync(Args);
        }

        /// <summary>
        /// Creates a service collection for use by the bootstrapper.
        /// </summary>
        /// <remarks>
        /// Override to perform post-creation configuration or to use an alternate service collection type.
        /// </remarks>
        /// <returns>A service collection for use by the bootstrapper.</returns>
        protected virtual IServiceCollection CreateServiceCollection() => null;

        // Static factories

        public static IBootstrapper CreateDefault(string[] args) =>
            new Bootstrapper(args).AddDefaults();

        public static IBootstrapper CreateDefault<TConfigurator>(string[] args)
            where TConfigurator : Common.IConfigurator<IEngine> =>
            new Bootstrapper(args).AddDefaults<TConfigurator>();

        public static IBootstrapper CreateDefault(string[] args, Action<IEngine> configureEngineAction) =>
            new Bootstrapper(args).AddDefaults(configureEngineAction);

        public static IBootstrapper CreateDefault(string[] args, Common.IConfigurator<IEngine> configurator) =>
            new Bootstrapper(args).AddDefaults(configurator);
    }
}
