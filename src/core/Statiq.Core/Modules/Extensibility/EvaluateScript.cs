using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Evaluates a C# based script contained in document content.
    /// </summary>
    /// <category>Extensibility</category>
    public class EvaluateScript : ParallelModule
    {
        protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
        {
            // Get the assembly
            byte[] assembly = input.MediaTypeEquals(CompileScript.ScriptMediaType)
                ? await input.GetContentBytesAsync()
                : context.ScriptHelper.Compile(await input.GetContentStringAsync(), input);

            // Evaluate the script
            object value = await context.ScriptHelper.EvaluateAsync(assembly, input);
            return await context.CloneOrCreateDocumentsAsync(input, input.Yield(), value ?? new NullContent());
        }
    }
}
