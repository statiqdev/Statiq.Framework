using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Statiq.CodeAnalysis.Scripting;
using Statiq.Common;

namespace Statiq.CodeAnalysis
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
                : ScriptHelper.Compile(await input.GetContentStringAsync(), input, context);

            // Evaluate the script
            object value = await ScriptHelper.EvaluateAsync(assembly, input, context);
            return await context.CloneOrCreateDocumentsAsync(input, input.Yield(), value);
        }
    }
}
