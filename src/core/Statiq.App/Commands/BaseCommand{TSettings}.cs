using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
        protected BaseCommand(IConfigurationSettingsDictionary configurationSettings, IServiceCollection serviceCollection)
        {
            ConfigurationSettings = configurationSettings;
            ServiceCollection = serviceCollection;
        }

        public IConfigurationSettingsDictionary ConfigurationSettings { get; }

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
                    options.LogDirectory = string.Empty;
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
                classCatalog?.LogDebugMessages(logger);

                // Attach
                if (commandSettings.Attach)
                {
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
                    ConfigurationSettings[setting.Key] = setting.Value;
                }
            }

            return await ExecuteCommandAsync(context, commandSettings);
        }

        public abstract Task<int> ExecuteCommandAsync(CommandContext context, TSettings settings);
    }
}
