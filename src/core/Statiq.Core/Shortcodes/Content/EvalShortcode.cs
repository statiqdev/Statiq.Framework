using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Evaluates C# code.
    /// </summary>
    /// <remarks>The current context and document are in-scope as <c>Context</c> and <c>Document</c> respectively and can be used from within the script.</remarks>
    /// <example>
    /// <code>
    /// &lt;?# Eval ?>&lt;?# return 1 + 2; ?>&lt;?#/ Eval ?>
    /// </code>
    /// </example>
    public class EvalShortcode : ContentShortcode
    {
        public override async Task<string> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context)
        {
            byte[] assembly = context.ScriptHelper.Compile(content, document);
            object value = await context.ScriptHelper.EvaluateAsync(assembly, document);
            return value.ToString();
        }
    }
}
