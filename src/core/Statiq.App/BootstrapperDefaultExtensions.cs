using System;
using System.Linq;
using Statiq.App.Commands;
using Statiq.App.Tracing;
using Statiq.Common.Configuration;
using Statiq.Common.Execution;
using Statiq.Common.IO;
using Statiq.Common.Modules;
using Statiq.Common.Shortcodes;

namespace Statiq.App
{
    public static class BootstrapperDefaultExtensions
    {
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
                // Add all Statiq.Common namespaces
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
    }
}
