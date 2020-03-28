using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A base class for synchronous shortcodes that return a single document.
    /// </summary>
    public abstract class SyncDocumentShortcode : IShortcode
    {
        Task<IEnumerable<IDocument>> IShortcode.ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
            Task.FromResult(Execute(args, content, document, context).Yield());

        public abstract IDocument Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context);
    }
}
