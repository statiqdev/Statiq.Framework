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
        /// calls <see cref="ExecuteAsync(IDocument, IExecutionContext)"/> for each input document
        /// and overriding this method will result in <see cref="ExecuteAsync(IDocument, IExecutionContext)"/>
        /// not being called.
        /// </remarks>
        /// <param name="context">The execution context.</param>
        /// <returns>The result documents.</returns>
        public virtual async Task<IEnumerable<IDocument>> ExecuteAsync(IExecutionContext context)
        {
            IEnumerable<IDocument> aggregateResults = null;
            foreach (IDocument input in context.Inputs)
            {
                IEnumerable<IDocument> results = await ExecuteInput(input, context, ExecuteAsync);
                if (results != null)
                {
                    aggregateResults = aggregateResults?.Concat(results) ?? results;
                }
            }
            return aggregateResults;
        }

        internal static T ExecuteInput<T>(
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

        /// <summary>
        /// Executes the module.
        /// </summary>
        /// <remarks>
        /// This method will be called for each document unless <see cref="ExecuteAsync(IExecutionContext)"/>
        /// is overridden.
        /// </remarks>
        /// <param name="input">
        /// The input document this module is currently processing.
        /// </param>
        /// <param name="context">The execution context.</param>
        /// <returns>The result documents.</returns>
        protected virtual Task<IEnumerable<IDocument>> ExecuteAsync(IDocument input, IExecutionContext context) =>
            Task.FromResult<IEnumerable<IDocument>>(null);
    }
}
