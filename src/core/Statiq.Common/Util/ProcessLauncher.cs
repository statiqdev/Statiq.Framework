using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    /// <summary>
    /// A utility class that wraps process launching and provides better tracking and logging.
    /// </summary>
    public class ProcessLauncher : IDisposable
    {
        private readonly ConcurrentCache<int, (Process Process, ILogger Logger, Action<Process> ExitedLogAction)> _processes =
            new ConcurrentCache<int, (Process Process, ILogger Logger, Action<Process> ExitedLogAction)>();

        public ProcessLauncher()
        {
        }

        public ProcessLauncher(string fileName)
        {
            FileName = fileName;
        }

        public ProcessLauncher(string fileName, string arguments)
        {
            FileName = fileName;
            Arguments = arguments;
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
        public bool LogOutput { get; set; }

        /// <summary>
        /// Toggles whether to log error process output as error messages.
        /// </summary>
        public bool LogErrors { get; set; }

        /// <summary>
        /// Toggles throwing an exception if the process exits with a non-zero exit code.
        /// </summary>
        public bool ContinueOnError { get; set; }

        /// <summary>
        /// A function that determines if the exit code from the process was an error.
        /// </summary>
        public Func<int, bool> IsErrorExitCode { get; set; }

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

        public ProcessLauncherResult Start() => Start(null, (ILoggerFactory)null);

        public ProcessLauncherResult Start(Stream outputStream) => Start(outputStream, (ILoggerFactory)null);

        public ProcessLauncherResult Start(ILoggerFactory loggerFactory) => Start(null, loggerFactory);

        public ProcessLauncherResult Start(IServiceProvider serviceProvider) => Start(null, serviceProvider);

        public ProcessLauncherResult Start(Stream outputStream, IServiceProvider serviceProvider) => Start(outputStream, serviceProvider?.GetService<ILoggerFactory>());

        public ProcessLauncherResult Start(Stream outputStream, ILoggerFactory loggerFactory) => Start(outputStream, null, loggerFactory);

        public ProcessLauncherResult Start(Stream outputStream, ILogger logger, IServiceProvider serviceProvider) => Start(outputStream, logger, serviceProvider?.GetService<ILoggerFactory>());

        public ProcessLauncherResult Start(Stream outputStream, ILogger logger, ILoggerFactory loggerFactory)
        {
            if (FileName.IsNullOrWhiteSpace())
            {
                return null;
            }

            // USe a default exit code function if one isn't provided
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
            logger ??= loggerFactory?.CreateLogger<ProcessLauncher>();
            string logCommand = $"{process.StartInfo.FileName}{(HideArguments ? string.Empty : (" " + process.StartInfo.Arguments))}";
            using (StreamWriter outputWriter = IsBackground || outputStream is null ? null : new StreamWriter(outputStream, leaveOpen: true))
            {
                using (StringWriter errorWriter = !IsBackground && ContinueOnError ? new StringWriter() : null)
                {
                    // Write to the stream on data received
                    // If we happen to write before we've created and added the process to the collection, go ahead and do that too
                    process.OutputDataReceived += (_, e) =>
                    {
                        if (!e.Data.IsNullOrEmpty())
                        {
                            (Process Process, ILogger Logger, Action<Process> ExitedLogAction) item = _processes.GetOrAdd(
                                process.Id,
                                _ => (process, loggerFactory?.CreateLogger($"{process.Id}: {Path.GetFileName(startInfo.FileName)}"), (Action<Process>)ExitedLogAction));
                            item.Logger?.Log(LogOutput ? LogLevel.Information : LogLevel.Debug, e.Data);
                            outputWriter?.WriteLine(e.Data);
                        }
                    };
                    process.ErrorDataReceived += (_, e) =>
                    {
                        if (!e.Data.IsNullOrEmpty())
                        {
                            (Process Process, ILogger Logger, Action<Process> ExitedLogAction) item = _processes.GetOrAdd(
                                process.Id,
                                _ => (process, loggerFactory?.CreateLogger($"{process.Id}: {Path.GetFileName(startInfo.FileName)}"), (Action<Process>)ExitedLogAction));
                            item.Logger?.Log(LogErrors ? LogLevel.Error : LogLevel.Debug, e.Data);
                            errorWriter?.WriteLine(e.Data);
                        }
                    };

                    // Start the process
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // Use a separate logger, but only create and add it if it wasn't already from one of the received events
                    _processes.GetOrAdd(
                        process.Id,
                        _ => (process, loggerFactory?.CreateLogger($"{process.Id}: {Path.GetFileName(startInfo.FileName)}"), (Action<Process>)ExitedLogAction));

                    // Log the process command
                    logger?.Log(
                        LogOutput ? LogLevel.Information : LogLevel.Debug,
                        $"Started {(IsBackground ? "background " : string.Empty)}process {process.Id}: {logCommand}");

                    // If this is a background process, let it run and just return the original document
                    if (IsBackground)
                    {
                        return null;
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
                        if (_processes.TryRemove(process.Id, out (Process Process, ILogger Logger, Action<Process> ExitedLogAction) item))
                        {
                            item.ExitedLogAction(item.Process);
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
                    }

                    // Finish the stream and return a document with output as content
                    outputWriter?.Flush();
                    errorWriter?.Flush();
                    string errorData = errorWriter?.ToString();
                    return new ProcessLauncherResult(exitCode, errorData);
                }
            }

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
            Process process = (Process)sender;
            if (_processes.TryRemove(process.Id, out (Process Process, ILogger Logger, Action<Process> ExitedLogAction) item))
            {
                item.ExitedLogAction(item.Process);
            }
        }

        public void Dispose()
        {
            // Close processes that haven't already exited
            foreach ((Process Process, ILogger Logger, Action<Process> ExitedLogAction) item in _processes.Values)
            {
                item.Process.Close();
                item.Process.Exited -= ProcessExited;
            }
            _processes.Clear();
        }
    }
}