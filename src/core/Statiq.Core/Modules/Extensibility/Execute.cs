using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Executes custom code on all input documents.
    /// </summary>
    /// <remarks>
    /// This module is very useful for customizing pipeline execution without having to write an entire module.
    /// Returning modules from the delegate is also useful for customizing existing modules based on the
    /// current set of documents. For example, you can use this module to execute the <see cref="ReplaceInContent"/> module
    /// with customized search strings based on the results of other pipelines.
    /// </remarks>
    /// <category>Extensibility</category>
    public class Execute : IModule
    {
        private readonly Func<IReadOnlyList<IDocument>, IExecutionContext, Task<IEnumerable<IDocument>>> _execute;

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

        /// <summary>
        /// Specifies a delegate that should be invoked for all input documents. If the delegate
        /// returns a <c>IEnumerable&lt;IDocument&gt;</c> or <see cref="IDocument"/>, the document(s) will be the
        /// output(s) of this module. If the delegate returns null or anything else, this module will just output the input documents.
        /// </summary>
        /// <param name="execute">A delegate to invoke.</param>
        public Execute(Func<IReadOnlyList<IDocument>, IExecutionContext, Task> execute)
        {
            _execute = async (inputs, context) =>
            {
                await (execute(inputs, context) ?? Task.CompletedTask);
                return inputs;
            };
        }

        /// <summary>
        /// Specifies a delegate that should be invoked for all input documents. If the delegate
        /// returns a <c>IEnumerable&lt;IDocument&gt;</c> or <see cref="IDocument"/>, the document(s) will be the
        /// output(s) of this module. If the delegate returns null or anything else, this module will just output the input documents.
        /// </summary>
        /// <param name="execute">A delegate to invoke.</param>
        public Execute(Action<IReadOnlyList<IDocument>, IExecutionContext> execute)
        {
            _execute = (inputs, context) =>
            {
                execute(inputs, context);
                return Task.FromResult<IEnumerable<IDocument>>(inputs);
            };
        }

        /// <inheritdoc />
        public Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context) => _execute(inputs, context);

        protected static IEnumerable<IDocument> GetDocuments(object result) =>
            result is IDocument document ? document.Yield() : result as IEnumerable<IDocument>;

        protected static async Task<IEnumerable<IDocument>> ExecuteModulesAsync(object results, IExecutionContext context, IEnumerable<IDocument> inputs)
        {
            // Check for a single IModule first since some modules also implement IEnumerable<IModule>
            IEnumerable<IModule> modules = results is IModule module ? new[] { module } : results as IEnumerable<IModule>;
            return modules != null ? await context.ExecuteAsync(modules, inputs) : (IEnumerable<IDocument>)null;
        }

        protected static async Task<IEnumerable<IDocument>> ChangeContentAsync(object result, IExecutionContext context, IDocument document) =>
            document.Clone(await GetContentProviderAsync(result, context)).Yield();

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

        protected static IEnumerable<IDocument> ThrowInvalidDelegateResult(object result)
        {
            throw new Exception($"Execute delegate must return IEnumerable<IDocument>, IDocument, IEnumerable<IModule>, IModule, or null; {result.GetType().Name} is an invalid return type");
        }
    }
}