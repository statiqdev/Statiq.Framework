using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A base class for synchronous simple shortcodes that return string content.
    /// </summary>
    public abstract class SimpleSyncShortcode : IShortcode
    {
        public abstract string Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context);

        public async Task<IEnumerable<IDocument>> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context)
        {
            string result = Execute(args, content, document, context);
            if (string.IsNullOrEmpty(result))
            {
                return null;
            }
            return (await context.CreateDocumentAsync(result)).Yield();
        }
    }
}
