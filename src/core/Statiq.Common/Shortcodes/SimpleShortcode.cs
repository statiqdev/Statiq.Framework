using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A base class for simple shortcodes that return string content.
    /// </summary>
    public abstract class SimpleShortcode : IShortcode
    {
        public abstract Task<string> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context);

        async Task<IEnumerable<IDocument>> IShortcode.ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context)
        {
            string result = await ExecuteAsync(args, content, document, context);
            if (string.IsNullOrEmpty(result))
            {
                return null;
            }
            return (await context.CreateDocumentAsync(result)).Yield();
        }
    }
}
