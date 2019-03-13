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
using Wyam.Common.Modules;
using Wyam.Common.Shortcodes;

namespace Wyam.App
{
    public static class BootstrapperExtensions
    {
        public static IBootstrapper AddCommand<TCommand>(this IBootstrapper bootstrapper, string name)
            where TCommand : class, ICommand
        {
            bootstrapper.Configurators.Add(new AddCommandConfigurator<TCommand>(name));
            return bootstrapper;
        }

        public static IBootstrapper AddDefaultConfigurators(this IBootstrapper bootstrapper)
        {
            foreach (Common.Configuration.IConfigurator<IConfigurableBootstrapper> bootstraperConfigurator
                in bootstrapper.ClassCatalog.GetInstances<Common.Configuration.IConfigurator<IConfigurableBootstrapper>>())
            {
                bootstrapper.Configurators.Add(bootstraperConfigurator);
            }
            foreach (Common.Configuration.IConfigurator<IBootstrapper> bootstraperConfigurator
                in bootstrapper.ClassCatalog.GetInstances<Common.Configuration.IConfigurator<IConfigurableBootstrapper>>())
            {
                bootstrapper.Configurators.Add(bootstraperConfigurator);
            }
            return bootstrapper;
        }

        public static IBootstrapper AddDefaultShortcodes(this IBootstrapper bootstrapper) =>
            bootstrapper.Configure<IEngine>(engine =>
            {
                foreach (Type shortcode in bootstrapper.ClassCatalog.GetAssignableFrom<IShortcode>())
                {
                    engine.Shortcodes.Add(shortcode);

                    // Special case for the meta shortcode to register with the name "="
                    if (shortcode.Equals(typeof(Core.Shortcodes.Metadata.Meta)))
                    {
                        engine.Shortcodes.Add("=", shortcode);
                    }
                }
            });

        public static IBootstrapper AddDefaultNamespaces(this IBootstrapper bootstrapper) =>
            bootstrapper.Configure<IEngine>(engine =>
            {
                // Add all Wyam.Common namespaces
                // the JetBrains.Profiler filter is needed due to DotTrace dynamically
                // adding a reference to that assembly when running under its profiler. We want
                // to exclude it.
                engine.Namespaces.AddRange(typeof(IModule).Assembly.GetTypes()
                    .Where(x => !string.IsNullOrWhiteSpace(x.Namespace) && !x.Namespace.StartsWith("JetBrains.Profiler"))
                    .Select(x => x.Namespace)
                    .Distinct());

                // Add all module namespaces
                engine.Namespaces.AddRange(bootstrapper
                    .ClassCatalog
                    .GetAssignableFrom<IModule>()
                    .Select(x => x.Namespace));
            });

        public static IBootstrapper AddDefaultCommands(this IBootstrapper bootstrapper)
        {
            bootstrapper.SetDefaultCommand<BuildCommand>();
            bootstrapper.AddCommand<BuildCommand>("build");
            bootstrapper.AddCommand<PreviewCommand>("preview");
            return bootstrapper;
        }

        public static IBootstrapper AddDefaultTracing(this IBootstrapper bootstrapper)
        {
            Common.Tracing.Trace.AddListener(new SimpleColorConsoleTraceListener
            {
                TraceOutputOptions = System.Diagnostics.TraceOptions.None
            });

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                if (e.ExceptionObject is Exception exception)
                {
                    Common.Tracing.Trace.Critical(exception.ToString());
                }
                Environment.Exit((int)ExitCode.UnhandledError);
            };

            return bootstrapper;
        }

        public static IBootstrapper Configure<T>(this IBootstrapper bootstrapper, Action<T> action)
            where T : class
        {
            bootstrapper.Configurators.Add(action);
            return bootstrapper;
        }

        public static IBootstrapper AddConfigurator<T, TConfigurator>(this IBootstrapper bootstrapper)
            where T : class
            where TConfigurator : class, Common.Configuration.IConfigurator<T>
        {
            bootstrapper.Configurators.Add<T, TConfigurator>();
            return bootstrapper;
        }

        public static IBootstrapper AddConfigurator<T>(
            this IBootstrapper bootstrapper,
            Common.Configuration.IConfigurator<T> configurator)
            where T : class
        {
            bootstrapper.Configurators.Add(configurator);
            return bootstrapper;
        }
    }
}
