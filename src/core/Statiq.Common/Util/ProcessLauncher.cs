using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    /// <summary>
    /// A utility class that wraps process launching and provides better tracking and logging.
    /// </summary>
    public class ProcessLauncher : IDisposable
    {
        private readonly ConcurrentCache<int, StartedProcess> _runningProcesses = new ConcurrentCache<int, StartedProcess>();
        private readonly ManualResetEvent _allProcessesExited = new ManualResetEvent(true);

        /// <summary>
        /// Creates a new process launcher without a defined file name or arguments.
        /// </summary>
        public ProcessLauncher()
        {
        }

        /// <summary>
        /// Creates a new process launcher with the specified file name and arguments.
        /// </summary>
        /// <param name="fileName">The file name of the process to start.</param>
        /// <param name="arguments">The arguments to pass to the process.</param>
        public ProcessLauncher(string fileName, params string[] arguments)
        {
            FileName = fileName;
            if (arguments is object)
            {
                Arguments = string.Join(" ", arguments.Where(argument => !argument.IsNullOrEmpty()));
            }
        }

        /// <summary>
        /// The file name of the process to start.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The arguments to pass to the process.
        /// </summary>
        public string Arguments { get; set; }

        /// <summary>
        /// The working directory to use for the process.
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Environment variables to set for the process.
        /// </summary>
        public Dictionary<string, string> EnvironmentVariables { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Sets a timeout in milliseconds before the process will be terminated.
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// Starts the process and leaves it running in the background.
        /// </summary>
        public bool IsBackground { get; set; }

        /// <summary>
        /// Toggles whether to hide the arguments list when logging the process command.
        /// </summary>
        public bool HideArguments { get; set; }

        /// <summary>
        /// Toggles whether to log standard process output as information messages.
        /// </summary>
        public bool LogOutput { get; set; } = true;

        /// <summary>
        /// Toggles whether to log error process output as error messages.
        /// </summary>
        public bool LogErrors { get; set; } = true;

        /// <summary>
        /// Toggles throwing an exception if the process exits with a non-zero exit code.
        /// </summary>
        public bool ContinueOnError { get; set; }

        /// <summary>
        /// A function that determines if the exit code from the process was an error.
        /// </summary>
        public Func<int, bool> IsErrorExitCode { get; set; }

        /// <summary>
        /// Returns <c>true</c> if any processes launched by this launcher are currently running.
        /// </summary>
        public bool AreAnyRunning => _runningProcesses.Count > 0;

        public IEnumerable<Process> RunningProcesses => _runningProcesses.Select(x => x.Value.Process);

        /// <summary>
        /// Waits for all launched processes to exit (useful for background processes).
        /// If any processes are launched while waiting, those will also be waited on.
        /// </summary>
        public void WaitForRunningProcesses() => _allProcessesExited.WaitOne();

        public ProcessLauncher WithArgument(string argument, bool quoted = false)
        {
            if (!Arguments.IsNullOrEmpty())
            {
                Arguments += " ";
            }
            if (argument is object)
            {
                Arguments += quoted ? $"\"{argument}\"" : argument;
            }
            return this;
        }

        public ProcessLauncher WithEnvironmentVariables(IEnumerable<KeyValuePair<string, string>> environmentVariables)
        {
            foreach (KeyValuePair<string, string> environmentVariable in environmentVariables)
            {
                WithEnvironmentVariable(environmentVariable.Key, environmentVariable.Value);
            }
            return this;
        }

        public ProcessLauncher WithEnvironmentVariable(string name, string value)
        {
            EnvironmentVariables[name] = value;
            return this;
        }

        public int StartNew(CancellationToken cancellationToken = default) =>
            StartNew(null, null, (ILoggerFactory)null, cancellationToken);

        public int StartNew(TextWriter outputWriter, TextWriter errorWriter, CancellationToken cancellationToken = default) =>
            StartNew(outputWriter, errorWriter, (ILoggerFactory)null, cancellationToken);

        public int StartNew(ILoggerFactory loggerFactory, CancellationToken cancellationToken = default) =>
            StartNew(null, null, loggerFactory, cancellationToken);

        public int StartNew(IServiceProvider serviceProvider, CancellationToken cancellationToken = default) =>
            StartNew(null, null, serviceProvider, cancellationToken);

        public int StartNew(TextWriter outputWriter, TextWriter errorWriter, IServiceProvider serviceProvider, CancellationToken cancellationToken = default) =>
            StartNew(outputWriter, errorWriter, serviceProvider?.GetService<ILoggerFactory>(), cancellationToken);

        public int StartNew(TextWriter outputWriter, TextWriter errorWriter, ILogger logger, CancellationToken cancellationToken = default) =>
            StartNew(outputWriter, errorWriter, new LoggerFactoryLoggerWrapper(logger), cancellationToken);

        public int StartNew(TextWriter outputWriter, TextWriter errorWriter, ILogger logger, IServiceProvider serviceProvider, CancellationToken cancellationToken = default) =>
            StartNew(outputWriter, errorWriter, serviceProvider?.GetService<ILoggerFactory>() ?? new LoggerFactoryLoggerWrapper(logger), cancellationToken);

        public int StartNew(TextWriter outputWriter, TextWriter errorWriter, ILoggerFactory loggerFactory, CancellationToken cancellationToken = default)
        {
            if (FileName.IsNullOrWhiteSpace())
            {
                return 0;
            }

            // Block waiters until the process exits
            _allProcessesExited.Reset();

            // Use a default exit code function if one isn't provided
            Func<int, bool> isErrorExitCode = IsErrorExitCode ?? (x => x != 0);

            // Create the process start info
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = FileName,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            // Set arguments
            if (!Arguments.IsNullOrEmpty())
            {
                startInfo.Arguments = Arguments;
            }

            // Set working directory
            if (!WorkingDirectory.IsNullOrWhiteSpace())
            {
                startInfo.WorkingDirectory = WorkingDirectory;
            }

            // Set environment variables
            foreach (KeyValuePair<string, string> environmentVariable in EnvironmentVariables)
            {
                if (!environmentVariable.Key.IsNullOrEmpty())
                {
                    startInfo.Environment[environmentVariable.Key] = environmentVariable.Value;
                    startInfo.EnvironmentVariables[environmentVariable.Key] = environmentVariable.Value;
                }
            }

            // Create the process
            Process process = new Process
            {
                StartInfo = startInfo
            };

            // Raises Process.Exited immediately instead of when checked via .WaitForExit() or .HasExited
            process.EnableRaisingEvents = true;
            process.Exited += ProcessExited;

            // Prepare the streams
            ILogger logger = loggerFactory?.CreateLogger<ProcessLauncher>();
            string logCommand = $"{process.StartInfo.FileName}{(HideArguments ? string.Empty : (" " + process.StartInfo.Arguments))}";

            // Write to the stream on data received
            // If we happen to write before we've created and added the process to the collection, go ahead and do that too
            process.OutputDataReceived += (_, e) =>
            {
                if (!e.Data.IsNullOrEmpty())
                {
                    StartedProcess startedProcess = _runningProcesses.GetOrAdd(
                        process.Id,
                        _ => new StartedProcess(process, loggerFactory, ExitedLogAction));
                    startedProcess.Logger?.Log(LogOutput ? LogLevel.Information : LogLevel.Debug, e.Data);
                    outputWriter?.WriteLine(e.Data);
                }
            };
            process.ErrorDataReceived += (_, e) =>
            {
                if (!e.Data.IsNullOrEmpty())
                {
                    StartedProcess startedProcess = _runningProcesses.GetOrAdd(
                        process.Id,
                        _ => new StartedProcess(process, loggerFactory, ExitedLogAction));
                    startedProcess.Logger?.Log(LogErrors ? LogLevel.Error : LogLevel.Debug, e.Data);
                    errorWriter?.WriteLine(e.Data);
                }
            };

            // Log starting
            logger?.Log(LogLevel.Debug, $"Starting {(IsBackground ? "background " : string.Empty)}process: {logCommand}");

            // Start the process
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Use a separate logger, but only create and add it if it wasn't already from one of the received events
            // Either way, register a cancellation handler and set the cancellation token registration here
            _runningProcesses.GetOrAdd(process.Id, _ => new StartedProcess(process, loggerFactory, ExitedLogAction))
                .CancellationTokenRegistration = cancellationToken.Register(() =>
                {
                    if (_runningProcesses.TryRemove(process.Id, out StartedProcess startedProcess))
                    {
                        try
                        {
                            process.Kill(true);
                        }
                        catch (Exception ex)
                        {
                            logger?.LogWarning($"Could not kill process {process.Id}: {ex.Message}");
                        }
                        startedProcess.Process.Exited -= ProcessExited;
                        startedProcess.CancellationTokenRegistration.Dispose();
                    }

                    // Allow waiters to run if there are no more processes
                    if (_runningProcesses.Count == 0)
                    {
                        _allProcessesExited.Set();
                    }
                });

            // Log start
            logger?.Log(
                LogOutput ? LogLevel.Information : LogLevel.Debug,
                $"Started {(IsBackground ? "background " : string.Empty)}process {process.Id}: {logCommand}");

            // If this is a background process, let it run and just return the original document
            if (IsBackground)
            {
                return 0;
            }

            // Otherwise wait for exit
            int exitCode = 0;
            try
            {
                if (Timeout > 0)
                {
                    if (process.WaitForExit(Timeout))
                    {
                        // To ensure that asynchronous event handling has been completed, call the WaitForExit() overload that takes no parameter after receiving a true from this overload.
                        // From https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.process.waitforexit?redirectedfrom=MSDN&view=netcore-3.1#System_Diagnostics_Process_WaitForExit_System_Int32_
                        // See also https://github.com/dotnet/runtime/issues/27128
                        process.WaitForExit();
                    }
                }
                else
                {
                    process.WaitForExit();
                }

                // Log exit (synchronous)
                exitCode = process.ExitCode;
                if (_runningProcesses.TryRemove(process.Id, out StartedProcess startedProcess))
                {
                    startedProcess.ExitedLogAction(startedProcess.Process);
                }

                // Only throw if running synchronously
                if (isErrorExitCode(exitCode) && !ContinueOnError)
                {
                    throw new Exception(GetExitLogMessage(process, true));
                }
            }
            finally
            {
                process.Close();

                // Allow waiters to run if there are no more processes
                if (_runningProcesses.Count == 0)
                {
                    _allProcessesExited.Set();
                }
            }

            // Finish the stream and return a document with output as content
            outputWriter?.Flush();
            errorWriter?.Flush();
            return exitCode;

            // Logging that occurs on process exit
            void ExitedLogAction(Process p)
            {
                bool errorExitCode = isErrorExitCode(process.ExitCode);
                string logMessage = GetExitLogMessage(p, errorExitCode);
                logger?.Log(LogOutput ? (errorExitCode ? LogLevel.Error : LogLevel.Information) : LogLevel.Debug, logMessage);
            }

            string GetExitLogMessage(Process p, bool errorExitCode) => $"Process {p.Id} exited with {(errorExitCode ? "error " : string.Empty)}code {p.ExitCode}: {logCommand}";
        }

        // Log exit (asynchronous)
        private void ProcessExited(object sender, EventArgs e)
        {
            // Remove the process and log exit
            Process process = (Process)sender;
            if (_runningProcesses.TryRemove(process.Id, out StartedProcess startedProcess))
            {
                startedProcess.ExitedLogAction(startedProcess.Process);
            }

            // Allow waiters to run if there are no more processes
            if (_runningProcesses.Count == 0)
            {
                _allProcessesExited.Set();
            }
        }

        public void Dispose()
        {
            // Kill processes that haven't already exited
            foreach (StartedProcess startedProcess in _runningProcesses.Values)
            {
                try
                {
                    startedProcess.Process.Kill(true);
                }
                catch
                {
                }
                startedProcess.Process.Exited -= ProcessExited;
                startedProcess.CancellationTokenRegistration.Dispose();
            }
            _runningProcesses.Clear();
            _allProcessesExited.Set();
        }

        private class StartedProcess
        {
            public StartedProcess(
                Process process,
                ILoggerFactory loggerFactory,
                Action<Process> exitedLogAction)
            {
                Process = process;
                Logger = loggerFactory?.CreateLogger($"{process.Id}: {Path.GetFileName(process.StartInfo.FileName)}");
                ExitedLogAction = exitedLogAction;
            }

            public Process Process { get; }

            public ILogger Logger { get; }

            public Action<Process> ExitedLogAction { get; }

            public CancellationTokenRegistration CancellationTokenRegistration { get; set; }
        }

        /// <summary>
        /// Always provides a wrapped <see cref="ILogger"/>.
        /// </summary>
        private class LoggerFactoryLoggerWrapper : ILoggerFactory
        {
            private readonly ILogger _logger;

            public LoggerFactoryLoggerWrapper(ILogger logger)
            {
                _logger = logger;
            }

            public void Dispose()
            {
            }

            public ILogger CreateLogger(string categoryName) => _logger;

            public void AddProvider(ILoggerProvider provider) => throw new NotSupportedException();
        }
    }
}