using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Cli;
using Wyam.App.Assemblies;
using Wyam.App.Commands;
using Wyam.App.Configuration;
using Wyam.App.Tracing;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Common.Shortcodes;
using Wyam.Core.Execution;
using Wyam.Core.Util;

namespace Wyam.App
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
            Common.Tracing.Trace.Information($"Wyam version {Engine.Version}");

            // It's not a serious console app unless there's some ASCII art
            OutputLogo();

            // Populate the class catalog (if we haven't already)
            _classCatalog.Populate();

            // Run bootstraper configurators first
            _configurators.Configure<IConfigurableBootstrapper>(this);
            _configurators.Configure<IBootstrapper>(this);

            // Configure the service collection
            IServiceCollection serviceCollection = CreateServiceCollection();
            serviceCollection.AddSingleton<IConfigurableBootstrapper>(this);
            serviceCollection.AddSingleton<IBootstrapper>(this);
            _configurators.Configure(serviceCollection);

            // Create the command line parser and run the command
            ServiceTypeRegistrar registrar = new ServiceTypeRegistrar(serviceCollection, BuildServiceProvider);
            ICommandApp app = _getCommandApp(registrar);
            app.Configure(x =>
            {
                x.ValidateExamples();
                _configurators.Configure(x);
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
                .AddDefaultFileProviders()
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
