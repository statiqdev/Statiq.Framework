using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A base class for shortcodes.
    /// </summary>
    public abstract class Shortcode : IShortcode
    {
        /// <inheritdoc />
        public abstract Task<IDocument> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context);
    }
}
