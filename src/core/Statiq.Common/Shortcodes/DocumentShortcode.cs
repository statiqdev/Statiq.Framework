using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A base class for shortcodes that return a single document.
    /// </summary>
    public abstract class DocumentShortcode : IShortcode
    {
        async Task<IEnumerable<IDocument>> IShortcode.ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
            (await ExecuteAsync(args, content, document, context)).Yield();

        public abstract Task<IDocument> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context);
    }
}
