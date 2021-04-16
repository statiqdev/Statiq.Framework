using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    public class FileCleaner
    {
        private readonly IFileSystem _fileSystem;
        private readonly ILogger _logger;

        private bool _firstExecution = true;

        internal FileCleaner(CleanMode cleanMode, IFileSystem fileSystem, ILogger logger)
        {
            CleanMode = cleanMode;
            _fileSystem = fileSystem.ThrowIfNull(nameof(fileSystem));
            _logger = logger.ThrowIfNull(nameof(logger));
        }

        public CleanMode CleanMode { get; }

        /// <summary>
        /// Cleans folders before execution.
        /// </summary>
        internal void CleanBeforeExecution()
        {
            _fileSystem.WriteTracker.Reset();
            CleanDirectory(_fileSystem.GetTempDirectory(), "temp");
            if (CleanMode == CleanMode.Full || _firstExecution)
            {
                CleanDirectory(_fileSystem.GetOutputDirectory(), "output");
            }
            else if (CleanMode == CleanMode.Self)
            {
                CleanSelf();
            }
            _firstExecution = false;
        }

        /// <summary>
        /// Cleans folders after execution.
        /// </summary>
        internal void CleanAfterExecution()
        {
            if (CleanMode == CleanMode.Unwritten)
            {
                CleanUnwritten();
            }
        }

        /// <summary>
        /// Recursively deletes a directory and then recreates it.
        /// </summary>
        /// <param name="directory">The directory to clean.</param>
        /// <param name="name">A name for logging purposes.</param>
        public void CleanDirectory(IDirectory directory, string name = null)
        {
            _ = directory.ThrowIfNull(nameof(directory));

            name = name is null ? string.Empty : (name + " ");
            try
            {
                _logger.LogDebug($"Cleaning {name}directory {directory.Path.FullPath}...");
                if (directory.Exists)
                {
                    directory.Delete(true);
                }
                directory.Create();
                _logger.LogInformation($"Cleaned {name}directory {directory.Path.FullPath}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error while cleaning {name}directory {directory.Path.FullPath}: {0} - {1}", ex.GetType(), ex.Message);
            }
        }

        /// <summary>
        /// Deletes all files we wrote on the last execution.
        /// Called before execution.
        /// </summary>
        private void CleanSelf()
        {
            IDirectory directory = _fileSystem.GetOutputDirectory();

            try
            {
                _logger.LogDebug($"Cleaning files written to output directory {directory.Path.FullPath} during previous execution...");
                int count = 0;
                foreach (KeyValuePair<NormalizedPath, int> write in _fileSystem.WriteTracker.PreviousWrites)
                {
                    if (directory.Path.ContainsDescendant(write.Key))
                    {
                        IFile file = _fileSystem.GetFile(write.Key);
                        if (file.Exists)
                        {
                            file.Delete();
                            count++;
                        }
                    }
                }
                _logger.LogInformation($"Cleaned {count} files written to output directory {directory.Path.FullPath} during previous execution");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error while cleaning files written to output directory {directory.Path.FullPath} during previous execution: {0} - {1}", ex.GetType(), ex.Message);
            }
        }

        /// <summary>
        /// Compares what we wrote this time to what we wrote last time and deletes the files that we didn't write this time.
        /// Called after execution.
        /// </summary>
        private void CleanUnwritten()
        {
            IDirectory directory = _fileSystem.GetOutputDirectory();

            try
            {
                _logger.LogDebug($"Cleaning files not written to output directory {directory.Path.FullPath} during current execution...");
                int count = 0;
                foreach (KeyValuePair<NormalizedPath, int> write in _fileSystem.WriteTracker.PreviousWrites)
                {
                    if (directory.Path.ContainsDescendant(write.Key))
                    {
                        IFile file = _fileSystem.GetFile(write.Key);

                        // Only delete it if the file exists and was not written during the current execution
                        if (file.Exists && !_fileSystem.WriteTracker.TryGetCurrentWrite(file.Path, out int _))
                        {
                            file.Delete();
                            count++;
                        }
                    }
                }
                _logger.LogInformation($"Cleaned {count} files not written to output directory {directory.Path.FullPath} during current execution");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error while cleaning files not written to output directory {directory.Path.FullPath} during current execution: {0} - {1}", ex.GetType(), ex.Message);
            }
        }
    }
}
