using System;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Executes custom code on the execution context.
    /// </summary>
    /// <remarks>
    /// This module is very useful for customizing pipeline execution without having to write an entire module.
    /// Returning modules from the delegate is also useful for customizing existing modules based on the
    /// current set of documents. For example, you can use this module to execute the <see cref="ReplaceInContent"/> module
    /// with customized search strings based on the results of other pipelines.
    /// </remarks>
    /// <category>Extensibility</category>
    public class ExecuteContext : Execute
    {
        /// <summary>
        /// Specifies a delegate that should be invoked once for all input documents. If the delegate
        /// returns a <c>IEnumerable&lt;IDocument&gt;</c> or <see cref="IDocument"/>, the document(s) will be the
        /// output(s) of this module. If the delegate returns a <c>IEnumerable&lt;IModule&gt;</c> or
        /// <see cref="IModule"/>, the module(s) will be executed with the input documents as their input
        /// and the results will be the output of this module. If the delegate returns null,
        /// this module will just output the input documents. If anything else is returned, an exception will be thrown.
        /// </summary>
        /// <param name="execute">A delegate to invoke that should return a <c>IEnumerable&lt;IDocument&gt;</c>,
        /// <see cref="IDocument"/>, <c>IEnumerable&lt;IModule&gt;</c>, <see cref="IModule"/>, or null.</param>
        public ExecuteContext(Func<IExecutionContext, Task<object>> execute)
            : base(async (inputs, context) =>
            {
                if (execute == null)
                {
                    return inputs;
                }

                object contextResult = await execute(context);
                if (contextResult == null)
                {
                    return inputs;
                }

                return GetDocuments(contextResult)
                    ?? await ExecuteModulesAsync(contextResult, context, inputs)
                    ?? ThrowInvalidDelegateResult(contextResult);
            })
        {
        }

        /// <summary>
        /// Specifies a delegate that should be invoked once for all input documents. If the delegate
        /// returns a <c>IEnumerable&lt;IDocument&gt;</c> or <see cref="IDocument"/>, the document(s) will be the
        /// output(s) of this module. If the delegate returns a <c>IEnumerable&lt;IModule&gt;</c> or
        /// <see cref="IModule"/>, the module(s) will be executed with the input documents as their input
        /// and the results will be the output of this module. If the delegate returns null,
        /// this module will just output the input documents. If anything else is returned, an exception will be thrown.
        /// </summary>
        /// <param name="execute">A delegate to invoke that should return a <c>IEnumerable&lt;IDocument&gt;</c>,
        /// <see cref="IDocument"/>, <c>IEnumerable&lt;IModule&gt;</c>, <see cref="IModule"/>, or null.</param>
        public ExecuteContext(Func<IExecutionContext, object> execute)
            : this(ctx => Task.FromResult(execute?.Invoke(ctx)))
        {
        }

        /// <summary>
        /// Specifies a delegate that should be invoked once for all input documents.
        /// The output from this module will be the input documents.
        /// </summary>
        /// <param name="execute">An action to execute.</param>
        public ExecuteContext(Func<IExecutionContext, Task> execute)
            : this(
                async ctx =>
                {
                    await (execute?.Invoke(ctx) ?? Task.CompletedTask);
                    return null;
                })
        {
        }

        /// <summary>
        /// Specifies a delegate that should be invoked once for all input documents.
        /// The output from this module will be the input documents.
        /// </summary>
        /// <param name="execute">An action to execute.</param>
        public ExecuteContext(Action<IExecutionContext> execute)
            : this(
                ctx =>
                {
                    execute?.Invoke(ctx);
                    return (object)null;
                })
        {
        }
    }
}