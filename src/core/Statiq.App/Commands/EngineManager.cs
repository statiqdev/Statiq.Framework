using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Cli;
using Statiq.Common;
using Statiq.Core;

namespace Statiq.App
{
    /// <summary>
    /// This class can be used from commands to wrap engine execution and apply settings, etc.
    /// </summary>
    public class EngineManager : IDisposable
    {
        private readonly ILogger _logger;
        private readonly string[] _pipelines;
        private readonly bool _defaultPipelines;

        public EngineManager(
            IConfiguration configuration,
            IServiceCollection serviceCollection,
            IBootstrapper bootstrapper,
            ICommand command,
            BuildCommand.Settings settings)
        {
            // Get the standard input stream
            string input = null;
            if (settings?.StdIn == true)
            {
                using (StreamReader reader = new StreamReader(Console.OpenStandardInput(), Console.InputEncoding))
                {
                    input = reader.ReadToEnd();
                }
            }

            // Create the application state
            if (!bootstrapper.CommandNames.TryGetValue(command.GetType(), out string commandName))
            {
                commandName = null;
            }
            ApplicationState applicationState = new ApplicationState(
                bootstrapper.Arguments,
                commandName,
                input);

            // Create the engine and get a logger
            Engine = new Engine(applicationState, configuration, serviceCollection);
            _logger = Engine.Services.GetRequiredService<ILogger<Bootstrapper>>();

            // Apply settings
            bootstrapper.Configurators.Configure(Engine.Settings);
            if (settings != null)
            {
                ApplyCommandSettings(Engine, settings);  // Apply command settings last so they can override others
            }
            _pipelines = settings?.Pipelines;
            _defaultPipelines = settings == null || settings.Pipelines == null || settings.Pipelines.Length == 0 || settings.DefaultPipelines;

            // Run engine configurators after command line, settings, etc. have been applied
            bootstrapper.Configurators.Configure<IEngine>(Engine);

            // Log the full environment
            _logger.LogInformation($"Root path:{Environment.NewLine}       {Engine.FileSystem.RootPath}");
            _logger.LogInformation($"Input path(s):{Environment.NewLine}       {string.Join(Environment.NewLine + "       ", Engine.FileSystem.InputPaths)}");
            _logger.LogInformation($"Output path:{Environment.NewLine}       {Engine.FileSystem.OutputPath}");
            _logger.LogInformation($"Temp path:{Environment.NewLine}       {Engine.FileSystem.TempPath}");
            _logger.LogDebug($"Settings:{Environment.NewLine}       {string.Join(Environment.NewLine + "       ", Engine.Settings.Select(x => $"{x.Key}: {(x.Key.ToUpper() == x.Key ? "****" : (x.Value?.ToString() ?? "null"))}"))}");
        }

        public Engine Engine { get; }

        public async Task<bool> ExecuteAsync(CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                await Engine.ExecuteAsync(_pipelines, _defaultPipelines, cancellationTokenSource);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
            return true;
        }

        public void Dispose() => Engine.Dispose();

        private static void ApplyCommandSettings(Engine engine, BuildCommand.Settings commandSettings)
        {
            // Set folders
            DirectoryPath currentDirectory = Environment.CurrentDirectory;
            engine.FileSystem.RootPath = string.IsNullOrEmpty(commandSettings.RootPath)
                ? currentDirectory
                : currentDirectory.Combine(commandSettings.RootPath);
            if (commandSettings.InputPaths?.Length > 0)
            {
                // Clear existing default paths if new ones are set
                // and reverse the inputs so the last one is first to match the semantics of multiple occurrence single options
                engine.FileSystem.InputPaths.Clear();
                engine.FileSystem.InputPaths.AddRange(commandSettings.InputPaths.Select(x => new DirectoryPath(x)).Reverse());
            }
            if (!string.IsNullOrEmpty(commandSettings.OutputPath))
            {
                engine.FileSystem.OutputPath = commandSettings.OutputPath;
            }
            if (commandSettings.NoClean)
            {
                engine.Settings[Keys.CleanOutputPath] = false;
            }

            // Set no cache if requested
            if (commandSettings.NoCache)
            {
                engine.Settings[Keys.UseCache] = false;
            }

            // Set serial mode
            if (commandSettings.SerialExecution)
            {
                engine.SerialExecution = true;
            }

            // Add settings
            if (commandSettings.MetadataSettings?.Length > 0)
            {
                foreach (KeyValuePair<string, object> setting in MetadataParser.Parse(commandSettings.MetadataSettings))
                {
                    engine.Settings[setting.Key] = setting.Value;
                }
            }
        }
    }
}
