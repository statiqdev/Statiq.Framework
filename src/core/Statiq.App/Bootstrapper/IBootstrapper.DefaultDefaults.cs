using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using Microsoft.Extensions.Options;
using Statiq.Common;

namespace Statiq.App
{
    public partial interface IBootstrapper
    {
        public IBootstrapper AddDefaults(DefaultFeatures features = DefaultFeatures.All)
        {
            if (features.HasFlag(DefaultFeatures.BootstrapperConfigurators))
            {
                AddBootstrapperConfigurators();
            }
            if (features.HasFlag(DefaultFeatures.Logging))
            {
                AddDefaultLogging();
            }
            if (features.HasFlag(DefaultFeatures.Settings))
            {
                AddDefaultSettings();
            }
            if (features.HasFlag(DefaultFeatures.EnvironmentVariables))
            {
                AddEnvironmentVariables();
            }
            if (features.HasFlag(DefaultFeatures.ConfigurationFiles))
            {
                AddDefaultConfigurationFiles();
            }
            if (features.HasFlag(DefaultFeatures.BuildCommands))
            {
                AddBuildCommands();
            }
            if (features.HasFlag(DefaultFeatures.HostingCommands))
            {
                AddHostingCommands();
            }
            if (features.HasFlag(DefaultFeatures.CustomCommands))
            {
                AddCustomCommands();
            }
            if (features.HasFlag(DefaultFeatures.Shortcodes))
            {
                AddDefaultShortcodes();
            }
            if (features.HasFlag(DefaultFeatures.Namespaces))
            {
                AddDefaultNamespaces();
            }
            if (features.HasFlag(DefaultFeatures.Pipelines))
            {
                AddDefaultPipelines();
            }
            return this;
        }

        public IBootstrapper AddDefaultsWithout(DefaultFeatures withoutFeatures) =>
            AddDefaults(DefaultFeatures.All & ~withoutFeatures);

        public IBootstrapper AddBootstrapperConfigurators()
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

        public IBootstrapper AddDefaultLogging() =>
            ConfigureServices(services =>
            {
                services.AddSingleton<ILoggerProvider, ConsoleLoggerProvider>();
                services.AddLogging(logging => logging.AddDebug());
            });

        public IBootstrapper AddDefaultSettings() =>
            AddSettingsIfNonExisting(
                new Dictionary<string, string>
                {
                    { Keys.LinkHideIndexPages, "true" },
                    { Keys.LinkHideExtensions, "true" },
                    { Keys.UseCache, "true" },
                    { Keys.CleanOutputPath, "true" }
                });

        public IBootstrapper AddEnvironmentVariables() =>
            BuildConfiguration(builder => builder.AddEnvironmentVariables());

        public IBootstrapper AddDefaultConfigurationFiles() =>
            BuildConfiguration(builder => builder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("statiq.json", true));

        public IBootstrapper AddBuildCommands()
        {
            SetDefaultCommand<PipelinesCommand<PipelinesCommandSettings>>();
            AddCommand<PipelinesCommand<PipelinesCommandSettings>>();
            AddCommand<DeployCommand>();
            AddCommands();
            return this;
        }

        public IBootstrapper AddHostingCommands()
        {
            AddCommand<PreviewCommand>();
            AddCommand<ServeCommand>();
            return this;
        }

        public IBootstrapper AddCustomCommands()
        {
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

                // Add all namespaces from the entry app
                Assembly entryAssembly = Assembly.GetEntryAssembly();
                if (entryAssembly != null)
                {
                    engine.Namespaces.AddRange(
                        ClassCatalog
                            .GetTypesFromAssembly(entryAssembly)
                            .Select(x => x.Namespace)
                            .Distinct());
                }
            });

        public IBootstrapper AddDefaultPipelines() => AddPipelines();
    }
}
