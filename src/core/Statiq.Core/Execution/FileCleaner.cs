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
        private readonly CleanMode _cleanMode;
        private readonly IFileSystem _fileSystem;
        private readonly ILogger _logger;

        private bool _firstExecution = true;

        internal FileCleaner(CleanMode cleanMode, IFileSystem fileSystem, ILogger logger)
        {
            _cleanMode = cleanMode;
            _fileSystem = fileSystem.ThrowIfNull(nameof(fileSystem));
            _logger = logger.ThrowIfNull(nameof(logger));
        }

        /// <summary>
        /// Cleans folders before execution.
        /// </summary>
        internal void CleanBeforeExecution()
        {
            CleanDirectory(_fileSystem.GetTempDirectory(), "temp");
            if (_cleanMode == CleanMode.Full || _firstExecution)
            {
                CleanDirectory(_fileSystem.GetOutputDirectory(), "output");
            }
            else if (_cleanMode == CleanMode.Self)
            {
                CleanSelf();
            }
            _fileSystem.WriteTracker.Reset();
            _firstExecution = false;
        }

        /// <summary>
        /// Cleans folders after execution.
        /// </summary>
        internal void CleanAfterExecution()
        {
            // TODO: Changed clean
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
        /// </summary>
        private void CleanSelf()
        {
            IDirectory directory = _fileSystem.GetOutputDirectory();

            try
            {
                _logger.LogDebug($"Cleaning files written to output directory {directory.Path.FullPath} during previous execution...");
                int count = 0;
                foreach (KeyValuePair<NormalizedPath, int> write in _fileSystem.WriteTracker.CurrentWrites)
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
    }
}
