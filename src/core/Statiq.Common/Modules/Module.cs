using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    /// <summary>
    /// A common base class for modules.
    /// </summary>
    /// <remarks>
    /// Documents can either be processed one at a time by overriding
    /// <see cref="ExecuteInputAsync(IDocument, IExecutionContext)"/> or all
    /// at once by overriding <see cref="ExecuteContextAsync(IExecutionContext)"/>.
    /// </remarks>
    public abstract class Module : IModule
    {
        /// <inheritdoc />
        public async Task<IEnumerable<IDocument>> ExecuteAsync(IExecutionContext context)
        {
            IDisposable disposable = await BeforeExecutionAsync(context);
            try
            {
                return await AfterExecutionAsync(context, await ExecuteContextAsync(context));
            }
            finally
            {
                disposable?.Dispose();
            }
        }

        /// <summary>
        /// Called before the current module execution cycle and is typically used for configuring module state.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <returns>A disposable that is guaranteed to be disposed when the module finishes the current execution cycle (or <c>null</c>).</returns>
        protected virtual Task<IDisposable> BeforeExecutionAsync(IExecutionContext context) => Task.FromResult((IDisposable)null);

        /// <summary>
        /// Called after the current module execution cycle and is typically used for cleaning up module state
        /// or transforming the execution results.
        /// </summary>
        /// <remarks>
        /// If an exception is thrown during module execution, this method is never called. Return an <see cref="IDisposable"/>
        /// from <see cref="BeforeExecutionAsync(IExecutionContext)"/> if resources should be disposed even if an exception is thrown.
        /// </remarks>
        /// <param name="context">The execution context.</param>
        /// <param name="results">The results of module execution.</param>
        /// <returns>The final module results.</returns>
        protected virtual Task<IEnumerable<IDocument>> AfterExecutionAsync(IExecutionContext context, IEnumerable<IDocument> results) => Task.FromResult(results);

        /// <summary>
        /// Executes the module once for all input documents.
        /// </summary>
        /// <remarks>
        /// Override this method to execute the module once for all input documents. The default behavior
        /// calls <see cref="ExecuteInputAsync(IDocument, IExecutionContext)"/> for each input document
        /// and overriding this method will result in <see cref="ExecuteInputAsync(IDocument, IExecutionContext)"/>
        /// not being called.
        /// </remarks>
        /// <param name="context">The execution context.</param>
        /// <returns>The result documents.</returns>
        protected virtual async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            IEnumerable<IDocument> aggregateResults = null;
            foreach (IDocument input in context.Inputs)
            {
                IEnumerable<IDocument> results = await ExecuteInputFunc(input, context, ExecuteInputAsync);
                if (results != null)
                {
                    aggregateResults = aggregateResults?.Concat(results) ?? results;
                }
            }
            return aggregateResults;
        }

        /// <summary>
        /// Executes the module.
        /// </summary>
        /// <remarks>
        /// This method will be called for each document unless <see cref="ExecuteContextAsync(IExecutionContext)"/>
        /// is overridden.
        /// </remarks>
        /// <param name="input">
        /// The input document this module is currently processing.
        /// </param>
        /// <param name="context">The execution context.</param>
        /// <returns>The result documents.</returns>
        protected virtual Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context) =>
            Task.FromResult<IEnumerable<IDocument>>(null);

        /// <summary>
        /// Used by module base classes to execute an input while checking
        /// for cancellation and logging exceptions.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="input">The input document.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="executeFunc">The per-document execution function.</param>
        /// <returns>The results of the execution function.</returns>
        internal static T ExecuteInputFunc<T>(
            IDocument input,
            IExecutionContext context,
            Func<IDocument, IExecutionContext, T> executeFunc)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            try
            {
                return executeFunc(input, context);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                string displayString = input is IDisplayable displayable ? $" [{displayable.ToSafeDisplayString()}]" : string.Empty;
                context.LogError($"Exception while processing {input.GetType().Name}{displayString}: {ex.Message}");
                throw;
            }
        }
    }
}
