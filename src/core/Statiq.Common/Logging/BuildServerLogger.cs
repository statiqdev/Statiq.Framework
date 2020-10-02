using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    public class BuildServerLogger
    {
        private static readonly Dictionary<string, (string Prefix, string FilePrefix, string Suffix)> _buildServers =
            new Dictionary<string, (string Prefix, string FilePrefix, string Suffix)>
            {
                { "TF_BUILD", ("##vso[task.logissue type=", ";sourcepath=", "]") },
                { "GITHUB_ACTIONS", ("::", " file=", "::") }
            };

        private readonly IDirectory _repositoryDirectory;
        private readonly ILogger _logger;
        private readonly string _prefix;
        private readonly string _filePrefix;
        private readonly string _suffix;

        internal BuildServerLogger(IExecutionState executionState)
        {
            // Get the current build server
            KeyValuePair<string, (string Prefix, string FilePrefix, string Suffix)> match = _buildServers.FirstOrDefault(x => !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(x.Key)));
            if (match.Key is object)
            {
                _prefix = match.Value.Prefix;
                _filePrefix = match.Value.FilePrefix;
                _suffix = match.Value.Suffix;

                // Get the repository directory
                _repositoryDirectory = executionState.FileSystem.GetRootDirectory();
                while (_repositoryDirectory is object && _repositoryDirectory.GetDirectory(".git") is null)
                {
                    _repositoryDirectory = _repositoryDirectory.Parent;
                }

                // Get a logger with the build server category
                _logger = executionState.Services.GetRequiredService<ILogger<BuildServerLogger>>();
            }
        }

        public void Log(LogLevel logLevel, IDocument document, string message)
        {
            if (_logger is object)
            {
                StringBuilder builder = new StringBuilder(_prefix);
                builder.Append(logLevel == LogLevel.Warning ? "warning" : "error");
                if (document is object && !document.Source.IsNullOrEmpty && _repositoryDirectory is object)
                {
                    NormalizedPath relativePath = _repositoryDirectory.Path.GetRelativePath(document.Source);
                    if (!relativePath.IsNullOrEmpty)
                    {
                        builder.Append(_filePrefix);
                        builder.Append(relativePath.FullPath);
                    }
                }
                builder.Append(_suffix);
                builder.Append(message);
                _logger.Log(logLevel, builder.ToString());
            }
        }
    }
}
