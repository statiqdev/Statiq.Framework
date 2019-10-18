using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Cli;
using Statiq.Hosting;
using Statiq.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Statiq.App
{
    [Description("Builds the site and serves it, optionally watching for changes and rebuilding by default.")]
    internal class PreviewCommand : EngineCommand<PreviewCommandSettings>
    {
        private readonly ConcurrentQueue<string> _changedFiles = new ConcurrentQueue<string>();
        private readonly AutoResetEvent _messageEvent = new AutoResetEvent(false);
        private readonly InterlockedBool _exit = new InterlockedBool(false);

        public PreviewCommand(
            IEngineSettingsDictionary engineSettings,
            IConfigurationRoot configurationRoot,
            IServiceCollection serviceCollection,
            IBootstrapper bootstrapper)
            : base(engineSettings, configurationRoot, serviceCollection, bootstrapper)
        {
        }

        protected override async Task<int> ExecuteEngineAsync(
            CommandContext commandContext,
            PreviewCommandSettings commandSettings,
            IEngineManager engineManager)
        {
            ExitCode exitCode = ExitCode.Normal;
            ILogger logger = engineManager.Engine.Services.GetRequiredService<ILogger<Bootstrapper>>();

            // Execute the engine for the first time
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                if (!await engineManager.ExecuteAsync(cancellationTokenSource))
                {
                    return (int)ExitCode.ExecutionError;
                }
            }

            // Start the preview server
            Dictionary<string, string> contentTypes = commandSettings.ContentTypes?.Length > 0
                ? GetContentTypes(commandSettings.ContentTypes)
                : new Dictionary<string, string>();
            ILoggerProvider loggerProvider = engineManager.Engine.Services.GetRequiredService<ILoggerProvider>();
            Server previewServer = await StartPreviewServerAsync(
                engineManager.Engine.FileSystem.GetOutputDirectory().Path,
                commandSettings.Port,
                commandSettings.ForceExt,
                commandSettings.VirtualDirectory,
                !commandSettings.NoReload,
                contentTypes,
                loggerProvider,
                logger);

            // Start the watchers
            ActionFileSystemWatcher inputFolderWatcher = null;
            if (!commandSettings.NoWatch)
            {
                logger.LogInformation("Watching paths(s) {0}", string.Join(", ", engineManager.Engine.FileSystem.InputPaths));
                inputFolderWatcher = new ActionFileSystemWatcher(
                    engineManager.Engine.FileSystem.GetOutputDirectory().Path,
                    engineManager.Engine.FileSystem.GetInputDirectories().Select(x => x.Path),
                    true,
                    "*.*",
                    path =>
                    {
                        _changedFiles.Enqueue(path);
                        _messageEvent.Set();
                    });
            }

            // Start the message pump

            // Only wait for a key if console input has not been redirected, otherwise it's on the caller to exit
            if (!Console.IsInputRedirected)
            {
                // Start the key listening thread
                Thread thread = new Thread(() =>
                {
                    logger.LogInformation("Hit Ctrl-C to exit");
                    Console.TreatControlCAsInput = true;
                    while (true)
                    {
                        // Would have preferred to use Console.CancelKeyPress, but that bubbles up to calling batch files
                        // The (ConsoleKey)3 check is to support a bug in VS Code: https://github.com/Microsoft/vscode/issues/9347
                        ConsoleKeyInfo consoleKey = Console.ReadKey(true);
                        if (consoleKey.Key == (ConsoleKey)3 || (consoleKey.Key == ConsoleKey.C && (consoleKey.Modifiers & ConsoleModifiers.Control) != 0))
                        {
                            _exit.Set();
                            _messageEvent.Set();
                            break;
                        }
                    }
                })
                {
                    IsBackground = true
                };
                thread.Start();
            }

            // Wait for activity
            while (true)
            {
                _messageEvent.WaitOne(); // Blocks the current thread until a signal
                if (_exit)
                {
                    break;
                }

                // Execute if files have changed
                HashSet<string> changedFiles = new HashSet<string>();
                while (_changedFiles.TryDequeue(out string changedFile))
                {
                    if (changedFiles.Add(changedFile))
                    {
                        logger.LogDebug($"{changedFile} has changed");
                    }
                }
                if (changedFiles.Count > 0)
                {
                    logger.LogInformation($"{changedFiles.Count} files have changed, re-executing");

                    // Reset caches when an error occurs during the previous preview
                    string existingResetCacheSetting = null;
                    bool setResetCacheSetting = false;
                    if (exitCode == ExitCode.ExecutionError)
                    {
                        existingResetCacheSetting = engineManager.Engine.Settings.GetString(Keys.ResetCache);
                        setResetCacheSetting = true;
                        EngineSettings[Keys.ResetCache] = "true";
                    }

                    // If there was an execution error due to reload, keep previewing but clear the cache
                    using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
                    {
                        exitCode = await engineManager.ExecuteAsync(cancellationTokenSource)
                            ? ExitCode.Normal
                            : ExitCode.ExecutionError;
                    }

                    // Reset the reset cache setting after removing it
                    if (setResetCacheSetting)
                    {
                        if (existingResetCacheSetting == null)
                        {
                            EngineSettings.Remove(Keys.ResetCache);
                        }
                        {
                            EngineSettings[Keys.ResetCache] = existingResetCacheSetting;
                        }
                    }

                    await previewServer.TriggerReloadAsync();
                }

                // Check one more time for exit
                if (_exit)
                {
                    break;
                }
                logger.LogInformation("Hit Ctrl-C to exit");
                _messageEvent.Reset();
            }

            // Shutdown
            logger.LogInformation("Shutting down");
            inputFolderWatcher?.Dispose();
            previewServer.Dispose();

            return (int)exitCode;
        }

        private static Dictionary<string, string> GetContentTypes(string[] contentTypes)
        {
            Dictionary<string, string> contentTypeDictionary = new Dictionary<string, string>();
            foreach (string contentType in contentTypes)
            {
                string[] splitContentType = contentType.Split('=');
                if (splitContentType.Length != 2)
                {
                    throw new ArgumentException($"Invalid content type {contentType} specified.");
                }
                contentTypeDictionary[splitContentType[0].Trim().Trim('\"')] = splitContentType[1].Trim().Trim('\"');
            }
            return contentTypeDictionary;
        }

        private static async Task<Server> StartPreviewServerAsync(
            DirectoryPath path,
            int port,
            bool forceExtension,
            DirectoryPath virtualDirectory,
            bool liveReload,
            IDictionary<string, string> contentTypes,
            ILoggerProvider loggerProvider,
            ILogger logger)
        {
            Server server;
            try
            {
                server = new Server(path.FullPath, port, !forceExtension, virtualDirectory?.FullPath, liveReload, contentTypes, loggerProvider);
                await server.StartAsync();
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, $"Error while running preview server: {ex}");
                return null;
            }

            string urlPath = server.VirtualDirectory ?? string.Empty;
            logger.LogInformation($"Preview server listening at http://localhost:{port}{urlPath} and serving from path {path}"
                + (liveReload ? " with LiveReload support" : string.Empty));
            return server;
        }
    }
}
