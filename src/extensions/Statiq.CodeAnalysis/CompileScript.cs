using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Statiq.CodeAnalysis.Scripting;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.Meta;
using Statiq.Common.Modules;

namespace Statiq.CodeAnalysis
{
    /// <summary>
    /// Compiles a C# based script contained in document content.
    /// </summary>
    /// <category>Extensibility</category>
    public class CompileScript : IModule
    {
        public const string CompiledKey = "_CompiledScript";

        public async Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context) =>
            await inputs.ParallelSelectAsync(context, async input =>
            {
                byte[] assembly = ScriptHelper.Compile(await input.GetStringAsync(), input, context);
                MemoryStream stream = context.MemoryStreamFactory.GetStream(assembly);
                return input.Clone(
                    new MetadataItems
                    {
                        { CompiledKey, true }
                    },
                    context.GetContentProvider(stream));
            });
    }
}
