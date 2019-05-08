using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Common.Shortcodes
{
    internal class FuncShortcode : IShortcode
    {
        private readonly Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, Task<IDocument>> _func;

        public FuncShortcode(Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, Task<IDocument>> func)
        {
            _func = func;
        }

        public async Task<IDocument> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
            await _func?.Invoke(args, content, document, context);
    }
}
