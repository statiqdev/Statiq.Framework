using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetEscapades.Extensions.Logging.RollingFile;
using Spectre.Cli;
using Statiq.Common;
using Statiq.Core;

namespace Statiq.App
{
    /// <summary>
    /// A base command type that sets up logging and debugging.
    /// </summary>
    /// <typeparam name="TSettings">The type of settings this command uses.</typeparam>
    public abstract class BaseCommand<TSettings> : AsyncCommand<TSettings>
        where TSettings : BaseCommandSettings
    {
        private readonly IConfigurationSettings _configurationSettings;

        protected BaseCommand(
            IConfiguratorCollection configurators,
            IConfigurationSettings configurationSettings,
            IServiceCollection serviceCollection)
        {
            Configurators = configurators;
            ServiceCollection = serviceCollection;
            _configurationSettings = configurationSettings;
        }

        public IConfiguratorCollection Configurators { get; }

        public IServiceCollection ServiceCollection { get; }

        public sealed override async Task<int> ExecuteAsync(CommandContext context, TSettings commandSettings)
        {
            // Set verbose tracing
            if (commandSettings.LogLevel != LogLevel.Information)
            {
                ServiceCollection.Configure<LoggerFilterOptions>(options => options.MinLevel = commandSettings.LogLevel);
            }

            // File logging
            if (!string.IsNullOrEmpty(commandSettings.LogFile))
            {
                // Add the log provider (adding it to the service collection will get picked up by the logger factory)
                ServiceCollection.AddSingleton<ILoggerProvider, FileLoggerProvider>();
                ServiceCollection.Configure<FileLoggerOptions>(options =>
                {
                    options.FileName = commandSettings.LogFile;
                    options.LogDirectory = "logs";
                });
            }

            // Build a temporary service provider so we can log
            // Make sure to place it in it's own scope so transient services get correctly disposed
            IServiceProvider services = ServiceCollection.BuildServiceProvider();
            ClassCatalog classCatalog = services.GetService<ClassCatalog>();
            using (IServiceScope serviceScope = services.CreateScope())
            {
                // Log pending messages
                ILogger logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<Bootstrapper>>();
                logger.LogInformation($"Statiq version {Engine.Version}");
                classCatalog?.LogDebugMessagesTo(logger);

                // Debug/Attach
                if (commandSettings.Debug)
                {
                    logger.LogInformation($"Waiting to launch a debugger for process {Process.GetCurrentProcess().Id}...");
                    Debugger.Launch();
                }
                if ((commandSettings.Attach || commandSettings.Debug) && !Debugger.IsAttached)
                {
                    if (commandSettings.Debug)
                    {
                        // If we got here the debug command was unsuccessful
                        logger.LogInformation($"Could not launch a debugger, waiting for manual attach");
                    }
                    logger.LogInformation($"Waiting for a debugger to attach to process {Process.GetCurrentProcess().Id} (or press a key to continue)...");
                    while (!Debugger.IsAttached && !Console.KeyAvailable)
                    {
                        Thread.Sleep(100);
                    }
                    if (Console.KeyAvailable)
                    {
                        Console.ReadKey(true);
                        logger.LogInformation("Key pressed, continuing execution");
                    }
                    else
                    {
                        logger.LogInformation("Debugger attached, continuing execution");
                    }
                }
            }

            // Add settings
            if (commandSettings.Settings?.Length > 0)
            {
                foreach (KeyValuePair<string, string> setting in SettingsParser.Parse(commandSettings.Settings))
                {
                    _configurationSettings[setting.Key] = setting.Value;
                }
            }

            // Configure settings after other configuration so they can use the values
            ConfigurableSettings configurableSettings = new ConfigurableSettings(_configurationSettings);
            Configurators.Configure(configurableSettings);

            return await ExecuteCommandAsync(context, commandSettings);
        }

        public abstract Task<int> ExecuteCommandAsync(CommandContext context, TSettings settings);
    }
}
