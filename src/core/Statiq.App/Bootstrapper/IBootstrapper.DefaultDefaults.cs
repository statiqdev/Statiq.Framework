using System;
using System.Collections;
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
        public IBootstrapper AddDefaults(DefaultsToAdd defaultsToAdd = DefaultsToAdd.All) =>
            AddDefaults((IConfigurator<IEngine>)null, defaultsToAdd);

        public IBootstrapper AddDefaults<TConfigurator>(DefaultsToAdd defaultsToAdd = DefaultsToAdd.All)
            where TConfigurator : IConfigurator<IEngine> =>
            AddDefaults(Activator.CreateInstance<TConfigurator>(), defaultsToAdd);

        public IBootstrapper AddDefaults(Action<IEngine> configureEngineAction, DefaultsToAdd defaultsToAdd = DefaultsToAdd.All) =>
            AddDefaults(new DelegateConfigurator<IEngine>(configureEngineAction), defaultsToAdd);

        public IBootstrapper AddDefaults(IConfigurator<IEngine> configurator, DefaultsToAdd defaultsToAdd = DefaultsToAdd.All)
        {
            if (defaultsToAdd.HasFlag(DefaultsToAdd.Logging))
            {
                AddDefaultLogging();
            }
            if (defaultsToAdd.HasFlag(DefaultsToAdd.Settings))
            {
                AddDefaultSettings();
            }
            if (defaultsToAdd.HasFlag(DefaultsToAdd.EnvironmentVariables))
            {
                AddEnvironmentVariables();
            }
            if (defaultsToAdd.HasFlag(DefaultsToAdd.Configurators))
            {
                AddDefaultConfigurators();
            }
            if (defaultsToAdd.HasFlag(DefaultsToAdd.Commands))
            {
                AddDefaultCommands();
            }
            if (defaultsToAdd.HasFlag(DefaultsToAdd.Shortcodes))
            {
                AddDefaultShortcodes();
            }
            if (defaultsToAdd.HasFlag(DefaultsToAdd.Namespaces))
            {
                AddDefaultNamespaces();
            }
            if (defaultsToAdd.HasFlag(DefaultsToAdd.Pipelines))
            {
                AddDefaultPipelines();
            }
            return AddConfigurator(configurator);
        }

        public IBootstrapper AddDefaultLogging() =>
            ConfigureServices(services =>
            {
                services.AddSingleton<ILoggerProvider, ConsoleLoggerProvider>();
                services.AddLogging(logging => logging.AddDebug());
            });

        public IBootstrapper AddDefaultSettings() =>
            ConfigureSettings(settings =>
            {
                settings.Add(Keys.LinkHideIndexPages, true);
                settings.Add(Keys.LinkHideExtensions, true);
                settings.Add(Keys.UseCache, true);
                settings.Add(Keys.CleanOutputPath, true);
            });

        public IBootstrapper AddEnvironmentVariables() =>
            ConfigureSettings(settings =>
            {
                foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
                {
                    settings.Add(entry.Key.ToString().ToUpper(), entry.Value);
                }
            });

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

        public IBootstrapper AddDefaultCommands()
        {
            SetDefaultCommand<BuildCommand>();
            AddCommands(typeof(BuildCommand).Assembly);
            AddCommands();
            return this;
        }

        public IBootstrapper AddDefaultShortcodes() =>
            ConfigureEngine(engine =>
            {
                foreach (Type shortcode in ClassCatalog.GetTypesAssignableTo<IShortcode>())
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
            ConfigureEngine(engine =>
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
                        .GetTypesAssignableTo<IModule>()
                        .Select(x => x.Namespace));
            });

        public IBootstrapper AddDefaultPipelines() => AddPipelines();
    }
}
