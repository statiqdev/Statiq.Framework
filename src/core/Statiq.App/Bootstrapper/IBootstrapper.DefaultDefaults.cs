using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using Microsoft.Extensions.Options;
using Statiq.Common;

namespace Statiq.App
{
    public partial interface IBootstrapper
    {
        public IBootstrapper AddDefaults() =>
            AddDefaults((IConfigurator<IEngine>)null);

        public IBootstrapper AddDefaults<TConfigurator>()
            where TConfigurator : IConfigurator<IEngine> =>
            AddDefaults(Activator.CreateInstance<TConfigurator>());

        public IBootstrapper AddDefaults(Action<IEngine> configureEngineAction) =>
            AddDefaults(new DelegateConfigurator<IEngine>(configureEngineAction));

        public IBootstrapper AddDefaults(IConfigurator<IEngine> configurator) =>
            AddDefaultLogging()
            .AddDefaultConfigurators()
            .AddDefaultCommands()
            .AddDefaultShortcodes()
            .AddDefaultNamespaces()
            .AddConfigurator(configurator);

        public IBootstrapper AddDefaultConfigurators()
        {
            foreach (IConfigurator<IConfigurableBootstrapper> bootstraperConfigurator
                in ClassCatalog.GetInstances<IConfigurator<IConfigurableBootstrapper>>())
            {
                Configurators.Add(bootstraperConfigurator);
            }
            foreach (IConfigurator<IBootstrapper> bootstraperConfigurator
                in ClassCatalog.GetInstances<IConfigurator<IBootstrapper>>())
            {
                Configurators.Add(bootstraperConfigurator);
            }
            return this;
        }

        public IBootstrapper AddDefaultShortcodes() =>
            Configure<IEngine>(engine =>
            {
                foreach (Type shortcode in ClassCatalog.GetAssignableFrom<IShortcode>())
                {
                    engine.Shortcodes.Add(shortcode);

                    // Special case for the meta shortcode to register with the name "="
                    if (shortcode.Equals(typeof(Core.MetaShortcode)))
                    {
                        engine.Shortcodes.Add("=", shortcode);
                    }
                }
            });

        public IBootstrapper AddDefaultNamespaces() =>
            Configure<IEngine>(engine =>
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
                engine.Namespaces.AddRange(
                    ClassCatalog
                        .GetAssignableFrom<IModule>()
                        .Select(x => x.Namespace));
            });

        public IBootstrapper AddDefaultCommands()
        {
            SetDefaultCommand<BuildCommand>();
            AddCommand<BuildCommand>("build");
            AddCommand<PreviewCommand>("preview");
            return this;
        }

        public IBootstrapper AddDefaultLogging()
        {
            AddServices(services =>
            {
                services.AddSingleton<ILoggerProvider, ConsoleLoggerProvider>();
                services.AddLogging(logging => logging.AddDebug());
            });
            return this;
        }
    }
}
