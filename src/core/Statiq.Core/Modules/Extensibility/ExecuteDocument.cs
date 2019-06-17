using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common.Configuration;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.Modules;
using Statiq.Core.Modules.Contents;

namespace Statiq.Core.Modules.Extensibility
{
    /// <summary>
    /// Executes custom code on individual documents.
    /// </summary>
    /// <remarks>
    /// This module is very useful for customizing pipeline execution without having to write an entire module.
    /// Returning modules from the delegate is also useful for customizing existing modules based on the
    /// current set of documents. For example, you can use this module to execute the <see cref="Replace"/> module
    /// with customized search strings based on the results of other pipelines.
    /// </remarks>
    /// <category>Extensibility</category>
    public class ExecuteDocument : Execute
    {
        /// <summary>
        /// Specifies a delegate that should be invoked once for each input document. If the delegate
        /// returns a <c>IEnumerable&lt;IDocument&gt;</c> or <see cref="IDocument"/>, the document(s) will be the
        /// output(s) of this module. If the delegate returns a <c>IEnumerable&lt;IModule&gt;</c> or
        /// <see cref="IModule"/>, the module(s) will be executed with each input document as their input
        /// and the results will be the output of this module. If the delegate returns null,
        /// this module will just output the input document. If anything else is returned, the input
        /// document will be output with the string value of the delegate result as it's content.
        /// </summary>
        /// <param name="execute">A delegate to invoke that should return a <c>IEnumerable&lt;IDocument&gt;</c>,
        /// <see cref="IDocument"/>, <c>IEnumerable&lt;IModule&gt;</c>, <see cref="IModule"/>, object, or null.</param>
        /// <param name="parallel">The delegate is usually evaluated and each input document is processed in parallel.
        /// Setting this to <c>false</c> runs evaluates and processes each document in their original input order.</param>
        public ExecuteDocument(DocumentConfig<object> execute, bool parallel = true)
            : base(async (inputs, context) =>
            {
                return parallel
                    ? await inputs.ParallelSelectManyAsync(context, GetResults)
                    : await inputs.SelectManyAsync(context, GetResults);

                async Task<IEnumerable<IDocument>> GetResults(IDocument input)
                {
                    object documentResult = await execute.GetValueAsync(input, context);
                    if (documentResult == null)
                    {
                        return new[] { input };
                    }
                    return GetDocuments(documentResult)
                        ?? await ExecuteModulesAsync(documentResult, context, new[] { input })
                        ?? await ChangeContentAsync(documentResult, context, input);
                }
            })
        {
        }

        /// <summary>
        /// Specifies a delegate that should be invoked once for each input document.
        /// The output from this module will be the input documents.
        /// </summary>
        /// <param name="execute">An action to execute on each input document.</param>
        /// <param name="parallel">The delegate is usually evaluated and each input document is processed in parallel.
        /// Setting this to <c>false</c> runs evaluates and processes each document in their original input order.</param>
        public ExecuteDocument(Func<IDocument, IExecutionContext, Task> execute, bool parallel = true)
            : this(
                Config.FromDocument(
                    async (doc, ctx) =>
                    {
                        await (execute(doc, ctx) ?? Task.CompletedTask);
                        return (object)null;
                    }),
                parallel)
        {
        }

        /// <summary>
        /// Specifies a delegate that should be invoked once for each input document.
        /// The output from this module will be the input documents.
        /// </summary>
        /// <param name="execute">An action to execute on each input document.</param>
        /// <param name="parallel">The delegate is usually evaluated and each input document is processed in parallel.
        /// Setting this to <c>false</c> runs evaluates and processes each document in their original input order.</param>
        public ExecuteDocument(Action<IDocument, IExecutionContext> execute, bool parallel = true)
            : this(
                Config.FromDocument(
                    (doc, ctx) =>
                    {
                        execute(doc, ctx);
                        return (object)null;
                    }),
                parallel)
        {
        }
    }
}