using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spectre.Cli;
using Wyam.App.Commands;
using Wyam.App.Configuration;
using Wyam.App.Tracing;
using Wyam.Common.Execution;
using Wyam.Core.Execution;

namespace Wyam.App
{
    public class Bootstrapper : IBootstrapper
    {
        private Func<ICommandApp> _getCommandAppFunc = () => new CommandApp();

        public Bootstrapper(string[] args)
        {
            Args = args ?? throw new ArgumentNullException(nameof(args));
        }

        public string[] Args { get; }

        public ConfiguratorCollection<IEngine> EngineConfigurators { get; } = new ConfiguratorCollection<IEngine>();

        public ConfiguratorCollection<IConfigurator> CommandConfigurators { get; } = new ConfiguratorCollection<IConfigurator>();

        public void SetDefaultCommand<TCommand>()
            where TCommand : class, ICommand
        {
            _getCommandAppFunc = () => new CommandApp<TCommand>();
        }

        public int Run()
        {
            // Output version info
            Common.Tracing.Trace.Information($"Wyam version {Engine.Version}");

            // It's not a serious console app unless there's some ASCII art
            OutputLogo();

            // Create the command line parser and run the command
            ICommandApp app = _getCommandAppFunc();
            app.Configure(x =>
            {
                x.ValidateExamples();
                CommandConfigurators.Configure(x);
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

        public static IBootstrapper CreateDefault(string[] args) =>
            CreateDefault(args, (IEngineConfigurator)null);

        public static IBootstrapper CreateDefault<TConfigurator>(string[] args)
            where TConfigurator : IEngineConfigurator =>
            CreateDefault(args, Activator.CreateInstance<TConfigurator>());

        public static IBootstrapper CreateDefault(string[] args, Action<IEngine> configureEngineAction) =>
            CreateDefault(args, new DelegateConfigurator<IEngine>(configureEngineAction));

        public static IBootstrapper CreateDefault(string[] args, IEngineConfigurator configurator) =>
            CreateDefault(args, (Configuration.IConfigurator<IEngine>)configurator);

        private static IBootstrapper CreateDefault(string[] args, Configuration.IConfigurator<IEngine> configurator)
        {
            // Add a default trace listener and tracing for exceptions
            Common.Tracing.Trace.AddListener(new SimpleColorConsoleTraceListener { TraceOutputOptions = System.Diagnostics.TraceOptions.None });
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionEvent;

            // Create the bootstrapper
            IBootstrapper bootstrapper = new Bootstrapper(args);

            // Add default commands
            bootstrapper.SetDefaultCommand<BuildCommand>();
            bootstrapper.CommandConfigurators.AddCommand<BuildCommand>("build");

            // Find and add configurators (add the passed-in one last if not null)
            // Scan assembly?? Settings configurator
            bootstrapper.EngineConfigurators.Add(configurator);

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
