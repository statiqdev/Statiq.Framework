using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common.Documents;
using Statiq.Common.Execution;

namespace Statiq.Common.Modules
{
    /// <summary>
    /// The primary module interface for classes that can transform or otherwise operate on documents.
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// This should not be called directly, instead call <c>IExecutionContext.Execute()</c> if you need to execute a module from within another module.
        /// </summary>
        /// <param name="inputs">The input documents to this module.</param>
        /// <param name="context">The execution context that can be used to access information about the environment and engine services.</param>
        /// <returns>A set of result documents (possibly the same as the input documents).</returns>
        Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context);
    }
}
