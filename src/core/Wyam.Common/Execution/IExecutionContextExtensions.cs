using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Modules;

namespace Wyam.Common.Execution
{
    public static class IExecutionContextExtensions
    {
        /// <summary>
        /// Provides access to the same enhanced type conversion used to convert metadata types.
        /// This method never throws an exception. It will return default(T) if the value cannot be converted to T.
        /// </summary>
        /// <typeparam name="T">The desired return type.</typeparam>
        /// <param name="context">The execution context.</param>
        /// <param name="value">The value to convert.</param>
        /// <returns>The value converted to type T or default(T) if the value cannot be converted to type T.</returns>
        public static T Convert<T>(this IExecutionContext context, object value) => Convert<T>(context, value, null);

        /// <summary>
        /// Provides access to the same enhanced type conversion used to convert metadata types.
        /// This method never throws an exception. It will return the specified default value if the value cannot be converted to T.
        /// </summary>
        /// <typeparam name="T">The desired return type.</typeparam>
        /// <param name="context">The execution context.</param>
        /// <param name="value">The value to convert.</param>
        /// <param name="defaultValueFactory">A factory to get a default value if the value cannot be converted to type T.</param>
        /// <returns>The value converted to type T or the specified default value if the value cannot be converted to type T.</returns>
        public static T Convert<T>(this IExecutionContext context, object value, Func<T> defaultValueFactory) =>
            context.TryConvert<T>(value, out T result) ? result : (defaultValueFactory == null ? default : defaultValueFactory());

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
            IEnumerable<KeyValuePair<string, object>> metadata = null) =>
            await context.ExecuteAsync(modules, new[] { await context.NewGetDocumentAsync(metadata: metadata) });
    }
}
