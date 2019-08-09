using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A common base class for synchronous modules.
    /// </summary>
    /// <remarks>
    /// Documents can either be processed one at a time by overriding
    /// <see cref="Execute(IDocument, IExecutionContext)"/> or all
    /// at once by overriding <see cref="Execute(IExecutionContext)"/>.
    /// </remarks>
    public abstract class SyncModule : Module
    {
        /// <inheritdoc />
        public override sealed IAsyncEnumerable<IDocument> ExecuteAsync(IExecutionContext context) =>
            Execute(context).ToAsyncEnumerable();

        /// <inheritdoc />
        // Unused, prevent overriding in derived classes
        protected sealed override IAsyncEnumerable<IDocument> ExecuteAsync(IDocument input, IExecutionContext context) =>
            base.ExecuteAsync(input, context);

        /// <summary>
        /// Executes the module once for all input documents.
        /// </summary>
        /// <remarks>
        /// Override this method to execute the module once for all input documents. The default behavior
        /// calls <see cref="Execute(IDocument, IExecutionContext)"/> for each input document
        /// and overriding this method will result in <see cref="Execute(IDocument, IExecutionContext)"/>
        /// not being called.
        /// </remarks>
        /// <param name="context">The execution context.</param>
        /// <returns>The result documents.</returns>
        protected virtual IEnumerable<IDocument> Execute(IExecutionContext context) =>
            context.Inputs
                .ToEnumerable()
                .Select(input => ExecuteInput(input, context, Execute))
                .Where(x => x != null)
                .SelectMany(x => x);

        /// <summary>
        /// Executes the module.
        /// </summary>
        /// <remarks>
        /// This method will be called for each document unless <see cref="Execute(IExecutionContext)"/>
        /// is overridden.
        /// </remarks>
        /// <param name="input">
        /// The input document this module is currently processing.
        /// </param>
        /// <param name="context">The execution context.</param>
        /// <returns>The result documents.</returns>
        protected virtual IEnumerable<IDocument> Execute(IDocument input, IExecutionContext context) => null;
    }
}
