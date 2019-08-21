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
        public sealed override Task<IEnumerable<IDocument>> ExecuteAsync(IExecutionContext context) =>
            Task.FromResult(Execute(context));

        /// <inheritdoc />
        // Unused, prevent overriding in derived classes
        protected sealed override Task<IEnumerable<IDocument>> ExecuteAsync(IDocument input, IExecutionContext context) =>
            base.ExecuteAsync(input, context);

        private IEnumerable<IDocument> Execute(IExecutionContext context)
        {
            if (Parallel)
            {
                return context.Inputs
                    .AsParallel()
                    .AsOrdered()
                    .WithCancellation(context.CancellationToken)
                    .Select(input => ExecuteInput(input, context, Execute))
                    .Where(x => x != null)
                    .SelectMany(x => x);
            }

            return context.Inputs
                .Select(input => ExecuteInput(input, context, Execute))
                .Where(x => x != null)
                .SelectMany(x => x);
        }

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
