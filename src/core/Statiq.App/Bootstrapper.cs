using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Cli;
using Statiq.App.Assemblies;
using Statiq.App.Commands;
using Statiq.App.Configuration;
using Statiq.Common.Configuration;
using Statiq.Common.Execution;
using Statiq.Core.Execution;
using Statiq.Core.Util;

namespace Statiq.App
{
    public class Bootstrapper : IBootstrapper
    {
        private readonly ClassCatalog _classCatalog = new ClassCatalog();

        private readonly ConfiguratorCollection _configurators = new ConfiguratorCollection();

        private Func<ServiceTypeRegistrar, ICommandApp> _getCommandApp = x => new CommandApp(x);

        public Bootstrapper(string[] args)
        {
            Args = args ?? throw new ArgumentNullException(nameof(args));
        }

        public IClassCatalog ClassCatalog => _classCatalog;

        public IConfiguratorCollection Configurators => _configurators;

        public string[] Args { get; }

        public void SetDefaultCommand<TCommand>()
            where TCommand : class, ICommand
        {
            _getCommandApp = x => new CommandApp<TCommand>(x);
        }

        public async Task<int> RunAsync()
        {
            // Remove the synchronization context
            await default(SynchronizationContextRemover);

            // Output version info
            Common.Tracing.Trace.Information($"Statiq version {Engine.Version}");

            // It's not a serious console app unless there's some ASCII art
            OutputLogo();

            // Populate the class catalog (if we haven't already)
            _classCatalog.Populate();

            // Run bootstraper configurators first
            _configurators.Configure<IConfigurableBootstrapper>(this);
            _configurators.Configure<IBootstrapper>(this);

            // Configure the service collection
            IServiceCollection services = CreateServiceCollection();
            services.AddSingleton<IConfigurableBootstrapper>(this);
            services.AddSingleton<IBootstrapper>(this);
            ConfigurableServices configurableServices = new ConfigurableServices(services);
            _configurators.Configure(configurableServices);

            // Create the command line parser and run the command
            ServiceTypeRegistrar registrar = new ServiceTypeRegistrar(services, BuildServiceProvider);
            ICommandApp app = _getCommandApp(registrar);
            app.Configure(x =>
            {
                x.ValidateExamples();
                ConfigurableCommands configurableCommands = new ConfigurableCommands(x);
                _configurators.Configure(configurableCommands);
            });
            return await app.RunAsync(Args);
        }

        protected virtual IServiceCollection CreateServiceCollection() => new ServiceCollection();

        protected virtual IServiceProvider BuildServiceProvider(IServiceCollection serviceCollection) =>
            serviceCollection.BuildServiceProvider();

        // Static factories

        public static IBootstrapper CreateDefault(string[] args) =>
            CreateDefault(args, (Common.Configuration.IConfigurator<IEngine>)null);

        public static IBootstrapper CreateDefault<TConfigurator>(string[] args)
            where TConfigurator : Common.Configuration.IConfigurator<IEngine> =>
            CreateDefault(args, Activator.CreateInstance<TConfigurator>());

        public static IBootstrapper CreateDefault(string[] args, Action<IEngine> configureEngineAction) =>
            CreateDefault(args, new DelegateConfigurator<IEngine>(configureEngineAction));

        public static IBootstrapper CreateDefault(string[] args, Common.Configuration.IConfigurator<IEngine> configurator) =>
            new Bootstrapper(args)
                .AddDefaultTracing()
                .AddDefaultConfigurators()
                .AddDefaultCommands()
                .AddDefaultShortcodes()
                .AddDefaultNamespaces()
                .AddConfigurator(configurator);

        private static void OutputLogo()
        {
            Console.WriteLine(@"
                ,@@@@@@p
              ,@@@@@@@@@@g
            z@@@@@@@@@@@@@@@
          g@@@@@@@@@@@@@@@@@@@,
        g@@@@@@@@@@@@@@@@@@@@@@@,
      ,@@@@@@@@@@@@@@@@@@@@@@@@@@@
     ,@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
     $@@@@@@@@@@@@@@@@@@@@@@@@@@@@@c
     @@@@@@@@@@@@@@@@@@@@@@@@B@@@@@@
     @@@@@@@@@@@@@@@@@@@@@@@  j@@@@@
     $@@@@@@@@@@@@@@@@@@@@@F  #@@@@`
      $@@@@@@@@@@@@@@@@@@P   g@@@@P
       %@@@@@@@@@@@@@     ,g@@@@@P
        3@@@@@@@@@@@@@@@@@@@@@@B`
          `%@@@@@@@@@@@@@@@@@P
             `*%RB@@@@@RRP`");
        }
    }
}
