using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Replaces the content of each document.
    /// </summary>
    /// <category>Content</category>
    public class ReplaceContent : ConfigModule<string>
    {
        /// <summary>
        /// Replaces the content of each document with the config value.
        /// If the value is <c>null</c>, the original input document will be output
        /// (use <see cref="string.Empty"/> to clear the content).
        /// </summary>
        /// <param name="content">A delegate that returns the content to append.</param>
        public ReplaceContent(Config<string> content)
            : base(content, true)
        {
        }

        /// <inheritdoc />
        protected override async Task<IEnumerable<IDocument>> ExecuteAsync(
            IDocument input,
            IReadOnlyList<IDocument> inputs,
            IExecutionContext context,
            string value)
        {
            if (input == null)
            {
                return context.CreateDocument(await context.GetContentProviderAsync(value)).Yield();
            }
            return value == null
                ? input.Yield()
                : input.Clone(await context.GetContentProviderAsync(value)).Yield();
        }
    }
}