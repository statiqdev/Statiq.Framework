using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.Shortcodes;

namespace Statiq.Core.Shortcodes.Html
{
    /// <summary>
    /// Embeds a YouTube video.
    /// </summary>
    /// <remarks>
    /// You only need the ID of the video which can be obtained from it's URL after the <c>?v=</c>:
    /// <code>
    /// https://www.youtube.com/watch?v=u5ayTqlLWQQ
    /// </code>
    /// </remarks>
    /// <example>
    /// <code>
    /// &lt;?# YouTube u5ayTqlLWQQ /?&gt;
    /// </code>
    /// </example>
    /// <parameter>The ID of the video.</parameter>
    public class YouTube : Embed
    {
        public override async Task<IDocument> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
            await ExecuteAsync("https://www.youtube.com/oembed", $"https://www.youtube.com/watch?v={args.SingleValue()}", context);
    }
}
