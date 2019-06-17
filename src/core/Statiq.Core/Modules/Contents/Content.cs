using System.Threading.Tasks;
using Statiq.Common.Configuration;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.Modules;

namespace Statiq.Core.Modules.Contents
{
    /// <summary>
    /// Replaces the content of each input document with the string value of the specified content object.
    /// </summary>
    /// <remarks>
    /// In the case where modules are provided, they are executed against an
    /// empty initial document and the results are applied to each input document.
    /// </remarks>
    /// <category>Content</category>
    public class Content : ContentModule
    {
        /// <summary>
        /// Uses the string value of the returned object as the new content for each document. This
        /// allows you to specify different content for each document depending on the input document.
        /// </summary>
        /// <param name="content">A delegate that gets the new content to use.</param>
        public Content(DocumentConfig<string> content)
            : base(content)
        {
        }

        /// <summary>
        /// The specified modules are executed against an empty initial document and the results
        /// are applied to every input document (possibly creating more than one output
        /// document for each input document).
        /// </summary>
        /// <param name="modules">The modules to execute.</param>
        public Content(params IModule[] modules)
            : base(modules)
        {
        }

        /// <inheritdoc />
        protected override async Task<IDocument> ExecuteAsync(string content, IDocument input, IExecutionContext context)
        {
            if (input == null)
            {
                return context.GetDocument(await context.GetContentProviderAsync(content));
            }
            return content == null ? input : context.GetDocument(input, await context.GetContentProviderAsync(content));
        }
    }
}
