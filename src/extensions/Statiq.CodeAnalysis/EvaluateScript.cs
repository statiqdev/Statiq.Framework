using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Statiq.CodeAnalysis.Scripting;
using Statiq.Common.Content;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.Meta;
using Statiq.Common.Modules;

namespace Statiq.CodeAnalysis
{
    public class EvaluateScript : IModule
    {
        private bool _parallel;

        public EvaluateScript(bool parallel = true)
        {
            _parallel = parallel;
        }

        public async Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return _parallel
                ? await inputs.ParallelSelectManyAsync(context, GetResults)
                : await inputs.SelectManyAsync(context, GetResults);

            async Task<IEnumerable<IDocument>> GetResults(IDocument input)
            {
                // Get the assembly
                byte[] assembly = input.Bool(CompileScript.CompiledKey)
                    ? await input.GetBytesAsync()
                    : ScriptHelper.Compile(await input.GetStringAsync(), input, context);

                // Evaluate the script
                object result = await ScriptHelper.EvaluateAsync(assembly, input, context);
                if (result == null)
                {
                    return new[] { input };
                }
                return GetDocuments(result)
                    ?? await ExecuteModulesAsync(result, context, new[] { input })
                    ?? await ChangeContentAsync(result, context, input);
            }
        }

        private static IEnumerable<IDocument> GetDocuments(object result) =>
            result is IDocument document ? new[] { document } : result as IEnumerable<IDocument>;

        private static async Task<IEnumerable<IDocument>> ExecuteModulesAsync(object results, IExecutionContext context, IEnumerable<IDocument> inputs)
        {
            // Check for a single IModule first since some modules also implement IEnumerable<IModule>
            IEnumerable<IModule> modules = results is IModule module ? new[] { module } : results as IEnumerable<IModule>;
            return modules != null ? await context.ExecuteAsync(modules, inputs) : (IEnumerable<IDocument>)null;
        }

        private static async Task<IEnumerable<IDocument>> ChangeContentAsync(object result, IExecutionContext context, IDocument document) =>
            new[] { document.Clone(await GetContentProviderAsync(result, context)) };

        private static async Task<IContentProvider> GetContentProviderAsync(object content, IExecutionContext context)
        {
            switch (content)
            {
                case null:
                    return null;
                case IContentProvider contentProvider:
                    return contentProvider;
                case IContentProviderFactory factory:
                    return context.GetContentProvider(factory);
                case Stream stream:
                    return context.GetContentProvider(stream);
                case string str:
                    return await context.GetContentProviderAsync(str);
            }
            return await context.GetContentProviderAsync(content.ToString());
        }
    }
}
