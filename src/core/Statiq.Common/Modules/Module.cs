using System;
using System.Collections.Generic;
using System.Linq;
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
        /// calls <see cref="ExecuteAsync(IDocument, IExecutionContext)"/> for each input document
        /// and overriding this method will result in <see cref="ExecuteAsync(IDocument, IExecutionContext)"/>
        /// not being called.
        /// </remarks>
        /// <param name="context">The execution context.</param>
        /// <returns>The result documents.</returns>
        public virtual async IAsyncEnumerable<IDocument> ExecuteAsync(IExecutionContext context)
        {
            await foreach (IDocument input in context.Inputs)
            {
                IAsyncEnumerable<IDocument> results = ExecuteInput(input, context, ExecuteAsync);
                if (results != null)
                {
                    await foreach (IDocument result in results)
                    {
                        yield return result;
                    }
                }
            }
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
                Trace.ProcessingException(input, ex);
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
        protected virtual IAsyncEnumerable<IDocument> ExecuteAsync(IDocument input, IExecutionContext context) => null;
    }
}
