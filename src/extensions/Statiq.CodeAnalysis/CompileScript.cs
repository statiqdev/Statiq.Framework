using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Statiq.CodeAnalysis.Scripting;
using Statiq.Common;

namespace Statiq.CodeAnalysis
{
    /// <summary>
    /// Compiles a C# based script contained in document content.
    /// </summary>
    /// <category>Extensibility</category>
    public class CompileScript : ParallelModule
    {
        public const string CompiledKey = "_CompiledScript";

        protected override async Task<IEnumerable<IDocument>> ExecuteAsync(IDocument input, IExecutionContext context)
        {
            byte[] assembly = ScriptHelper.Compile(await input.GetStringAsync(), input, context);
            MemoryStream stream = context.MemoryStreamFactory.GetStream(assembly);
            return input
                .Clone(
                    new MetadataItems
                    {
                        { CompiledKey, true }
                    },
                    context.GetContentProvider(stream))
                .Yield();
        }
    }
}
