using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using Statiq.Common;
using Statiq.Core;

namespace Statiq.App
{
    /// <summary>
    /// This class can be used from commands to wrap engine execution and apply settings, etc.
    /// </summary>
    internal class EngineManager : IEngineManager, IDisposable
    {
        private readonly ILogger _logger;

        public EngineManager(
            CommandContext commandContext,
            EngineCommandSettings commandSettings,
            Settings settings,
            IServiceCollection serviceCollection,
            Bootstrapper bootstrapper)
        {
            // Get the standard input stream
            string input = null;
            if (commandSettings?.StdIn == true)
            {
                using (StreamReader reader = new StreamReader(Console.OpenStandardInput(), Console.InputEncoding))
                {
                    input = reader.ReadToEnd();
                }
            }

            // Create the application state
            ApplicationState applicationState = new ApplicationState(bootstrapper.Arguments, commandContext.Name, input);

            // Create the engine and get a logger
            // The configuration settings should not be used after this point
            Engine = new Engine(applicationState, serviceCollection, settings, bootstrapper.ClassCatalog);

            // Get the logger from the engine and store it for use during execute
            _logger = Engine.Services.GetRequiredService<ILogger<Bootstrapper>>();

            // Apply command settings
            if (commandSettings is object)
            {
                ApplyCommandSettings(Engine, commandSettings);
            }

            // Verify version
            Engine.LogAndCheckVersion(typeof(Engine).Assembly, "Statiq Framework", Keys.MinimumStatiqFrameworkVersion);

            // Run engine configurators after command line, settings, etc. have been applied
            bootstrapper.Configurators.Configure<IEngine>(Engine);

            // Apply analyzers after configurators (since the configurators register the analyzers)
            Engine.ApplyAnalyzerSettings(commandSettings.Analyzers);

            // Log the full environment
            _logger.LogInformation($"Root path:{Environment.NewLine}       {Engine.FileSystem.RootPath}");
            _logger.LogInformation($"Input path(s):{Environment.NewLine}       {string.Join(Environment.NewLine + "       ", Engine.FileSystem.InputPaths)}");
            if (Engine.FileSystem.ExcludedPaths.Count > 0)
            {
                _logger.LogInformation($"Excluded path(s):{Environment.NewLine}       {string.Join(Environment.NewLine + "       ", Engine.FileSystem.ExcludedPaths)}");
            }
            _logger.LogInformation($"Output path:{Environment.NewLine}       {Engine.FileSystem.OutputPath}");
            _logger.LogInformation($"Temp path:{Environment.NewLine}       {Engine.FileSystem.TempPath}");
            _logger.LogInformation($"Cache path:{Environment.NewLine}       {Engine.FileSystem.CachePath}");
        }

        public Engine Engine { get; }

        public string[] Pipelines { get; set; }

        public bool NormalPipelines { get; set; } = true;

        public async Task<ExitCode> ExecuteAsync(CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                await Engine.ExecuteAsync(Pipelines, NormalPipelines, cancellationTokenSource?.Token ?? CancellationToken.None);
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    // No log message for cancellation
                    return ExitCode.OperationCanceled;
                }

                // Log exceptions not already logged (including those thrown by the engine directly)
                Exception[] exceptions = ex.Unwrap(false).ToArray();
                bool logged = false;
                foreach (Exception exception in exceptions.Where(x => !(x is LoggedException) && !x.Message.IsNullOrWhiteSpace()))
                {
                    _logger.LogCritical(exception.Message);
                    logged = true;
                }
                if (!logged)
                {
                    _logger.Log(LogLevel.Critical, new StatiqLogState { LogToBuildServer = false }, "One or more errors occurred");
                }
                _logger.LogInformation("To get more detailed logging output run with the \"-l Debug\" flag");

                // Unwrapped logged exceptions to figure out error code
                exceptions = exceptions.SelectMany(x => x.Unwrap(true)).ToArray();
                if (exceptions.Any(x => x is LogLevelFailureException))
                {
                    return ExitCode.LogLevelFailure;
                }
                if (exceptions.Any(x => x is ExecutionException))
                {
                    return ExitCode.ExecutionError;
                }
                return ExitCode.UnhandledError;
            }
            return ExitCode.Normal;
        }

        public void Dispose() => Engine.Dispose();

        private static void ApplyCommandSettings(Engine engine, EngineCommandSettings commandSettings)
        {
            // Set folders
            NormalizedPath currentDirectory = Environment.CurrentDirectory;
            engine.FileSystem.RootPath = string.IsNullOrEmpty(commandSettings.RootPath)
                ? currentDirectory
                : currentDirectory.Combine(commandSettings.RootPath);
            if (commandSettings.InputPaths?.Length > 0)
            {
                // Clear existing default paths if new ones are set
                // and reverse the inputs so the last one is first to match the semantics of multiple occurrence single options
                engine.FileSystem.InputPaths.Clear();
                engine.FileSystem.InputPaths.AddRange(commandSettings.InputPaths.Select(x => new NormalizedPath(x)).Reverse());
            }
            if (!string.IsNullOrEmpty(commandSettings.OutputPath))
            {
                engine.FileSystem.OutputPath = commandSettings.OutputPath;
            }

            // Clean mode
            engine.Settings[Keys.CleanMode] = commandSettings.NoClean ? CleanMode.None : commandSettings.CleanMode;

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
        }
    }
}
