using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// Base class for modules that rely on a config value and could apply to input documents (or not) depending
    /// on whether the config delegate requires them.
    /// </summary>
    public abstract class ConfigModule<T> : IModule
    {
        private readonly DocumentConfig<T> _config;
        private readonly bool _alwaysApplyToInputs;

        /// <summary>
        /// Creates a new config module.
        /// </summary>
        /// <param name="config">The delegate to use for getting a config value.</param>
        /// <param name="alwaysApplyToInputs">
        /// <c>true</c> to call <see cref="ExecuteAsync(IDocument, IReadOnlyList{IDocument}, IExecutionContext, T)"/> for each
        /// input document regardless of whether the config delegate requires a document or <c>false</c>
        /// to allow only calling <see cref="ExecuteAsync(IDocument, IReadOnlyList{IDocument}, IExecutionContext, T)"/> once
        /// with a null input document if the config delegate does not require a document.
        /// </param>
        protected ConfigModule(DocumentConfig<T> config, bool alwaysApplyToInputs)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _alwaysApplyToInputs = alwaysApplyToInputs;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context) =>
            _config.RequiresDocument || _alwaysApplyToInputs
                ? await inputs.ParallelSelectManyAsync(context, async input => await ExecuteAsync(input, inputs, context, await _config.GetValueAsync(input, context)))
                : await ExecuteAsync(null, inputs, context, await _config.GetValueAsync(null, context));

        /// <summary>
        /// Executes the module for each input document in parallel.
        /// If there aren't any input documents and the config delegate doesn't require documents,
        /// this will be called once with a null <paramref name="input"/>.
        /// </summary>
        /// <param name="input">
        /// The input document this module is currently applying to or <c>null</c> if there aren't any
        /// input documents or if the config delegate doesn't require documents.
        /// </param>
        /// <param name="inputs">The original input documents.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="value">The evaluated config value.</param>
        /// <returns>The result documents.</returns>
        protected abstract Task<IEnumerable<IDocument>> ExecuteAsync(
            IDocument input,
            IReadOnlyList<IDocument> inputs,
            IExecutionContext context,
            T value);
    }
}
