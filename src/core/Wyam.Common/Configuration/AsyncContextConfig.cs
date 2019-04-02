using System.Threading.Tasks;
using Wyam.Common.Execution;

namespace Wyam.Common.Configuration
{
    /// <summary>
    /// A delegate that uses the execution context.
    /// </summary>
    /// <param name="ctx">The execution context.</param>
    /// <returns>A result object.</returns>
    public delegate Task<object> AsyncContextConfig(IExecutionContext ctx);
}
