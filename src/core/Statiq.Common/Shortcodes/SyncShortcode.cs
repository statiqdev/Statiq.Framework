using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A base class for synchronous shortcodes.
    /// </summary>
    public abstract class SyncShortcode : IShortcode
    {
        /// <summary>
        /// Executes the shortcode and returns an <see cref="IDocument"/> with the shortcode result content and metadata.
        /// </summary>
        /// <param name="args">
        /// The arguments declared with the shortcode. This contains a list of key-value pairs in the order
        /// they appeared in the shortcode declaration. If no key was specified, then the <see cref="KeyValuePair{TKey, TValue}.Key"/>
        /// property will be <c>null</c>.
        /// </param>
        /// <param name="content">The content of the shortcode.</param>
        /// <param name="document">The current document (including metadata from previous shortcodes in the same document).</param>
        /// <param name="context">The current execution context.</param>
        /// <returns>
        /// A shortcode result that contains a collection of documents containing a stream and new metadata as a result of executing this shortcode.
        /// The result can be <c>null</c> in which case the shortcode declaration will be removed from the document
        /// but no replacement content will be added and the metadata will not change.
        /// </returns>
        public abstract IEnumerable<IDocument> Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context);

        /// <inheritdoc />
        Task<IEnumerable<IDocument>> IShortcode.ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
            Task.FromResult(Execute(args, content, document, context));
    }
}
