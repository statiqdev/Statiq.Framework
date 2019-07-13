using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// This class can be used as a base class for modules that operate on arbitrary content (as represented by an object).
    /// </summary>
    public abstract class ContentModule : IModule
    {
        private readonly DocumentConfig<string> _content;
        private readonly IModule[] _modules;

        /// <summary>
        /// Creates a new content module with the specified content delegate.
        /// If there are no input documents and the content delegate doesn't require a document, it will be still be evaluated without an initial document.
        /// </summary>
        /// <param name="content">The content delegate.</param>
        protected ContentModule(DocumentConfig<string> content) => _content = content;

        /// <summary>
        /// Creates a new content module with the content determined by child modules.
        /// </summary>
        /// <remarks>
        /// If only one input document is available, it will be used as the initial document for the specified modules.
        /// If more than one document is available, an empty collection of initial documents will be used.
        /// To force usage of each input document in a set (I.e., A, B, and C input documents specify a unique "template" metadata value and you want to append
        /// some result of operating on that template value to each), make the content module a child of the ForEach module.
        /// Each input will be applied against each result from the specified modules (I.e., if 2 inputs and the module chain results in 2 outputs, there will be 4 total outputs).
        /// </remarks>
        /// <param name="modules">The child modules.</param>
        protected ContentModule(params IModule[] modules)
        {
            _modules = modules;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IDocument>> ExecuteAsync(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            if (_modules != null)
            {
                IReadOnlyList<IDocument> documents = await context.ExecuteAsync(_modules, inputs.Count == 1 ? inputs : null);
                return await documents.SelectManyAsync(
                    context,
                    async x => await inputs.SelectAsync(context, async y => await ExecuteAsync(await x.GetStringAsync(), y, context)));
            }

            if (inputs.Count == 0)
            {
                if (_content.RequiresDocument)
                {
                    return Array.Empty<IDocument>();
                }
                return new[] { await ExecuteAsync(await _content.GetValueAsync(null, context), null, context) };
            }
            return await inputs.SelectAsync(context, async x => await ExecuteAsync(await _content.GetAndTransformValueAsync(x, context), x, context));
        }

        /// <summary>
        /// Executes the module with the specified content against a single document.
        /// Note that content can be passed in as null, implementers should guard against that.
        /// Also the input document can be null if the module was executed against an empty
        /// set of input documents and the content delegate does not require a document.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="input">The input document.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>Result documents.</returns>
        protected abstract Task<IDocument> ExecuteAsync(string content, IDocument input, IExecutionContext context);
    }
}
