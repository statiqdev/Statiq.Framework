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
    public abstract class ConfigModule<TValue> : Module
    {
        private readonly Config<TValue> _config;
        private readonly bool _eachDocument;

        /// <summary>
        /// Creates a new config module.
        /// </summary>
        /// <param name="config">The delegate to use for getting a config value.</param>
        /// <param name="eachDocument">
        /// <c>true</c> to call <see cref="ExecuteConfigAsync(IDocument, IExecutionContext, TValue)"/> for each
        /// input document regardless of whether the config delegate requires a document or <c>false</c>
        /// to allow only calling <see cref="ExecuteConfigAsync(IDocument, IExecutionContext, TValue)"/> once
        /// with a null input document if the config delegate does not require a document.
        /// </param>
        protected ConfigModule(Config<TValue> config, bool eachDocument)
        {
            _config = config;
            _eachDocument = eachDocument || (config?.RequiresDocument ?? throw new ArgumentNullException(nameof(config)));
        }

        /// <inheritdoc />
        protected sealed override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            if (_eachDocument)
            {
                IEnumerable<IDocument> aggregateResults = null;
                TValue value = default;

                // Only need to evaluate the config delegate once
                if (!_config.RequiresDocument)
                {
                    value = await _config.GetValueAsync(null, context);
                }

                // Iterate the inputs
                foreach (IDocument input in context.Inputs)
                {
                    // If the config requires a document, evaluate it each time
                    if (_config.RequiresDocument)
                    {
                        value = await _config.GetValueAsync(input, context);
                    }

                    // Get the results for this input document
                    IEnumerable<IDocument> results = await ExecuteInputFunc(input, context, (i, c) => ExecuteConfigAsync(i, c, value));
                    if (results != null)
                    {
                        aggregateResults = aggregateResults?.Concat(results) ?? results;
                    }
                }

                return aggregateResults;
            }

            return await ExecuteConfigAsync(null, context, await _config.GetValueAsync(null, context));
        }

        /// <inheritdoc />
        // Unused, prevent overriding in derived classes
        protected sealed override Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context) =>
            base.ExecuteInputAsync(input, context);

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
        protected abstract Task<IEnumerable<IDocument>> ExecuteConfigAsync(IDocument input, IExecutionContext context, TValue value);
    }
}