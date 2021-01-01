using System;
using System.IO;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Html
{
    public static class HtmlHelper
    {
        public static readonly HtmlParser DefaultHtmlParser = new HtmlParser();

        // Statically cache generated IHtmlDocument instances keyed to the content provider
        private static readonly ConcurrentCache<IContentProvider, Task<IHtmlDocument>> _htmlDocumentCache = new ConcurrentCache<IContentProvider, Task<IHtmlDocument>>();

        internal static void ClearHtmlDocumentCache() => _htmlDocumentCache.Clear();

        /// <summary>
        /// Gets an <see cref="IHtmlDocument"/> by parsing the content of an <see cref="IDocument"/>.
        /// </summary>
        /// <param name="document">The document to parse.</param>
        /// <param name="clone">Set to <c>true</c> if potentially modifying the result, <c>false</c> if only using as read-only.</param>
        /// <returns>The parsed HTML document.</returns>
        // Since the results are cached, we have to be careful to use the same HtmlParser. If an alternate set
        // of parser options are needed, the content will need to be parsed manually.
        public static async Task<IHtmlDocument> ParseHtmlAsync(IDocument document, bool clone = true)
        {
            IHtmlDocument htmlDocument = await _htmlDocumentCache.GetOrAdd(
                document.ContentProvider,
                async _ =>
                {
                    try
                    {
                        using (Stream stream = document.GetContentStream())
                        {
                            return await DefaultHtmlParser.ParseDocumentAsync(stream);
                        }
                    }
                    catch (Exception ex)
                    {
                        document.LogWarning($"Exception while parsing HTML: {ex.Message}");
                    }
                    return null;
                });
            if (clone)
            {
                htmlDocument = (IHtmlDocument)htmlDocument.Clone();
            }
            return htmlDocument;
        }

        public static async Task AddOrUpdateCacheAsync(IDocument document, IHtmlDocument htmlDocument) =>
            await _htmlDocumentCache.AddOrUpdate(document.ContentProvider, _ => Task.FromResult(htmlDocument), (_, __) => Task.FromResult(htmlDocument));
    }
}
