using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// An abstract base for modules that execute children and then combine the results with the
    /// input documents in some way.
    /// </summary>
    /// <remarks>
    /// The child modules are executed once and the original input documents
    /// are passed to the child modules. Wrap the child modules with a <c>ForEachDocument</c>
    /// module to execute the child modules for each input document individually. Add a
    /// <c>ClearDocuments</c> module as the first child if the original input documents
    /// should not be passed to the child modules.
    /// </remarks>
    public abstract class SyncChildDocumentsModule : ChildDocumentsModule
    {
        /// <summary>
        /// Executes the specified modules to get result documents.
        /// </summary>
        /// <param name="modules">The modules to execute.</param>
        protected SyncChildDocumentsModule(params IModule[] modules)
            : base(modules)
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
        protected sealed override Task<IEnumerable<IDocument>> ExecuteChildrenAsync(IExecutionContext context, ImmutableArray<IDocument> childOutputs) =>
            Task.FromResult(ExecuteChildren(context, childOutputs));

        /// <summary>
        /// Gets the output documents given the input documents and the output documents from the execution of child modules.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="childOutputs">The output documents from the child modules.</param>
        /// <returns>The output documents of this module.</returns>
        protected abstract IEnumerable<IDocument> ExecuteChildren(IExecutionContext context, ImmutableArray<IDocument> childOutputs);
    }
}
