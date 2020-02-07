using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Compiles a C# based script contained in document content.
    /// </summary>
    /// <category>Extensibility</category>
    public class CompileScript : ParallelModule
    {
        public const string ScriptMediaType = "application/x.csx";

        protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
        {
            byte[] assembly = context.ScriptHelper.Compile(await input.GetContentStringAsync(), input);
            MemoryStream stream = context.MemoryStreamFactory.GetStream(assembly);
            return input.Clone(context.GetContentProvider(stream, ScriptMediaType)).Yield();
        }
    }
}
