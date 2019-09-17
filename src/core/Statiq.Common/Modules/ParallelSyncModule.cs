using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A module that processes documents in parallel (with the option to process sequentially).
    /// </summary>
    public abstract class ParallelSyncModule : Module, IParallelModule
    {
        /// <inheritdoc />
        public bool Parallel { get; set; } = true;

        /// <inheritdoc />
        protected sealed override Task<IDisposable> BeforeExecutionAsync(IExecutionContext context) =>
            Task.FromResult(BeforeExecution(context));

        /// <summary>
        /// Called before the current module execution cycle and is typically used for configuring module state.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <returns>A disposable that is guaranteed to be disposed when the module finishes the current execution cycle (or <c>null</c>).</returns>
        protected virtual IDisposable BeforeExecution(IExecutionContext context) => null;

        /// <inheritdoc />
        protected sealed override Task<IEnumerable<IDocument>> AfterExecutionAsync(IExecutionContext context, IEnumerable<IDocument> results) =>
            Task.FromResult(AfterExecution(context, results));

        /// <summary>
        /// Called after the current module execution cycle and is typically used for cleaning up module state
        /// or transforming the execution results.
        /// </summary>
        /// <remarks>
        /// If an exception is thrown during module execution, this method is never called. Return an <see cref="IDisposable"/>
        /// from <see cref="BeforeExecution(IExecutionContext)"/> if resources should be disposed even if an exception is thrown.
        /// </remarks>
        /// <param name="context">The execution context.</param>
        /// <param name="results">The results of module execution.</param>
        /// <returns>The final module results.</returns>
        protected virtual IEnumerable<IDocument> AfterExecution(IExecutionContext context, IEnumerable<IDocument> results) => results;

        /// <inheritdoc />
        protected sealed override Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context) =>
            Task.FromResult(ExecuteInput(context));

        /// <inheritdoc />
        // Unused, prevent overriding in derived classes
        protected sealed override Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context) =>
            throw new NotSupportedException();

        private IEnumerable<IDocument> ExecuteInput(IExecutionContext context)
        {
            if (Parallel)
            {
                return context.Inputs
                    .AsParallel()
                    .AsOrdered()
                    .WithCancellation(context.CancellationToken)
                    .Select(input => ExecuteInputFunc(input, context, ExecuteInput))
                    .Where(x => x != null)
                    .SelectMany(x => x);
            }

            return context.Inputs
                .Select(input => ExecuteInputFunc(input, context, ExecuteInput))
                .Where(x => x != null)
                .SelectMany(x => x);
        }

        /// <summary>
        /// Executes the module.
        /// </summary>
        /// <remarks>
        /// This method will be called for each document unless <see cref="ExecuteInput(IExecutionContext)"/>
        /// is overridden.
        /// </remarks>
        /// <param name="input">
        /// The input document this module is currently processing.
        /// </param>
        /// <param name="context">The execution context.</param>
        /// <returns>The result documents.</returns>
        protected virtual IEnumerable<IDocument> ExecuteInput(IDocument input, IExecutionContext context) => null;
    }
}
