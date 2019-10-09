using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// Base class for modules that rely on multiple config values and could apply to input documents in parallel
    /// (or not) depending on whether the config delegate requires them.
    /// </summary>
    public abstract class ParallelMultiConfigModule : Module, IParallelModule
    {
        private readonly Dictionary<string, IConfig> _configs = new Dictionary<string, IConfig>(StringComparer.OrdinalIgnoreCase);
        private readonly bool _forceDocumentExecution;

        /// <inheritdoc />
        public bool Parallel { get; set; } = true;

        /// <summary>
        /// Creates a new config module.
        /// </summary>
        /// <param name="configs">
        /// The delegates to use for getting a config value.
        /// </param>
        /// <param name="forceDocumentExecution">
        /// <c>true</c> to force calling <see cref="ExecuteConfigAsync(IDocument, IExecutionContext, IMetadata)"/> for each
        /// input document regardless of whether the config delegate requires a document or <c>false</c>
        /// to allow calling <see cref="ExecuteConfigAsync(IDocument, IExecutionContext, IMetadata)"/> once
        /// with a null input document if the config delegate does not require a document.
        /// </param>
        protected ParallelMultiConfigModule(IEnumerable<KeyValuePair<string, IConfig>> configs, bool forceDocumentExecution)
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
        /// <c>true</c> to force calling <see cref="ExecuteConfigAsync(IDocument, IExecutionContext, IMetadata)"/> for each
        /// input document regardless of whether the config delegate requires a document or <c>false</c>
        /// to allow calling <see cref="ExecuteConfigAsync(IDocument, IExecutionContext, IMetadata)"/> once
        /// with a null input document if the config delegate does not require a document.
        /// </param>
        protected ParallelMultiConfigModule(bool forceDocumentExecution)
            : this(null, forceDocumentExecution)
        {
        }

        protected ParallelMultiConfigModule SetConfig(string key, IConfig config)
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
            if (_forceDocumentExecution || _configs.Any(x => x.Value.RequiresDocument))
            {
                // Only need to evaluate the context config delegates once
                ImmutableDictionary<string, object>.Builder configValuesBuilder = ImmutableDictionary.CreateBuilder<string, object>(StringComparer.OrdinalIgnoreCase);
                foreach (KeyValuePair<string, IConfig> config in _configs.Where(x => !x.Value.RequiresDocument))
                {
                    configValuesBuilder[config.Key] = await config.Value.GetValueAsync(null, context);
                }
                ImmutableDictionary<string, object> configValues = configValuesBuilder.ToImmutable();

                // Parallel
                if (Parallel && !context.SerialExecution)
                {
                    return await context.Inputs.ParallelSelectManyAsync(
                        async input =>
                        {
                            // If the config requires a document, evaluate it each time
                            ImmutableDictionary<string, object>.Builder valuesBuilder = configValues.ToBuilder();
                            foreach (KeyValuePair<string, IConfig> config in _configs.Where(x => x.Value.RequiresDocument))
                            {
                                valuesBuilder[config.Key] = await config.Value.GetValueAsync(input, context);
                            }
                            IMetadata values = new ReadOnlyConvertingDictionary(valuesBuilder.ToImmutable());
                            return await ExecuteInputFunc(input, context, (i, c) => ExecuteConfigAsync(i, c, values));
                        },
                        context.CancellationToken);
                }

                // Not parallel
                IEnumerable<IDocument> aggregateResults = null;
                foreach (IDocument input in context.Inputs)
                {
                    // If the config requires a document, evaluate it each time
                    configValuesBuilder = configValues.ToBuilder();
                    foreach (KeyValuePair<string, IConfig> config in _configs.Where(x => x.Value.RequiresDocument))
                    {
                        configValuesBuilder[config.Key] = await config.Value.GetValueAsync(input, context);
                    }

                    // Get the results for this input document
                    IMetadata values = new ReadOnlyConvertingDictionary(configValuesBuilder.ToImmutable());
                    IEnumerable<IDocument> results = await ExecuteInputFunc(input, context, (i, c) => ExecuteConfigAsync(i, c, values));
                    if (results != null)
                    {
                        aggregateResults = aggregateResults?.Concat(results) ?? results;
                    }
                }
                return aggregateResults;
            }
            else
            {
                // Only context configs
                ImmutableDictionary<string, object>.Builder valuesBuilder = ImmutableDictionary.CreateBuilder<string, object>();
                foreach (KeyValuePair<string, IConfig> config in _configs)
                {
                    valuesBuilder[config.Key] = await config.Value.GetValueAsync(null, context);
                }
                IMetadata values = new ReadOnlyConvertingDictionary(valuesBuilder.ToImmutable());
                return await ExecuteConfigAsync(null, context, values);
            }
        }

        /// <inheritdoc />
        // Unused, prevent overriding in derived classes
        protected sealed override Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context) =>
            throw new NotSupportedException();

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
        /// <param name="values">The evaluated config values.</param>
        /// <returns>The result documents.</returns>
        protected abstract Task<IEnumerable<IDocument>> ExecuteConfigAsync(IDocument input, IExecutionContext context, IMetadata values);
    }
}