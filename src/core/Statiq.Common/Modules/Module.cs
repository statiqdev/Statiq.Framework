using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A common base class for modules that includes
    /// built-in support for parallelism (when also
    /// implementing <see cref="IParallelModule"/>)
    /// and per-document processing.
    /// </summary>
    public abstract class Module : IModule
    {
        private readonly bool _eachDocument;

        /// <summary>
        /// Indicates whether documents will be
        /// processed by this module in parallel.
        /// </summary>
        public bool Parallel { get; internal set; }

        /// <summary>
        /// Creates a module, indicating if the execute
        /// method should be called for each document
        /// or once overall.
        /// </summary>
        /// <param name="eachDocument">
        /// <c>true</c> to call <see cref="ExecuteAsync(IDocument, IExecutionContext)"/>
        /// for each input document, <c>false</c> to call it once overall with a <c>null</c>
        /// input argument. Note that setting this to <c>false</c> will also turn off
        /// any parallel processing (since there's nothing to execute in parallel given
        /// the <see cref="ExecuteAsync(IDocument, IExecutionContext)"/> is only called once).
        /// </param>
        protected Module(bool eachDocument)
        {
            _eachDocument = eachDocument;
            Parallel = this is IParallelModule;
        }

        /// <summary>
        /// Creates a module that will call the execute method
        /// for each input document.
        /// </summary>
        protected Module()
            : this(true)
        {
        }

        /// <inheritdoc />
        public Task<IEnumerable<IDocument>> ExecuteAsync(IExecutionContext context)
        {
            if (_eachDocument)
            {
                return Parallel
                    ? context.Inputs.ParallelSelectManyAsync(context, async input => await SafeExecuteAsync(input, context))
                    : context.Inputs.SelectManyAsync(context, async input => await SafeExecuteAsync(input, context));
            }
            return ExecuteAsync(null, context);
        }

        /// <summary>
        /// Takes care of exceptional cases like a <c>null</c> return task or <c>null</c> result sequence.
        /// Because the result is passed to a <c>SelectMany</c> style function, execute cannot return <c>null</c>.
        /// </summary>
        private async Task<IEnumerable<IDocument>> SafeExecuteAsync(IDocument input, IExecutionContext context) =>
            (await (ExecuteAsync(input, context) ?? Task.FromResult<IEnumerable<IDocument>>(Array.Empty<IDocument>()))) ?? Array.Empty<IDocument>();

        /// <summary>
        /// Executes the module.
        /// </summary>
        /// <remarks>
        /// This method will be called for each document if specified in the constructor,
        /// otherwise it will be called once with a <c>null</c> <paramref name="input"/>.
        /// </remarks>
        /// <param name="input">
        /// The input document this module is currently processing or <c>null</c> if
        /// the module does not process each document.
        /// </param>
        /// <param name="context">The execution context.</param>
        /// <returns>The result documents.</returns>
        protected abstract Task<IEnumerable<IDocument>> ExecuteAsync(IDocument input, IExecutionContext context);
    }
}
