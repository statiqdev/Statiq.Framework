using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Starts a system process.
    /// </summary>
    /// <category>Extensibility</category>
    public class StartProcess : ConfigModule<string>, IDisposable
    {
        private readonly ConcurrentDictionary<int, Process> _processes = new ConcurrentDictionary<int, Process>();

        private readonly string _fileName;
        private string _workingDirectory;
        private Dictionary<string, string> _environmentVariables;
        private int _timeout = 0;
        private bool _background;
        private bool _logOutput;

        public StartProcess(string fileName)
            : this(fileName, Config.FromValue((string)null))
        {
        }

        public StartProcess(string fileName, string arguments)
            : this(fileName, Config.FromValue(arguments))
        {
        }

        public StartProcess(string fileName, Config<string> arguments)
            : base(arguments, false)
        {
            _fileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        }

        public StartProcess WithWorkingDirectory(string workingDirectory)
        {
            _workingDirectory = workingDirectory;
            return this;
        }

        public StartProcess WithEnvironmentVariables(Dictionary<string, string> environmentVariables)
        {
            _environmentVariables = environmentVariables;
            return this;
        }

        public StartProcess WithTimeout(int timeout)
        {
            _timeout = timeout;
            return this;
        }

        public StartProcess AsBackground(bool background)
        {
            _background = background;
            return this;
        }

        public StartProcess LogOutput(bool logOutput = true)
        {
            _logOutput = logOutput;
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
                RedirectStandardOutput = !_background,
                RedirectStandardError = !_background
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
            if (_environmentVariables != null)
            {
                foreach (KeyValuePair<string, string> variable in _environmentVariables)
                {
                    startInfo.Environment[variable.Key] = variable.Value;
                    startInfo.EnvironmentVariables[variable.Key] = variable.Value;
                }
            }

            // Create the process
            Process process = new Process
            {
                StartInfo = startInfo
            };

            // Prepare the streams
            Stream contentStream = null;
            StreamWriter contentWriter = null;
            if (!_background)
            {
                contentStream = await context.GetContentStreamAsync();
                contentWriter = new StreamWriter(contentStream);
                process.OutputDataReceived += (_, e) =>
                {
                    if (_logOutput)
                    {
                        context.LogInformation(e.Data);
                    }
                    contentWriter.Write(e.Data);
                };
                process.ErrorDataReceived += (_, e) => context.LogError(e.Data);
            }

            // Raises Process.Exited immediately instead of when checked via .WaitForExit() or .HasExited
            process.EnableRaisingEvents = true;
            process.Exited += ProcessExited;

            // Start the process
            process.Start();
            context.LogDebug($"Started process {process.Id}: {process.StartInfo.FileName} {process.StartInfo.Arguments}");

            // If this is a background process, let it run and just return the original document
            if (_background)
            {
                return input.Yield();
            }

            // Start reading the streams
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Otherwise wait for exit
            if (_timeout > 0)
            {
                process.WaitForExit(_timeout);
            }
            else
            {
                process.WaitForExit();
            }

            context.LogDebug($"Process {process.Id} exited with code {process.ExitCode}");
            process.Close();

            // Finish the stream and return a document with output as content
            contentWriter.Flush();
            contentWriter.Dispose();
            return context.CloneOrCreateDocument(input, context.GetContentProvider(contentStream)).Yield();
        }

        private void ProcessExited(object sender, EventArgs e)
        {
            Process process = (Process)sender;
            _processes.TryRemove(process.Id, out _);
        }

        public void Dispose()
        {
            foreach (Process process in _processes.Values)
            {
                process.Exited -= ProcessExited;
                process.Close();
            }
            _processes.Clear();
        }
    }
}