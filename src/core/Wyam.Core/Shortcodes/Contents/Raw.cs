using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Shortcodes;

namespace Wyam.Core.Shortcodes.Contents
{
    /// <summary>
    /// A special shortcode that will output whatever is in it's content.
    /// </summary>
    /// <remarks>
    /// This will not evaluate nested shortcodes and is useful
    /// for escaping shortcode syntax.
    /// </remarks>
    /// <example>
    /// <code>
    /// &lt;?# Raw ?>&lt;?# ThisWillBeOutputVerbatim ?>&lt;?#/ Raw ?>
    /// </code>
    /// </example>
    public class Raw : IShortcode
    {
        /// <inheritdoc />
        public async Task<IShortcodeResult> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
            await context.GetShortcodeResultAsync(content);
    }
}
