using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Debug;
using Microsoft.Extensions.Options;
using Statiq.Common;

namespace Statiq.App
{
    public static class BootstrapperDefaultExtensions
    {
        public static IBootstrapper AddDefaults(this IBootstrapper bootstrapper) =>
            bootstrapper.AddDefaults((IConfigurator<IEngine>)null);

        public static IBootstrapper AddDefaults<TConfigurator>(this IBootstrapper bootstrapper)
            where TConfigurator : IConfigurator<IEngine> =>
            bootstrapper.AddDefaults(Activator.CreateInstance<TConfigurator>());

        public static IBootstrapper AddDefaults(this IBootstrapper bootstrapper, Action<IEngine> configureEngineAction) =>
            bootstrapper.AddDefaults(new DelegateConfigurator<IEngine>(configureEngineAction));

        public static IBootstrapper AddDefaults(this IBootstrapper bootstrapper, IConfigurator<IEngine> configurator) =>
            bootstrapper
                .AddDefaultLogging()
                .AddDefaultConfigurators()
                .AddDefaultCommands()
                .AddDefaultShortcodes()
                .AddDefaultNamespaces()
                .AddConfigurator(configurator);

        public static IBootstrapper AddDefaultConfigurators(this IBootstrapper bootstrapper)
        {
            foreach (IConfigurator<IConfigurableBootstrapper> bootstraperConfigurator
                in bootstrapper.ClassCatalog.GetInstances<IConfigurator<IConfigurableBootstrapper>>())
            {
                bootstrapper.Configurators.Add(bootstraperConfigurator);
            }
            foreach (IConfigurator<IBootstrapper> bootstraperConfigurator
                in bootstrapper.ClassCatalog.GetInstances<IConfigurator<IBootstrapper>>())
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
                    if (shortcode.Equals(typeof(Core.MetaShortcode)))
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

        public static IBootstrapper AddDefaultLogging(this IBootstrapper bootstrapper)
        {
            bootstrapper.AddServices(services => services.AddLogging(logging =>
            {
                logging.AddConsole();
                logging.AddDebug();
            }));
            return bootstrapper;
        }
    }
}
