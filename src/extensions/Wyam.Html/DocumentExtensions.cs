using System;
using System.IO;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Tracing;

namespace Wyam.Html
{
    public static class DocumentExtensions
    {
        /// <summary>
        /// Gets an <see cref="IHtmlDocument"/> by parsing the content of an <see cref="IDocument"/>.
        /// </summary>
        /// <param name="document">The document to parse.</param>
        /// <returns>The parsed HTML document.</returns>
        public static async Task<IHtmlDocument> ParseHtmlAsync(this IDocument document) =>
            await ParseHtmlAsync(document, new HtmlParser());

        /// <summary>
        /// Gets an <see cref="IHtmlDocument"/> by parsing the content of an <see cref="IDocument"/>.
        /// </summary>
        /// <param name="document">The document to parse.</param>
        /// <param name="parser">A parser instance.</param>
        /// <returns>The parsed HTML document.</returns>
        public static async Task<IHtmlDocument> ParseHtmlAsync(this IDocument document, HtmlParser parser)
        {
            try
            {
                using (Stream stream = await document.GetStreamAsync())
                {
                    return await parser.ParseAsync(stream);
                }
            }
            catch (Exception ex)
            {
                Trace.Warning("Exception while parsing HTML for {0}: {1}", document.Source.ToDisplayString(), ex.Message);
            }
            return null;
        }
    }
}
