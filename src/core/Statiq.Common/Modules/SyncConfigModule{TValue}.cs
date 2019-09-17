using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// Base class for modules that rely on a config value and could apply to input documents (or not) depending
    /// on whether the config delegate requires them.
    /// </summary>
    public abstract class SyncConfigModule<TValue> : ConfigModule<TValue>
    {
        /// <summary>
        /// Creates a new config module.
        /// </summary>
        /// <param name="config">The delegate to use for getting a config value.</param>
        /// <param name="eachDocument">
        /// <c>true</c> to call <see cref="ExecuteConfigAsync(IDocument, IExecutionContext, TValue)"/> for each
        /// input document regardless of whether the config delegate requires a document or <c>false</c>
        /// to allow only calling <see cref="ExecuteConfigAsync(IDocument, IExecutionContext, TValue)"/> once
        /// with a null input document if the config delegate does not require a document.
        /// </param>
        protected SyncConfigModule(Config<TValue> config, bool eachDocument)
            : base(config, eachDocument)
        {
        }

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
        protected sealed override Task<IEnumerable<IDocument>> ExecuteConfigAsync(IDocument input, IExecutionContext context, TValue value) =>
            Task.FromResult(ExecuteConfig(input, context, value));

        /// <summary>
        /// Executes the module for each input document in parallel.
        /// If there aren't any input documents and the config delegate doesn't require documents,
        /// this will be called once with a null <paramref name="input"/>.
        /// </summary>
        /// <param name="input">
        /// The input document this module is currently applying to or <c>null</c> if there aren't any
        /// input documents or if the config delegate doesn't require documents.
        /// </param>
        /// <param name="context">The execution context.</param>
        /// <param name="value">The evaluated config value.</param>
        /// <returns>The result documents.</returns>
        protected abstract IEnumerable<IDocument> ExecuteConfig(IDocument input, IExecutionContext context, TValue value);
    }
}