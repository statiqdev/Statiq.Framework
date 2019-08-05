using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A common base class for modules.
    /// </summary>
    /// <remarks>
    /// Documents can either be processed one at a time by overriding
    /// <see cref="ExecuteAsync(IDocument, IExecutionContext)"/> or all
    /// at once by overriding <see cref="ExecuteAsync(IExecutionContext)"/>.
    /// </remarks>
    public abstract class Module : IModule
    {
        /// <summary>
        /// Executes the module once for all input documents.
        /// </summary>
        /// <remarks>
        /// Override this method to execute the module once for all input documents. The default behavior
        /// calls <see cref="ExecuteAsync(IDocument, IExecutionContext)"/> for each input document (possibly
        /// in parallel) and overriding this method will result in <see cref="ExecuteAsync(IDocument, IExecutionContext)"/>
        /// not being called.
        /// </remarks>
        /// <param name="context">The execution context.</param>
        /// <returns>The result documents.</returns>
        public virtual Task<IEnumerable<IDocument>> ExecuteAsync(IExecutionContext context) =>
            context.QueryInputs().SelectManyAsync(async input => await SafeExecuteAsync(input, context)).Task;

        /// <summary>
        /// Takes care of exceptional cases like a <c>null</c> return task or <c>null</c> result sequence.
        /// Because the result is passed to a <c>SelectMany</c> style function, execute cannot return <c>null</c>.
        /// </summary>
        protected async Task<IEnumerable<IDocument>> SafeExecuteAsync(IDocument input, IExecutionContext context) =>
            (await (ExecuteAsync(input, context) ?? Task.FromResult<IEnumerable<IDocument>>(Array.Empty<IDocument>()))) ?? Array.Empty<IDocument>();

        /// <summary>
        /// Executes the module.
        /// </summary>
        /// <remarks>
        /// This method will be called for each document unless <see cref="ExecuteAsync(IExecutionContext)"/>
        /// is overridden.
        /// </remarks>
        /// <param name="input">
        /// The input document this module is currently processing..
        /// </param>
        /// <param name="context">The execution context.</param>
        /// <returns>The result documents.</returns>
        protected virtual Task<IEnumerable<IDocument>> ExecuteAsync(IDocument input, IExecutionContext context) =>
            Task.FromResult<IEnumerable<IDocument>>(Array.Empty<IDocument>());
    }
}
