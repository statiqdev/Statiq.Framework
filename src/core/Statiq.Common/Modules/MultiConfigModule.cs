using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// Base class for modules that rely on multiple config values and could apply to input documents (or not) depending
    /// on whether the config delegates require them.
    /// </summary>
    public abstract class MultiConfigModule : Module
    {
        private readonly Dictionary<string, IConfig> _configs = new Dictionary<string, IConfig>();
        private readonly bool _forceDocumentExecution;

        /// <summary>
        /// Creates a new config module.
        /// </summary>
        /// <param name="configs">
        /// The delegates to use for getting a config value.
        /// </param>
        /// <param name="forceDocumentExecution">
        /// <c>true</c> to force calling <see cref="ExecuteConfigAsync(IDocument, IExecutionContext, ImmutableDictionary{string, object})"/> for each
        /// input document regardless of whether the config delegate requires a document or <c>false</c>
        /// to allow calling <see cref="ExecuteConfigAsync(IDocument, IExecutionContext, ImmutableDictionary{string, object})"/> once
        /// with a null input document if the config delegate does not require a document.
        /// </param>
        protected MultiConfigModule(IEnumerable<KeyValuePair<string, IConfig>> configs, bool forceDocumentExecution)
        {
            if (configs != null)
            {
                foreach (KeyValuePair<string, IConfig> config in configs)
                {
                    SetConfig(config.Key, config.Value);
                }
            }
            _forceDocumentExecution = forceDocumentExecution;
        }

        /// <summary>
        /// Creates a new config module.
        /// </summary>
        /// <param name="forceDocumentExecution">
        /// <c>true</c> to force calling <see cref="ExecuteConfigAsync(IDocument, IExecutionContext, ImmutableDictionary{string, object})"/> for each
        /// input document regardless of whether the config delegate requires a document or <c>false</c>
        /// to allow calling <see cref="ExecuteConfigAsync(IDocument, IExecutionContext, ImmutableDictionary{string, object})"/> once
        /// with a null input document if the config delegate does not require a document.
        /// </param>
        protected MultiConfigModule(bool forceDocumentExecution)
            : this(null, forceDocumentExecution)
        {
        }

        protected MultiConfigModule SetConfig(string key, IConfig config)
        {
            _ = key ?? throw new ArgumentNullException(nameof(key));
            if (config == null)
            {
                _configs.Remove(key);
            }
            else
            {
                _configs[key] = config;
            }
            return this;
        }

        /// <inheritdoc />
        protected sealed override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            ImmutableDictionary<string, object>.Builder valuesBuilder;

            if (_forceDocumentExecution || _configs.Any(x => x.Value.RequiresDocument))
            {
                // Only need to evaluate the context config delegates once
                valuesBuilder = ImmutableDictionary.CreateBuilder<string, object>();
                foreach (KeyValuePair<string, IConfig> config in _configs.Where(x => !x.Value.RequiresDocument))
                {
                    valuesBuilder[config.Key] = await config.Value.GetValueAsync(null, context);
                }
                ImmutableDictionary<string, object> configValues = valuesBuilder.ToImmutable();

                // Iterate the inputs
                IEnumerable<IDocument> aggregateResults = null;
                foreach (IDocument input in context.Inputs)
                {
                    // If the config requires a document, evaluate it each time
                    valuesBuilder = configValues.ToBuilder();
                    foreach (KeyValuePair<string, IConfig> config in _configs.Where(x => x.Value.RequiresDocument))
                    {
                        valuesBuilder[config.Key] = await config.Value.GetValueAsync(input, context);
                    }

                    // Get the results for this input document
                    IEnumerable<IDocument> results = await ExecuteInputFunc(input, context, (i, c) => ExecuteConfigAsync(i, c, valuesBuilder.ToImmutable()));
                    if (results != null)
                    {
                        aggregateResults = aggregateResults?.Concat(results) ?? results;
                    }
                }
                return aggregateResults;
            }

            // Only context configs
            valuesBuilder = ImmutableDictionary.CreateBuilder<string, object>();
            foreach (KeyValuePair<string, IConfig> config in _configs)
            {
                valuesBuilder[config.Key] = await config.Value.GetValueAsync(null, context);
            }
            return await ExecuteConfigAsync(null, context, valuesBuilder.ToImmutable());
        }

        /// <inheritdoc />
        // Unused, prevent overriding in derived classes
        protected sealed override Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context) =>
            base.ExecuteInputAsync(input, context);

        /// <summary>
        /// Executes the module for each input document.
        /// If there aren't any input documents and the config delegate doesn't require documents,
        /// this will be called once with a null <paramref name="input"/>.
        /// </summary>
        /// <param name="input">
        /// The input document this module is currently applying to or <c>null</c> if there aren't any
        /// input documents or if the config delegate doesn't require documents.
        /// </param>
        /// <param name="context">The execution context.</param>
        /// <param name="values">The evaluated config values.</param>
        /// <returns>The result documents.</returns>
        protected abstract Task<IEnumerable<IDocument>> ExecuteConfigAsync(IDocument input, IExecutionContext context, ImmutableDictionary<string, object> values);
    }
}