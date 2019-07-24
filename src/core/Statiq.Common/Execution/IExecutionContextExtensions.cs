using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static class IExecutionContextExtensions
    {
        /// <summary>
        /// Executes the specified modules with an single initial input document with optional additional metadata and returns the result documents.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="modules">The modules to execute.</param>
        /// <param name="metadata">The metadata to use.</param>
        /// <returns>The result documents from the executed modules.</returns>
        public static async Task<IReadOnlyList<IDocument>> ExecuteAsync(
            this IExecutionContext context,
            IEnumerable<IModule> modules,
            IEnumerable<KeyValuePair<string, object>> metadata) =>
            await context.ExecuteAsync(modules, context.CreateDocument(metadata).Yield());

        /// <summary>
        /// Executes the specified modules without an initial input document and returns the result documents.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="modules">The modules to execute.</param>
        /// <returns>The result documents from the executed modules.</returns>
        public static async Task<IReadOnlyList<IDocument>> ExecuteAsync(
            this IExecutionContext context,
            IEnumerable<IModule> modules) =>
            await context.ExecuteAsync(modules, null);
    }
}
