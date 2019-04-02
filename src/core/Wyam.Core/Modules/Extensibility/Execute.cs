using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Common.Util;
using Wyam.Core.Modules.Contents;

namespace Wyam.Core.Modules.Extensibility
{
    /// <summary>
    /// Executes custom code that returns documents, modules, or new content.
    /// </summary>
    /// <remarks>
    /// This module is very useful for customizing pipeline execution without having to write an entire module.
    /// Returning modules from the delegate is also useful for customizing existing modules based on the
    /// current set of documents. For example, you can use this module to execute the <see cref="Replace"/> module
    /// with customized search strings based on the results of other pipelines.
    /// </remarks>
    /// <category>Extensibility</category>
    public class Execute : IModule
    {
        private readonly Func<IReadOnlyList<IDocument>, IExecutionContext, Task<IEnumerable<IDocument>>> _execute;

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
        public Execute(AsyncDocumentConfig execute, bool parallel = true)
        {
            _execute = async (inputs, context) =>
            {
                Func<IDocument, Task<IEnumerable<IDocument>>> selectMany = async input =>
                {
                    object documentResult = execute(input, context);
                    if (documentResult == null)
                    {
                        return new[] { input };
                    }
                    return GetDocuments(documentResult)
                        ?? await ExecuteModulesAsync(documentResult, context, new[] { input })
                        ?? ChangeContent(documentResult, context, input);
                };
                return parallel
                    ? await inputs.ParallelSelectManyAsync(context, selectMany)
                    : await inputs.SelectManyAsync(context, selectMany);
            };
        }

        /// <summary>
        /// Specifies a delegate that should be invoked once for each input document.
        /// The output from this module will be the input documents.
        /// </summary>
        /// <param name="execute">An action to execute on each input document.</param>
        /// <param name="parallel">The delegate is usually evaluated and each input document is processed in parallel.
        /// Setting this to <c>false</c> runs evaluates and processes each document in their original input order.</param>
        public Execute(Func<IDocument, IExecutionContext, Task> execute, bool parallel = true)
            : this(
                async (doc, ctx) =>
                {
                    await execute(doc, ctx);
                    return null;
                },
                parallel)
        {
        }

        /// <summary>
        /// Specifies a delegate that should be invoked once for all input documents. If the delegate
        /// returns a <c>IEnumerable&lt;IDocument&gt;</c> or <see cref="IDocument"/>, the document(s) will be the
        /// output(s) of this module. If the delegate returns a <c>IEnumerable&lt;IModule&gt;</c> or
        /// <see cref="IModule"/>, the module(s) will be executed with the input documents as their input
        /// and the results will be the output of this module. If the delegate returns null,
        /// this module will just output the input documents. If anything else is returned, an exception will be thrown.
        /// </summary>
        /// <param name="execute">A delegate to invoke that should return a <c>IEnumerable&lt;IDocument&gt;</c>,
        /// <see cref="IDocument"/>, <c>IEnumerable&lt;IModule&gt;</c>, <see cref="IModule"/>, or null.</param>
        public Execute(AsyncContextConfig execute)
        {
            _execute = async (inputs, context) =>
            {
                object contextResult = await execute(context);
                if (contextResult == null)
                {
                    return inputs;
                }
                return GetDocuments(contextResult)
                    ?? await ExecuteModulesAsync(contextResult, context, inputs)
                    ?? ThrowInvalidDelegateResult(contextResult);
            };
        }

        /// <summary>
        /// Specifies a delegate that should be invoked once for all input documents.
        /// The output from this module will be the input documents.
        /// </summary>
        /// <param name="execute">An action to execute.</param>
        public Execute(Func<IExecutionContext, Task> execute)
            : this(
                async ctx =>
                {
                    await execute(ctx);
                    return null;
                })
        {
        }

        /// <summary>
        /// Specifies a delegate that should be invoked for all input documents. If the delegate
        /// returns a <c>IEnumerable&lt;IDocument&gt;</c> or <see cref="IDocument"/>, the document(s) will be the
        /// output(s) of this module. If the delegate returns null or anything else, this module will just output the input documents.
        /// </summary>
        /// <param name="execute">A delegate to invoke that should return a <c>IEnumerable&lt;IDocument&gt;</c>, <see cref="IDocument"/>, or null.</param>
        public Execute(Func<IReadOnlyList<IDocument>, IExecutionContext, Task<object>> execute)
        {
            _execute = async (inputs, context) => GetDocuments(await execute(inputs, context)) ?? inputs;
        }

        /// <inheritdoc />
        Task<IEnumerable<IDocument>> IModule.ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context) => _execute(inputs, context);

        private IEnumerable<IDocument> GetDocuments(object result)
        {
            IEnumerable<IDocument> documents = result as IEnumerable<IDocument>;
            if (documents == null && result is IDocument document)
            {
                documents = new[] { document };
            }
            return documents;
        }

        private async Task<IEnumerable<IDocument>> ExecuteModulesAsync(object results, IExecutionContext context, IEnumerable<IDocument> inputs)
        {
            // Check for a single IModule first since some modules also implement IEnumerable<IModule>
            IEnumerable<IModule> modules;
            IModule module = results as IModule;
            if (module != null)
            {
                modules = new[] { module };
            }
            else
            {
                modules = results as IEnumerable<IModule>;
            }
            return modules != null ? await context.ExecuteAsync(modules, inputs) : null;
        }

        private IEnumerable<IDocument> ChangeContent(object result, IExecutionContext context, IDocument document) =>
            new[] { context.GetDocumentAsync(document, result.ToString()).Result };

        private IEnumerable<IDocument> ThrowInvalidDelegateResult(object result)
        {
            throw new Exception($"Execute delegate must return IEnumerable<IDocument>, IDocument, IEnumerable<IModule>, IModule, or null; {result.GetType().Name} is an invalid return type");
        }
    }
}