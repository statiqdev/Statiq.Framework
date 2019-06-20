using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Statiq.Common.Configuration;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.IO;
using Statiq.Common.Meta;
using Statiq.Common.Modules;

namespace Statiq.Core.Modules.Metadata
{
    /// <summary>
    /// Optimizes a file name.
    /// </summary>
    /// <remarks>
    /// This module takes the destination file name (or the value of a specified
    /// metadata key) and optimizes it by removing reserved characters, white-listing characters, etc.
    /// </remarks>
    /// <category>Metadata</category>
    public class FileName : IModule
    {
        private readonly List<string> _allowedCharacters = new List<string>();

        internal static readonly string[] ReservedChars = new string[]
        {
            "-", "_", "~", ":", "/", "?", "#", "[", "]",
            "@", "!", "$", "&", "'", "(", ")", "*", "+", ",",
            ";", "=", "}", ";"
        };

        private static readonly Regex FileNameRegex = new Regex("^([a-zA-Z0-9])+$");

        private readonly DocumentConfig<string> _fileName = null;
        private readonly string _outputKey = null;

        /// <summary>
        /// Optimizes the destination file name of each input document.
        /// </summary>
        public FileName()
        {
        }

        /// <summary>
        /// Optimizes the file name stored in the given metadata key and stores it back in the same key.
        /// </summary>
        /// <param name="key">The key containing the file name to optimize.</param>
        public FileName(string key)
        {
            _ = key ?? throw new ArgumentNullException(nameof(key));
            _fileName = Config.FromDocument(doc => doc.String(key));
            _outputKey = key;
        }

        /// <summary>
        /// Optimizes the file name stored in the given metadata key and stores it at the provided key.
        /// </summary>
        /// <param name="inputKey">The metadata key to use for the input file name.</param>
        /// <param name="outputKey">The metadata key to use for the optimized file name.</param>
        public FileName(string inputKey, string outputKey)
        {
            _ = inputKey ?? throw new ArgumentNullException(nameof(inputKey));
            _ = outputKey ?? throw new ArgumentNullException(nameof(outputKey));

            _fileName = Config.FromDocument(doc => doc.String(inputKey));
            _outputKey = outputKey;
        }

        /// <summary>
        /// Optimizes the file name in the resulting path and sets the specified metadata key.
        /// </summary>
        /// <param name="fileName">A delegate that should return a <see cref="string"/> file name to optimize.</param>
        /// <param name="outputKey">The metadata key to use for the optimized file name.</param>
        public FileName(DocumentConfig<string> fileName, string outputKey)
        {
            _ = outputKey ?? throw new ArgumentNullException(outputKey);

            _fileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            _outputKey = outputKey;
        }

        /// <summary>
        /// Specifies the characters to allow in the file name.
        /// </summary>
        /// <param name="allowedCharacters">The allowed characters.</param>
        /// <returns>The current module instance.</returns>
        public FileName WithAllowedCharacters(IEnumerable<string> allowedCharacters)
        {
            _allowedCharacters.AddRange(allowedCharacters);
            return this;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return await inputs.ParallelSelectAsync(context, async input =>
            {
                string fileName = _fileName == null
                    ? input.Destination.FileName.FullPath
                    : await _fileName.GetValueAsync(input, context);

                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    fileName = GetFileName(fileName);
                    if (!string.IsNullOrWhiteSpace(fileName))
                    {
                        if (_fileName == null || string.IsNullOrWhiteSpace(_outputKey))
                        {
                            // No output key so set the destination
                            FilePath path = input.Destination.ChangeFileName(fileName);
                            return input.Clone(path);
                        }
                        else
                        {
                            // Set the specified output key
                            return input.Clone(
                                new MetadataItems
                                {
                                    { _outputKey, fileName }
                                });
                        }
                    }
                }
                return input;
            });
        }

        private string GetFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return string.Empty;
            }

            // Trim whitespace
            fileName = fileName.Trim();

            // Remove multiple dashes
            fileName = Regex.Replace(fileName, @"\-{2,}", string.Empty);

            // Remove reserved chars - doing this as an array reads a lot better than a regex
            foreach (string token in ReservedChars.Except(_allowedCharacters))
            {
                fileName = fileName.Replace(token, string.Empty);
            }

            // Trim dot (special case, only reserved if at beginning or end)
            if (!_allowedCharacters.Contains("."))
            {
                fileName = fileName.Trim('.');
            }

            // Remove multiple spaces
            fileName = Regex.Replace(fileName, @"\s+", " ");

            // Turn spaces into dashes
            fileName = fileName.Replace(" ", "-");

            // Grab letters and numbers only, use a regex to be unicode-friendly
            if (FileNameRegex.IsMatch(fileName))
            {
                fileName = FileNameRegex.Matches(fileName)[0].Value;
            }

            // Urls should not be case-sensitive
            return fileName.ToLowerInvariant();
        }
    }
}
