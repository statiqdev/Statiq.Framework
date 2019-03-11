using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Cli;
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

        private Func<ServiceTypeRegistrar, ICommandApp> _getCommandAppFunc = x => new CommandApp(x);

        public Bootstrapper(string[] args)
        {
            Args = args ?? throw new ArgumentNullException(nameof(args));
        }

        public IConfiguratorCollection Configurators => _configurators;

        public string[] Args { get; }

        public void SetDefaultCommand<TCommand>()
            where TCommand : class, ICommand
        {
            _getCommandAppFunc = x => new CommandApp<TCommand>(x);
        }

        public int Run()
        {
            // Output version info
            Common.Tracing.Trace.Information($"Wyam version {Engine.Version}");

            // It's not a serious console app unless there's some ASCII art
            OutputLogo();

            // Run bootstraper configurators first
            _configurators.Configure<IBootstrapper>(this);

            // Create the engine and configure it
            Engine engine = new Engine();
            _configurators.Configure<IEngine>(engine);

            // Create the service collection and configure it
            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IEngine>(engine);
            _configurators.Configure<IServiceCollection>(serviceCollection);

            // Create the command line parser and run the command
            ServiceTypeRegistrar registrar = new ServiceTypeRegistrar(serviceCollection);
            ICommandApp app = _getCommandAppFunc(registrar);
            app.Configure(x =>
            {
                x.ValidateExamples();
                _configurators.Configure(x);
            });
            return app.Run(Args);
        }

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
            ICommandBootstrapper bootstrapper = new Bootstrapper(args);

            // Add default commands
            bootstrapper.SetDefaultCommand<BuildCommand>();
            bootstrapper.AddCommand<BuildCommand>("build");

            // Scan all dependencies for IBoostrapper configurators and add those

            // Add the explicit engine configurator
            bootstrapper.Configurators.Add(configurator);

            return bootstrapper;
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
