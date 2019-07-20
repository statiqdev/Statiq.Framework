using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Appends the specified content to the existing content of each document.
    /// </summary>
    /// <category>Content</category>
    public class AppendContent : ConfigModule<string>
    {
        /// <summary>
        /// Appends the string value of the returned object to to content of each document.
        /// This allows you to specify different content to append for each document depending
        /// on the input document.
        /// </summary>
        /// <param name="content">A delegate that returns the content to append.</param>
        public AppendContent(DocumentConfig<string> content)
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
            return new[]
            {
                value == null
                    ? input
                    : input.Clone(await context.GetContentProviderAsync(await input.GetStringAsync() + value))
            };
        }
    }
}