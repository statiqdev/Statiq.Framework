using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Statiq.Common.Documents;
using Statiq.Common.Modules;

namespace Statiq.Common.Execution
{
    public static class IExecutionContextExtensions
    {
        /// <summary>
        /// Executes the specified modules with an empty initial input document with optional additional metadata and returns the result documents.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="modules">The modules to execute.</param>
        /// <param name="metadata">The metadata to use.</param>
        /// <returns>The result documents from the executed modules.</returns>
        public static async Task<ImmutableArray<IDocument>> ExecuteAsync(
            this IExecutionContext context,
            IEnumerable<IModule> modules,
            IEnumerable<KeyValuePair<string, object>> metadata) =>
            await context.ExecuteAsync(modules, new[] { context.CreateDocument(metadata) });

        /// <summary>
        /// Executes the specified modules without an initial input document and returns the result documents.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="modules">The modules to execute.</param>
        /// <returns>The result documents from the executed modules.</returns>
        public static async Task<ImmutableArray<IDocument>> ExecuteAsync(
            this IExecutionContext context,
            IEnumerable<IModule> modules) =>
            await context.ExecuteAsync(modules, (IEnumerable<IDocument>)null);
    }
}
