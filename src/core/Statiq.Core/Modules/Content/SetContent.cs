using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Sets the content of each document.
    /// </summary>
    /// <category>Content</category>
    public class SetContent : ParallelMultiConfigModule
    {
        // Config keys
        private const string Content = nameof(Content);
        private const string MediaType = nameof(MediaType);

        /// <summary>
        /// Sets the content of each document with the config value.
        /// If the value is <c>null</c>, the original input document will be output
        /// (use <see cref="string.Empty"/> to clear the content).
        /// </summary>
        /// <param name="content">A delegate that returns the content to set.</param>
        public SetContent(Config<string> content)
            : this(content, Config.FromDocument(doc => doc.ContentProvider.MediaType))
        {
        }

        /// <summary>
        /// Sets the content of each document with the config value.
        /// If the value is <c>null</c>, the original input document will be output
        /// (use <see cref="string.Empty"/> to clear the content).
        /// </summary>
        /// <param name="content">A delegate that returns the content to set.</param>
        /// <param name="mediaType">The media type of the new content.</param>
        public SetContent(Config<string> content, Config<string> mediaType)
            : base(
                new Dictionary<string, IConfig>
                {
                    { nameof(Content), content },
                    { nameof(MediaType), mediaType }
                },
                true)
        {
        }

        protected override async Task<IEnumerable<IDocument>> ExecuteConfigAsync(IDocument input, IExecutionContext context, IMetadata values)
        {
            string content = values.GetString(Content);
            return content == null
                ? input.Yield()
                : input.Clone(await context.GetContentProviderAsync(content, values.GetString(MediaType))).Yield();
        }
    }
}