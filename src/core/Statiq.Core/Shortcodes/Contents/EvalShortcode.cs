using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    public class EvalShortcode : Shortcode
    {
        public override async Task<IDocument> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context)
        {
            byte[] assembly = ScriptHelper.Compile(content, document, context);
            object value = await ScriptHelper.EvaluateAsync(assembly, document, context);
            return context.CreateDocument(await context.GetContentProviderAsync(value.ToString()));
        }
    }
}
