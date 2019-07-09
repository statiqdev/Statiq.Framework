using System.Threading.Tasks;
using Statiq.Common.Configuration;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.Modules;

namespace Statiq.Core.Modules.Contents
{
    /// <summary>
    /// Appends the specified content to the existing content of each document.
    /// </summary>
    /// <category>Content</category>
    public class AppendContent : ContentModule
    {
        /// <summary>
        /// Appends the string value of the returned object to to content of each document.
        /// This allows you to specify different content to append for each document depending
        /// on the input document.
        /// </summary>
        /// <param name="content">A delegate that returns the content to append.</param>
        public AppendContent(DocumentConfig<string> content)
            : base(content)
        {
        }

        /// <summary>
        /// The specified modules are executed against an empty initial document and the results
        /// are appended to the content of every input document (possibly creating more
        /// than one output document for each input document).
        /// </summary>
        /// <param name="modules">The modules to execute.</param>
        public AppendContent(params IModule[] modules)
            : base(modules)
        {
        }

        /// <inheritdoc />
        protected override async Task<IDocument> ExecuteAsync(string content, IDocument input, IExecutionContext context)
        {
            if (input == null)
            {
                return context.CreateDocument(await context.GetContentProviderAsync(content));
            }
            return content == null ? input : input.Clone(await context.GetContentProviderAsync(await input.GetStringAsync() + content));
        }
    }
}