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
    public static class BootstrapperDefaultExtensions
    {
        public static Bootstrapper AddDefaults(this Bootstrapper bootstrapper, DefaultFeatures features = DefaultFeatures.All)
        {
            bootstrapper.ThrowIfNull(nameof(bootstrapper));
            if (features.HasFlag(DefaultFeatures.BootstrapperConfigurators))
            {
                bootstrapper.AddBootstrapperConfigurators();
            }
            if (features.HasFlag(DefaultFeatures.Logging))
            {
                bootstrapper.AddDefaultLogging();
            }
            if (features.HasFlag(DefaultFeatures.Settings))
            {
                bootstrapper.AddDefaultSettings();
            }
            if (features.HasFlag(DefaultFeatures.EnvironmentVariables))
            {
                bootstrapper.AddEnvironmentVariables();
            }
            if (features.HasFlag(DefaultFeatures.ConfigurationFiles))
            {
                bootstrapper.AddDefaultConfigurationFiles();
            }
            if (features.HasFlag(DefaultFeatures.BuildCommands))
            {
                bootstrapper.AddBuildCommands();
            }
            if (features.HasFlag(DefaultFeatures.CustomCommands))
            {
                bootstrapper.AddCustomCommands();
            }
            if (features.HasFlag(DefaultFeatures.Shortcodes))
            {
                bootstrapper.AddDefaultShortcodes();
            }
            if (features.HasFlag(DefaultFeatures.Namespaces))
            {
                bootstrapper.AddDefaultNamespaces();
            }
            if (features.HasFlag(DefaultFeatures.Pipelines))
            {
                bootstrapper.AddDefaultPipelines();
            }
            return bootstrapper;
        }

        public static Bootstrapper AddDefaultsWithout(this Bootstrapper bootstrapper, DefaultFeatures withoutFeatures) =>
            bootstrapper.AddDefaults(DefaultFeatures.All & ~withoutFeatures);

        public static Bootstrapper AddBootstrapperConfigurators(this Bootstrapper bootstrapper)
        {
            bootstrapper.ThrowIfNull(nameof(bootstrapper));
            foreach (IConfigurator<IBootstrapper> bootstraperConfigurator
                in bootstrapper.ClassCatalog.GetInstances<IConfigurator<IBootstrapper>>())
            {
                bootstrapper.Configurators.Add(bootstraperConfigurator);
            }
            foreach (IConfigurator<Bootstrapper> bootstraperConfigurator
                in bootstrapper.ClassCatalog.GetInstances<IConfigurator<Bootstrapper>>())
            {
                bootstrapper.Configurators.Add(bootstraperConfigurator);
            }
            return bootstrapper;
        }

        public static Bootstrapper AddDefaultLogging(this Bootstrapper bootstrapper) =>
            bootstrapper.ConfigureServices(services =>
            {
                services.AddSingleton<ILoggerProvider, ConsoleLoggerProvider>();
                services.AddLogging(logging => logging.AddDebug());
            });

        public static Bootstrapper AddDefaultSettings(this Bootstrapper bootstrapper) =>
            bootstrapper.AddSettingsIfNonExisting(
                new Dictionary<string, object>
                {
                    { Keys.LinkHideIndexPages, true },
                    { Keys.LinkHideExtensions, true },
                    { Keys.UseCache, true },
                    { Keys.CleanOutputPath, true }
                });

        public static Bootstrapper AddEnvironmentVariables(this Bootstrapper bootstrapper) =>
            bootstrapper.BuildConfiguration(builder => builder.AddEnvironmentVariables());

        public static Bootstrapper AddDefaultConfigurationFiles(this Bootstrapper bootstrapper) =>
            bootstrapper.BuildConfiguration(builder => builder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddSettingsFile("appsettings")
                .AddSettingsFile("settings")
                .AddSettingsFile("statiq"));

        public static Bootstrapper AddBuildCommands(this Bootstrapper bootstrapper)
        {
            bootstrapper.ThrowIfNull(nameof(bootstrapper));
            bootstrapper.SetDefaultCommand<PipelinesCommand<PipelinesCommandSettings>>();
            bootstrapper.AddCommand<PipelinesCommand<PipelinesCommandSettings>>();
            bootstrapper.AddCommand<DeployCommand>();
            return bootstrapper;
        }

        public static Bootstrapper AddCustomCommands(this Bootstrapper bootstrapper) => bootstrapper.AddCommands();

        public static Bootstrapper AddDefaultShortcodes(this Bootstrapper bootstrapper) =>
            bootstrapper.ConfigureEngine(engine =>
            {
                foreach (Type shortcode in bootstrapper.ClassCatalog.GetTypesAssignableTo<IShortcode>())
                {
                    engine.Shortcodes.Add(shortcode);

                    // Special case for the meta shortcode to register with the name "="
                    if (shortcode.Equals(typeof(Core.MetaShortcode)))
                    {
                        engine.Shortcodes.Add("=", shortcode);
                    }

                    // Special case for the include shortcode to register with the name "^"
                    if (shortcode.Equals(typeof(Core.IncludeShortcode)))
                    {
                        engine.Shortcodes.Add("^", shortcode);
                    }
                }
            });

        public static Bootstrapper AddDefaultNamespaces(this Bootstrapper bootstrapper) =>
            bootstrapper.ConfigureEngine(engine =>
            {
                // Add all Statiq. namespaces
                engine.Namespaces.AddRange(
                    bootstrapper.ClassCatalog.Keys
                        .Where(x => x.StartsWith("Statiq."))
                        .Select(x => x.Substring(0, x.LastIndexOf("."))));

                // Add all module namespaces
                engine.Namespaces.AddRange(
                    bootstrapper.ClassCatalog
                        .GetTypesAssignableTo<IModule>()
                        .Select(x => x.Namespace));

                // Add all namespaces from the entry application
                Assembly entryAssembly = Assembly.GetEntryAssembly();
                if (entryAssembly != null)
                {
                    engine.Namespaces.AddRange(
                        bootstrapper.ClassCatalog
                            .GetTypesFromAssembly(entryAssembly)
                            .Select(x => x.Namespace)
                            .Distinct());
                }
            });

        public static Bootstrapper AddDefaultPipelines(this Bootstrapper bootstrapper) => bootstrapper.AddPipelines();
    }
}
