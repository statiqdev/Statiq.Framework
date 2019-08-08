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
        /// <inheritdoc />
        protected sealed override Task<IEnumerable<IDocument>> ExecuteAsync(IDocument input, IExecutionContext context, TValue value) =>
            Task.FromResult(Execute(input, context, value));

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
        protected abstract IEnumerable<IDocument> Execute(IDocument input, IExecutionContext context, TValue value);
    }
}