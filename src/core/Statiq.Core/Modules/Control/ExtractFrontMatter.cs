using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Extracts the first part of content for each document and sends it to a child module for processing.
    /// </summary>
    /// <remarks>
    /// This module is typically used in conjunction with the Yaml module to enable putting YAML front
    /// matter in a file. First, the content of each input document is scanned for a line that consists
    /// entirely of the delimiter character or (- by default) or the delimiter string. Once found, the
    /// content before the delimiter is passed to the specified child modules. Any metadata from the child
    /// module output document(s) is added to the input document. Note that if the child modules result
    /// in more than one output document, multiple clones of the input document will be made for each one.
    /// The output document content is set to the original content without the front matter.
    /// </remarks>
    /// <category>Control</category>
    public class ExtractFrontMatter : ParentModule
    {
        private readonly string _endDelimiter;
        private readonly bool _endRepeated;
        private bool _ignoreEndDelimiterOnFirstLine = true;
        private string _startDelimiter;
        private bool _startRepeated;
        private bool _preserveFrontMatter;

        /// <summary>
        /// Uses the default delimiter character and passes any front matter to the specified child modules for processing.
        /// </summary>
        /// <param name="modules">The modules to execute against the front matter.</param>
        public ExtractFrontMatter(params IModule[] modules)
            : base(modules)
        {
            _endDelimiter = "-";
            _endRepeated = true;
        }

        /// <summary>
        /// Uses the specified delimiter string and passes any front matter to the specified child modules for processing.
        /// </summary>
        /// <param name="delimiter">The delimiter to use.</param>
        /// <param name="modules">The modules to execute against the front matter.</param>
        public ExtractFrontMatter(string delimiter, params IModule[] modules)
            : base(modules)
        {
            _endDelimiter = delimiter;
            _endRepeated = false;
        }

        /// <summary>
        /// Uses the specified delimiter character and passes any front matter to the specified child modules for processing.
        /// </summary>
        /// <param name="delimiter">The delimiter to use.</param>
        /// <param name="modules">The modules to execute against the front matter.</param>
        public ExtractFrontMatter(char delimiter, params IModule[] modules)
            : base(modules)
        {
            _endDelimiter = new string(delimiter, 1);
            _endRepeated = true;
        }

        /// <summary>
        /// Ignores the delimiter if it appears on the first line. This is useful when processing Jekyll style front matter that
        /// has the delimiter both above and below the front matter content. The default behavior is <c>true</c>.
        /// This setting has no effect if a start delimiter is required.
        /// </summary>
        /// <param name="ignore">If set to <c>true</c>, ignore the delimiter if it appears on the first line.</param>
        /// <returns>The current module instance.</returns>
        public ExtractFrontMatter IgnoreDelimiterOnFirstLine(bool ignore = true)
        {
            _ignoreEndDelimiterOnFirstLine = ignore;
            return this;
        }

        /// <summary>
        /// Requires a start delimiter as the first line of the file.
        /// </summary>
        /// <param name="startDelimiter">The delimiter to require.</param>
        /// <returns>The current module instance.</returns>
        public ExtractFrontMatter RequireStartDelimiter(string startDelimiter)
        {
            _startDelimiter = startDelimiter;
            _startRepeated = false;
            return this;
        }

        /// <summary>
        /// Requires a start delimiter as the first line of the file.
        /// </summary>
        /// <param name="startDelimiter">The delimiter to require.</param>
        /// <returns>The current module instance.</returns>
        public ExtractFrontMatter RequireStartDelimiter(char startDelimiter)
        {
            _startDelimiter = new string(startDelimiter, 1);
            _startRepeated = true;
            return this;
        }

        /// <summary>
        /// By default the front matter is removed from the source file. This allows you to preserve
        /// it in the file for further processing.
        /// </summary>
        /// <param name="preserveFrontMatter">Set to <c>true</c> to preserve the front matter, <c>false</c> to remove it.</param>
        /// <returns>The current module instance.</returns>
        public ExtractFrontMatter PreserveFrontMatter(bool preserveFrontMatter = true)
        {
            _preserveFrontMatter = preserveFrontMatter;
            return this;
        }

        /// <inheritdoc />
        protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
        {
            string inputContent = await input.GetContentStringAsync();
            List<string> inputLines = inputContent.Split(new[] { '\n' }, StringSplitOptions.None).ToList();
            if (inputLines.Count == 0)
            {
                return input.Yield();
            }

            // Find the start delimiter if one is required
            int delimiterLine = -1;
            int startLine = 0;
            if (_startDelimiter is object)
            {
                // We require a start delimiter so verify the first line
                string trimmed = inputLines[0].TrimEnd();
                if (trimmed.Length > 0 && (_startRepeated ? trimmed.All(y => y == _startDelimiter[0]) : trimmed == _startDelimiter))
                {
                    // Found the start delimiter, skip to the next line and look for the end delimiter
                    startLine = 1;
                    delimiterLine = inputLines.FindIndex(1, x =>
                    {
                        string trimmed = x.TrimEnd();
                        return trimmed.Length > 0 && (_endRepeated ? trimmed.All(y => y == _endDelimiter[0]) : trimmed == _endDelimiter);
                    });
                }
                else
                {
                    // No start delimiter to return the document as-is
                    return input.Yield();
                }
            }
            else
            {
                // Find the end delimiter
                delimiterLine = inputLines.FindIndex(x =>
                {
                    string trimmed = x.TrimEnd();
                    return trimmed.Length > 0 && (_endRepeated ? trimmed.All(y => y == _endDelimiter[0]) : trimmed == _endDelimiter);
                });
                startLine = 0;

                // If we found it on the first line and are ignoring, start search again on the next line
                if (delimiterLine == 0 && _ignoreEndDelimiterOnFirstLine)
                {
                    startLine = 1;
                    delimiterLine = inputLines.FindIndex(1, x =>
                    {
                        string trimmed = x.TrimEnd();
                        return trimmed.Length > 0 && (_endRepeated ? trimmed.All(y => y == _endDelimiter[0]) : trimmed == _endDelimiter);
                    });
                }
            }

            // If a delimiter was found, extract the front matter
            if (delimiterLine != -1)
            {
                string frontMatter = string.Join("\n", inputLines.Skip(startLine).Take(delimiterLine - startLine)) + "\n";
                if (!_preserveFrontMatter)
                {
                    inputLines.RemoveRange(0, delimiterLine + 1);
                }
                foreach (IDocument result in await context.ExecuteModulesAsync(Children, input.Clone(await context.GetContentProviderAsync(frontMatter)).Yield()))
                {
                    return result.Clone(
                        _preserveFrontMatter ? input.ContentProvider : await context.GetContentProviderAsync(string.Join("\n", inputLines), input.ContentProvider.MediaType))
                        .Yield();
                }
            }
            else
            {
                return input.Yield();
            }
            return null;
        }
    }
}
