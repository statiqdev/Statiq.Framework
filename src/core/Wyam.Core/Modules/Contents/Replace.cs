using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Modules;

namespace Wyam.Core.Modules.Contents
{
    /// <summary>
    /// Replaces a search string in the content of each input document with new content.
    /// </summary>
    /// <category>Content</category>
    public class Replace : ContentModule
    {
        private readonly string _search;
        private readonly Func<Match, IDocument, string> _contentFinder;
        private bool _isRegex;
        private RegexOptions _regexOptions = RegexOptions.None;

        /// <summary>
        /// Replaces all occurrences of the search string in every input document with the
        /// string value of the returned object. This allows you to specify different content
        /// for each document depending on the input document.
        /// </summary>
        /// <param name="search">The string to search for.</param>
        /// <param name="content">A delegate that returns the content to replace the search string with.</param>
        public Replace(string search, DocumentConfig<string> content)
            : base(content)
        {
            _search = search;
        }

        /// <summary>
        /// The specified modules are executed against an empty initial document and the resulting
        /// document content replaces all occurrences of the search string in every input document
        /// (possibly creating more than one output document for each input document).
        /// </summary>
        /// <param name="search">The string to search for.</param>
        /// <param name="modules">Modules that output the content to replace the search string with.</param>
        public Replace(string search, params IModule[] modules)
            : base(modules)
        {
            _search = search;
        }

        /// <summary>
        /// Replaces all occurrences of the search string in every input document
        /// with the string value of the objects returned by the delegate. The delegate will be called
        /// for each Match in the supplied regular expression.
        /// </summary>
        /// <param name="search">The string to search for (interpreted as a regular expression).</param>
        /// <param name="contentFinder">A delegate that returns the content to replace the match.</param>
        public Replace(string search, Func<Match, string> contentFinder)
            : base(null as string)
        {
            _search = search;
            _contentFinder = (match, _) => contentFinder(match);
            _isRegex = true;
        }

        /// <summary>
        /// Replaces all occurrences of the search string in every input document
        /// with the string value of the objects returned by the delegate. The delegate will be called
        /// for each Match in the supplied regular expression.
        /// </summary>
        /// <param name="search">The string to search for (interpreted as a regular expression).</param>
        /// <param name="contentFinder">A delegate that returns the content to replace the match.</param>
        public Replace(string search, Func<Match, IDocument, string> contentFinder)
            : base(null as string)
        {
            _search = search;
            _contentFinder = contentFinder;
            _isRegex = true;
        }

        /// <summary>
        /// Indicates that the search string(s) should be treated as a regular expression(s)
        /// with the specified options.
        /// </summary>
        /// <param name="regexOptions">The options to use (if any).</param>
        /// <returns>The current module instance.</returns>
        public Replace IsRegex(RegexOptions regexOptions = RegexOptions.None)
        {
            _isRegex = true;
            _regexOptions = regexOptions;
            return this;
        }

        /// <inheritdoc />
        protected override async Task<IDocument> ExecuteAsync(string content, IDocument input, IExecutionContext context)
        {
            if (input == null)
            {
                return null;
            }
            if (content == null)
            {
                content = string.Empty;
            }
            if (string.IsNullOrEmpty(_search))
            {
                return input;
            }
            string currentDocumentContent = await input.GetStringAsync();
            if (_contentFinder != null)
            {
                string newDocumentContent = Regex.Replace(
                    currentDocumentContent,
                    _search,
                    match => _contentFinder(match, input)?.ToString() ?? string.Empty,
                    _regexOptions);
                return currentDocumentContent == newDocumentContent ? input : await context.NewGetDocumentAsync(input, content: newDocumentContent);
            }
            return await context.NewGetDocumentAsync(
                input,
                content: _isRegex
                    ? Regex.Replace(currentDocumentContent, _search, content, _regexOptions)
                    : currentDocumentContent.Replace(_search, content));
        }
    }
}
