using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    internal class FuncShortcode : IShortcode
    {
        private readonly Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, Task<IEnumerable<IDocument>>> _func;

        public FuncShortcode(Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, Task<IEnumerable<IDocument>>> func)
        {
            _func = func;
        }

        public FuncShortcode(Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, Task<IDocument>> func)
        {
            _func = async (a, b, c, d) => func != null ? (await func(a, b, c, d)).Yield() : null;
        }

        public async Task<IEnumerable<IDocument>> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
            await _func?.Invoke(args, content, document, context);
    }
}
