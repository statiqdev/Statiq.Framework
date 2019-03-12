using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Cli;
using Scrutor;
using Wyam.App.Commands;
using Wyam.App.Configuration;
using Wyam.App.Tracing;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Core.Execution;

namespace Wyam.App
{
    public class Bootstrapper : ICommandBootstrapper
    {
        private readonly ConfiguratorCollection _configurators = new ConfiguratorCollection();

        private Func<ServiceTypeRegistrar, ICommandApp> _getCommandApp = x => new CommandApp(x);

        private Func<IServiceCollection, IServiceProvider> _buildServiceProvider = x => x.BuildServiceProvider();

        public Bootstrapper(string[] args)
        {
            Args = args ?? throw new ArgumentNullException(nameof(args));
        }

        public IConfiguratorCollection Configurators => _configurators;

        public string[] Args { get; }

        public void SetDefaultCommand<TCommand>()
            where TCommand : class, ICommand
        {
            _getCommandApp = x => new CommandApp<TCommand>(x);
        }

        /// <summary>
        /// Use this to set an alternate service provider builder method. For example, this can
        /// be used to swap out the default service container for a different one.
        /// </summary>
        /// <param name="buildServiceProvider">A function used to build the <see cref="IServiceProvider"/>.</param>
        public void BuildServiceProvider(Func<IServiceCollection, IServiceProvider> buildServiceProvider) =>
            _buildServiceProvider = buildServiceProvider ?? throw new ArgumentNullException(nameof(buildServiceProvider));

        public int Run()
        {
            // Output version info
            Common.Tracing.Trace.Information($"Wyam version {Engine.Version}");

            // It's not a serious console app unless there's some ASCII art
            OutputLogo();

            // Run bootstraper configurators first using an intermediate service provider
            _configurators.Configure<IBootstrapper>(this);

            // Configure the service collection
            ServiceCollection serviceCollection = new ServiceCollection();
            _configurators.Configure<IServiceCollection>(serviceCollection);

            // Create the engine and configure it
            Engine engine = new Engine();
            serviceCollection.AddSingleton<IEngine>(engine);
            _configurators.Configure<IEngine>(engine);

            // Create the command line parser and run the command
            ServiceTypeRegistrar registrar = new ServiceTypeRegistrar(serviceCollection, _buildServiceProvider);
            ICommandApp app = _getCommandApp(registrar);
            app.Configure(x =>
            {
                x.ValidateExamples();
                _configurators.Configure(x);
            });
            return app.Run(Args);
        }

        // Static factories

        public static ICommandBootstrapper CreateDefault(string[] args) =>
            CreateDefault(args, (Common.Configuration.IConfigurator<IEngine>)null);

        public static ICommandBootstrapper CreateDefault<TConfigurator>(string[] args)
            where TConfigurator : Common.Configuration.IConfigurator<IEngine> =>
            CreateDefault(args, Activator.CreateInstance<TConfigurator>());

        public static ICommandBootstrapper CreateDefault(string[] args, Action<IEngine> configureEngineAction) =>
            CreateDefault(args, new DelegateConfigurator<IEngine>(configureEngineAction));

        private static ICommandBootstrapper CreateDefault(string[] args, Common.Configuration.IConfigurator<IEngine> configurator)
        {
            // Add a default trace listener and tracing for exceptions
            Common.Tracing.Trace.AddListener(new SimpleColorConsoleTraceListener { TraceOutputOptions = System.Diagnostics.TraceOptions.None });
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionEvent;

            // Create the bootstrapper
            Bootstrapper bootstrapper = new Bootstrapper(args);

            // Scan and add boostrapper configurators using a temporary service collection
            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.Scan(x => x
                .FromApplicationDependencies()
                .AddClasses(c => c.AssignableTo<Common.Configuration.IConfigurator<IBootstrapper>>())
                .As<Common.Configuration.IConfigurator<IBootstrapper>>()
                .WithSingletonLifetime());
            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            foreach (Common.Configuration.IConfigurator<IBootstrapper> bootstraperConfigurator in
                serviceProvider.GetServices<Common.Configuration.IConfigurator<IBootstrapper>>())
            {
                bootstrapper.Configurators.Add(bootstraperConfigurator);
            }

            // Add default commands
            bootstrapper.SetDefaultCommand<BuildCommand>();
            bootstrapper.AddCommand<BuildCommand>("build");

            // Add the explicit engine configurator
            bootstrapper.Configurators.Add(configurator);

            return bootstrapper;
        }

        // Static helpers

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

        private static void UnhandledExceptionEvent(object sender, UnhandledExceptionEventArgs e)
        {
            // Exit with a error exit code
            if (e.ExceptionObject is Exception exception)
            {
                Common.Tracing.Trace.Critical(exception.ToString());
            }
            Environment.Exit((int)ExitCode.UnhandledError);
        }
    }
}
