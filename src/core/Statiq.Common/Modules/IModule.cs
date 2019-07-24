using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// The primary module interface for classes that can transform or otherwise operate on documents.
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// This should not be called directly, instead call <c>IExecutionContext.Execute()</c> if you need to execute a module from within another module.
        /// </summary>
        /// <param name="context">The execution context that includes input documents, information about the environment, and engine services.</param>
        /// <returns>A set of result documents (possibly the same as the input documents).</returns>
        Task<IEnumerable<IDocument>> ExecuteAsync(IExecutionContext context);
    }
}
