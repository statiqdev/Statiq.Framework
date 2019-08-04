using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Sets a title metadata key for documents based on their file path or source.
    /// </summary>
    /// <remarks>
    /// This will split the title at special characters, capitalize first letters, remove extensions, etc.
    /// </remarks>
    /// <metadata cref="Keys.Title" usage="Output" />
    /// <category>Metadata</category>
    public class AddTitle : IModule
    {
        private static readonly ReadOnlyMemory<char> IndexFileName = "index.".AsMemory();

        private readonly Config<string> _title = Config.FromDocument(GetTitle);
        private string _key = Keys.Title;
        private bool _keepExisting = true;

        /// <summary>
        /// This will use the existing title metadata key if one exists,
        /// otherwise it will set a title based on the document source
        /// or the RelativeFilePath key if no source is available.
        /// </summary>
        public AddTitle()
        {
        }

        /// <summary>
        /// This sets the title of all input documents to a value from the delegate.
        /// </summary>
        /// <param name="title">A delegate that must return a string.</param>
        public AddTitle(Config<string> title)
        {
            _title = title ?? throw new ArgumentNullException(nameof(title));
        }

        /// <summary>
        /// Specifies the key to set for the title. By default this module sets
        /// a value for the key Title.
        /// </summary>
        /// <param name="key">The metadata key to set.</param>
        /// <returns>The current module instance.</returns>
        public AddTitle WithKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(key));
            }

            _key = key;
            return this;
        }

        /// <summary>
        /// Indicates that an existing value in the title key should be kept. The
        /// default value is <c>true</c>. Setting to <c>false</c> will always
        /// set the title metadata to the result of this module, even if the
        /// result is null or empty.
        /// </summary>
        /// <param name="keepExisting">Whether to keep the existing title metadata value.</param>
        /// <returns>The current module instance.</returns>
        public AddTitle KeepExisting(bool keepExisting = true)
        {
            _keepExisting = keepExisting;
            return this;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IDocument>> ExecuteAsync(IExecutionContext context)
        {
            return await context
                .ParallelQueryInputs()
                .SelectAsync(async input =>
                {
                    // Check if there's already a title set
                    if (_keepExisting && input.ContainsKey(_key))
                    {
                        return input;
                    }

                    // Calculate the new title
                    string title = await _title.GetValueAsync(input, context);
                    return title == null
                        ? input
                        : input.Clone(new MetadataItems { { _key, title } });
                });
        }

        /// <summary>
        /// Gets a normalized title given a document.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <returns>A normalized title.</returns>
        public static string GetTitle(IDocument doc) => doc.Source == null ? null : GetTitle(doc.Source);

        /// <summary>
        /// Gets a normalized title given a file path.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>A normalized title.</returns>
        public static string GetTitle(FilePath path)
        {
            // Get the filename, unless an index file, then get containing directory
            ReadOnlyMemory<char> titleMemory = path.Segments[path.Segments.Length - 1];
            if (titleMemory.StartsWith(IndexFileName) && path.Segments.Length > 1)
            {
                titleMemory = path.Segments[path.Segments.Length - 2];
            }

            // Strip the extension(s)
            int extensionIndex = titleMemory.Span.IndexOf('.');
            if (extensionIndex > 0)
            {
                titleMemory = titleMemory.Slice(0, extensionIndex);
            }

            // Decode URL escapes
            string title = WebUtility.UrlDecode(titleMemory.ToString());

            // Replace special characters with spaces
            title = title.Replace('-', ' ').Replace('_', ' ');

            // Join adjacent spaces
            while (title.IndexOf("  ", StringComparison.Ordinal) > 0)
            {
                title = title.Replace("  ", " ");
            }

            // Capitalize
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title);
        }
    }
}