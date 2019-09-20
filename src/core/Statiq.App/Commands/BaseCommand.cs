using System;
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
        where TSettings : BaseSettings
    {
        private readonly IServiceCollection _serviceCollection;

        protected BaseCommand(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        public sealed override async Task<int> ExecuteAsync(CommandContext context, TSettings settings)
        {
            // Set verbose tracing
            if (settings.LogLevel != LogLevel.Information)
            {
                _serviceCollection.Configure<LoggerFilterOptions>(options => options.MinLevel = settings.LogLevel);
            }

            // File logging
            if (!string.IsNullOrEmpty(settings.LogFile))
            {
                // Add the log provider (adding it to the service collection will get picked up by the logger factory)
                _serviceCollection.AddSingleton<ILoggerProvider, FileLoggerProvider>();
                _serviceCollection.Configure<FileLoggerOptions>(options =>
                {
                    options.FileName = settings.LogFile;
                    options.LogDirectory = string.Empty;
                });
            }

            // Build a temporary service provider so we can log
            IServiceProvider services = _serviceCollection.BuildServiceProvider();

            // Log pending messages
            ILogger logger = services.GetRequiredService<ILogger<Bootstrapper>>();
            logger.LogInformation($"Statiq version {Engine.Version}");
            ClassCatalog classCatalog = services.GetService<ClassCatalog>();
            classCatalog?.LogDebugMessages(logger);

            // Attach
            if (settings.Attach)
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

            return await ExecuteCommandAsync(context, settings);
        }

        public abstract Task<int> ExecuteCommandAsync(CommandContext context, TSettings settings);
    }
}
