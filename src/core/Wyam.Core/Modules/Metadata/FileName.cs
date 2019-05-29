using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;

namespace Wyam.Core.Modules.Metadata
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

        private readonly DocumentConfig<FilePath> _path = Config.FromDocument(doc => doc.Destination);
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
        /// <param name="key">The key containing the path to optimize.</param>
        public FileName(string key)
        {
            _ = key ?? throw new ArgumentNullException(nameof(key));
            _path = Config.FromDocument(doc => doc.FilePath(key));
            _outputKey = key;
        }

        /// <summary>
        /// Optimizes the file name stored in the given metadata key and stores it at the provided key.
        /// </summary>
        /// <param name="inputKey">The metadata key to use for the input filename.</param>
        /// <param name="outputKey">The metadata key to use for the optimized filename.</param>
        public FileName(string inputKey, string outputKey)
        {
            _ = inputKey ?? throw new ArgumentNullException(nameof(inputKey));
            _ = outputKey ?? throw new ArgumentNullException(nameof(outputKey));

            _path = Config.FromDocument(doc => doc.FilePath(inputKey));
            _outputKey = outputKey;
        }

        /// <summary>
        /// Optimizes the file name in the resulting path and sets the specified metadata key.
        /// </summary>
        /// <param name="path">A delegate that should return a <see cref="FilePath"/> to optimize.</param>
        /// <param name="outputKey">The metadata key to use for the optimized filename.</param>
        public FileName(DocumentConfig<FilePath> path, string outputKey)
        {
            _ = outputKey ?? throw new ArgumentNullException(outputKey);

            _path = path ?? throw new ArgumentNullException(nameof(path));
            _outputKey = outputKey;
        }

        /// <summary>
        /// Specifies the characters to allow in the filename.
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
                FilePath path = await _path.GetValueAsync(input, context);

                if (path != null)
                {
                    string fileName = GetFileName(path.FileName.FullPath);
                    if (!string.IsNullOrWhiteSpace(fileName))
                    {
                        path = path.ChangeFileName(fileName);
                        if (string.IsNullOrWhiteSpace(_outputKey))
                        {
                            // No output key so set the destination
                            return context.GetDocument(input, path);
                        }
                        else
                        {
                            // Set the specified output key
                            return context.GetDocument(
                                input,
                                new MetadataItems
                                {
                                    { _outputKey, path }
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
