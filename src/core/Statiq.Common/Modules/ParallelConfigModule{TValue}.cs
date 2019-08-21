using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// Base class for modules that rely on a config value and could apply to input documents in parallel
    /// (or not) depending on whether the config delegate requires them.
    /// </summary>
    public abstract class ParallelConfigModule<TValue> : Module, IParallelModule
    {
        private readonly Config<TValue> _config;
        private readonly bool _eachDocument;

        /// <inheritdoc />
        public bool Parallel { get; set; } = true;

        /// <summary>
        /// Creates a new config module.
        /// </summary>
        /// <param name="config">The delegate to use for getting a config value.</param>
        /// <param name="eachDocument">
        /// <c>true</c> to call <see cref="ExecuteAsync(IDocument, IExecutionContext, TValue)"/> for each
        /// input document regardless of whether the config delegate requires a document or <c>false</c>
        /// to allow only calling <see cref="ExecuteAsync(IDocument, IExecutionContext, TValue)"/> once
        /// with a null input document if the config delegate does not require a document.
        /// </param>
        protected ParallelConfigModule(Config<TValue> config, bool eachDocument)
        {
            _config = config;
            _eachDocument = eachDocument || (config?.RequiresDocument ?? throw new ArgumentNullException(nameof(config)));
        }

        /// <inheritdoc />
        public sealed override async Task<IEnumerable<IDocument>> ExecuteAsync(IExecutionContext context)
        {
            if (_eachDocument)
            {
                TValue value = default;

                // Only need to evaluate the config delegate once
                if (!_config.RequiresDocument)
                {
                    value = await _config.GetValueAsync(null, context);
                }

                return await context.Inputs.ParallelSelectManyAsync(input => ParallelExecuteAsync(input, context, value), context.CancellationToken);
            }

            return await ExecuteAsync(null, context, await _config.GetValueAsync(null, context));
        }

        private async Task<IEnumerable<IDocument>> ParallelExecuteAsync(IDocument input, IExecutionContext context, TValue value)
        {
            // If the config requires a document, evaluate it each time
            if (_config.RequiresDocument)
            {
                value = await _config.GetValueAsync(input, context);
            }

            // Get the results for this input document
            return await ExecuteInput(input, context, (i, c) => ExecuteAsync(i, c, value));
        }

        /// <inheritdoc />
        // Unused, prevent overriding in derived classes
        protected sealed override Task<IEnumerable<IDocument>> ExecuteAsync(IDocument input, IExecutionContext context) =>
            base.ExecuteAsync(input, context);

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
        protected abstract Task<IEnumerable<IDocument>> ExecuteAsync(IDocument input, IExecutionContext context, TValue value);
    }
}
