using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Starts a system process.
    /// </summary>
    /// <remarks>
    /// This module can start both foreground and background processes. If the process
    /// is a foreground process (the default), the module will wait for it to return
    /// and output a document with the process standard output as it's content.
    /// If the process is a background process, the module will fork it and let it run,
    /// but no output document will be generated and it will log with a debug level.
    /// </remarks>
    /// <category>Extensibility</category>
    public class StartProcess : ConfigModule<string>, IDisposable
    {
        public const string ExitCode = nameof(ExitCode);
        public const string ErrorData = nameof(ErrorData);

        private readonly ConcurrentDictionary<int, (Process, ILogger)> _processes = new ConcurrentDictionary<int, (Process, ILogger)>();
        private readonly Dictionary<string, string> _environmentVariables = new Dictionary<string, string>();

        private readonly string _fileName;
        private string _workingDirectory;
        private int _timeout;
        private bool _background;
        private bool _logOutput;
        private bool _continueOnError;

        /// <summary>
        /// Starts a process for the specified file name.
        /// </summary>
        /// <param name="fileName">The file name of the process to start.</param>
        public StartProcess(string fileName)
            : this(fileName, Config.FromValue((string)null))
        {
        }

        /// <summary>
        /// Starts a process for the specified file name and arguments.
        /// </summary>
        /// <param name="fileName">The file name of the process to start.</param>
        /// <param name="arguments">The arguments to pass to the process.</param>
        public StartProcess(string fileName, string arguments)
            : this(fileName, Config.FromValue(arguments))
        {
        }

        /// <summary>
        /// Starts a process for the specified file name and arguments.
        /// </summary>
        /// <param name="fileName">The file name of the process to start.</param>
        /// <param name="arguments">The arguments to pass to the process.</param>
        public StartProcess(string fileName, Config<string> arguments)
            : base(arguments, false)
        {
            _fileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        }

        /// <summary>
        /// Sets the working directory to use for the process.
        /// </summary>
        /// <param name="workingDirectory">The working directory.</param>
        /// <returns>The current module instance.</returns>
        public StartProcess WithWorkingDirectory(string workingDirectory)
        {
            _workingDirectory = workingDirectory;
            return this;
        }

        /// <summary>
        /// Sets process-specific environment variables.
        /// </summary>
        /// <param name="environmentVariables">The environment variables to set.</param>
        /// <returns>The current module instance.</returns>
        public StartProcess WithEnvironmentVariables(IEnumerable<KeyValuePair<string, string>> environmentVariables)
        {
            _ = environmentVariables ?? throw new ArgumentNullException(nameof(environmentVariables));
            foreach (KeyValuePair<string, string> environmentVariable in environmentVariables)
            {
                _environmentVariables[environmentVariable.Key] = environmentVariable.Value;
            }
            return this;
        }

        /// <summary>
        /// Sets a process-specific environment variable.
        /// </summary>
        /// <param name="environmentVariable">The name and value of the environment variable to set.</param>
        /// <returns>The current module instance.</returns>
        public StartProcess WithEnvironmentVariable(KeyValuePair<string, string> environmentVariable)
        {
            _environmentVariables[environmentVariable.Key] = environmentVariable.Value;
            return this;
        }

        /// <summary>
        /// Sets a timeout in milliseconds before the process will be terminated.
        /// </summary>
        /// <remarks>
        /// This has no effect for background processes.
        /// </remarks>
        /// <param name="timeout">The timeout in milliseconds.</param>
        /// <returns>The current module instance.</returns>
        public StartProcess WithTimeout(int timeout)
        {
            _timeout = timeout;
            return this;
        }

        /// <summary>
        /// Starts the process and leaves it running in the background.
        /// </summary>
        /// <remarks>
        /// If the process is a background process, the module will fork it and let it run,
        /// but no output document will be generated and it will log with a debug level.
        /// </remarks>
        /// <param name="background"><c>true</c> to start this process in the background, <c>false</c> otherwise.</param>
        /// <returns>The current module instance.</returns>
        public StartProcess AsBackground(bool background = true)
        {
            _background = background;
            return this;
        }

        /// <summary>
        /// Toggles whether to log process output.
        /// </summary>
        /// <remarks>
        /// By default, process output is only logged as debug messages. Output to standard error will always be logged.
        /// </remarks>
        /// <param name="logOutput"><c>true</c> to log process output, <c>false</c> otherwise.</param>
        /// <returns>The current module instance.</returns>
        public StartProcess LogOutput(bool logOutput = true)
        {
            _logOutput = logOutput;
            return this;
        }

        /// <summary>
        /// Toggles throwing an exception if the process exits with a non-zero exit code.
        /// </summary>
        /// <remarks>
        /// By default the module will throw an exception if the process exits with a non-zero exit code.
        /// </remarks>
        /// <param name="continueOnError"><c>true</c> to continue when the process exits with a non-zero exit code, <c>false</c> to throw an exception.</param>
        /// <returns>The current module instance.</returns>
        public StartProcess ContinueOnError(bool continueOnError = true)
        {
            _continueOnError = continueOnError;
            return this;
        }

        protected override async Task<IEnumerable<IDocument>> ExecuteConfigAsync(IDocument input, IExecutionContext context, string value)
        {
            // Create the process start info
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = _fileName,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            // Set arguments
            if (!string.IsNullOrEmpty(value))
            {
                startInfo.Arguments = value;
            }

            // Set working directory
            if (string.IsNullOrWhiteSpace(_workingDirectory))
            {
                startInfo.WorkingDirectory = context.FileSystem.RootPath.FullPath;
            }
            else
            {
                startInfo.WorkingDirectory = context.FileSystem.RootPath.Combine(_workingDirectory).FullPath;
            }

            // Set environment variables for the process
            foreach (KeyValuePair<string, string> environmentVariable in _environmentVariables)
            {
                startInfo.Environment[environmentVariable.Key] = environmentVariable.Value;
                startInfo.EnvironmentVariables[environmentVariable.Key] = environmentVariable.Value;
            }

            // Create the process
            Process process = new Process
            {
                StartInfo = startInfo
            };

            // Use a separate logger if a background job
            ILogger logger = context;
            if (_background)
            {
                ILoggerFactory loggerFactory = context.GetService<ILoggerFactory>();
                if (loggerFactory != null)
                {
                    logger = loggerFactory.CreateLogger(string.Empty);
                }
            }

            // Prepare the streams
            using (Stream contentStream = _background ? null : await context.GetContentStreamAsync())
            {
                using (StreamWriter contentWriter = contentStream == null ? null : new StreamWriter(contentStream))
                {
                    using (StringWriter errorWriter = !_background && _continueOnError ? new StringWriter() : null)
                    {
                        // Write to the stream on data received
                        process.OutputDataReceived += (_, e) =>
                        {
                            if (!string.IsNullOrEmpty(e.Data))
                            {
                                logger?.Log(_logOutput ? LogLevel.Information : LogLevel.Debug, e.Data);
                                contentWriter?.WriteLine(e.Data);
                            }
                        };
                        process.ErrorDataReceived += (_, e) =>
                        {
                            if (!string.IsNullOrEmpty(e.Data))
                            {
                                logger?.LogError(e.Data);
                                errorWriter?.WriteLine(e.Data);
                            }
                        };

                        // Raises Process.Exited immediately instead of when checked via .WaitForExit() or .HasExited
                        process.EnableRaisingEvents = true;
                        process.Exited += ProcessExited;

                        // Start the process
                        process.Start();
                        logger?.Log(
                            _logOutput ? LogLevel.Information : LogLevel.Debug,
                            $"Started {(_background ? "background " : string.Empty)}process {process.Id}: {process.StartInfo.FileName} {process.StartInfo.Arguments}");
                        _processes.TryAdd(process.Id, (process, logger));

                        // Start reading the streams
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();

                        // If this is a background process, let it run and just return the original document
                        if (_background)
                        {
                            return input.Yield();
                        }

                        // Otherwise wait for exit
                        int exitCode = 0;
                        try
                        {
                            if (_timeout > 0)
                            {
                                process.WaitForExit(_timeout);
                            }
                            else
                            {
                                process.WaitForExit();
                            }

                            // Log exit code and throw if non-zero
                            exitCode = process.ExitCode;
                            if (process.ExitCode != 0 && !_continueOnError)
                            {
                                throw new ExecutionException($"Process {process.Id} exited with non-zero code {process.ExitCode}");
                            }
                        }
                        finally
                        {
                            process.Close();
                        }

                        // Finish the stream and return a document with output as content
                        contentWriter?.Flush();
                        errorWriter?.Flush();
                        string errorData = errorWriter?.ToString();
                        MetadataItems metadata = new MetadataItems
                        {
                            { ExitCode, exitCode }
                        };
                        if (!string.IsNullOrEmpty(errorData))
                        {
                            metadata.Add(ErrorData, errorData);
                        }
                        return context.CloneOrCreateDocument(input, metadata, context.GetContentProvider(contentStream)).Yield();
                    }
                }
            }
        }

        private void ProcessExited(object sender, EventArgs e)
        {
            Process process = (Process)sender;
            if (_processes.TryRemove(process.Id, out (Process, ILogger) item))
            {
                item.Item2.Log(
                    _logOutput ? LogLevel.Information : LogLevel.Debug,
                    $"Process {process.Id} exited with code {process.ExitCode}");
            }
        }

        public void Dispose()
        {
            foreach ((Process, ILogger) item in _processes.Values)
            {
                item.Item1.Close();
                item.Item1.Exited -= ProcessExited;
            }
            _processes.Clear();
        }
    }
}