using System;
using System.IO;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Html
{
    public static class DocumentExtensions
    {
        /// <summary>
        /// Gets an <see cref="IHtmlDocument"/> by parsing the content of an <see cref="IDocument"/>.
        /// </summary>
        /// <param name="document">The document to parse.</param>
        /// <param name="context">The current execution context.</param>
        /// <returns>The parsed HTML document.</returns>
        public static async Task<IHtmlDocument> ParseHtmlAsync(this IDocument document, IExecutionContext context) =>
            await ParseHtmlAsync(document, context, new HtmlParser());

        /// <summary>
        /// Gets an <see cref="IHtmlDocument"/> by parsing the content of an <see cref="IDocument"/>.
        /// </summary>
        /// <param name="document">The document to parse.</param>
        /// <param name="context">The current execution context.</param>
        /// <param name="parser">A parser instance.</param>
        /// <returns>The parsed HTML document.</returns>
        public static async Task<IHtmlDocument> ParseHtmlAsync(this IDocument document, IExecutionContext context, HtmlParser parser)
        {
            try
            {
                using (Stream stream = document.GetContentStream())
                {
                    return await parser.ParseAsync(stream);
                }
            }
            catch (Exception ex)
            {
                context.LogWarning("Exception while parsing HTML for {0}: {1}", document.ToSafeDisplayString(), ex.Message);
            }
            return null;
        }
    }
}
