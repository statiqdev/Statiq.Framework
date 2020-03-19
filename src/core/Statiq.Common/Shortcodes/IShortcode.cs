using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// Contains the code for a given shortcode (see the <c>Shortcodes</c> module).
    /// </summary>
    /// <remarks>
    /// Shortcode instances are created once-per-document and reused for the life of that document.
    /// An exception is that nested shortcodes are always processed by a new instance of the shortcode
    /// implementation (which remains in use for that nested content). If a shortcode class also
    /// implements <see cref="IDisposable"/>, the shortcode will be disposed at the processing conclusion
    /// of the document or nested content. The input is cloned with each shortcode result document in order,
    /// so if each result sets a particular metadata value, the last one is what gets set in the host document.
    /// </remarks>
    public interface IShortcode
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
        Task<IEnumerable<IDocument>> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context);
    }
}
