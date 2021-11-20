using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Dom.Events;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.Text;
using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    public static class IDocumentHtmlExtensions
    {
        // Statically cache generated IHtmlDocument instances keyed to the content provider
        private static readonly ConcurrentCache<IContentProvider, Task<IHtmlDocument>> _htmlDocumentCache =
            new ConcurrentCache<IContentProvider, Task<IHtmlDocument>>(true);

        internal static void ClearHtmlDocumentCache() => _htmlDocumentCache.Reset();

        internal static void AddOrUpdateCache(IContentProvider contentProvider, IHtmlDocument htmlDocument) =>
            _htmlDocumentCache.AddOrUpdate(contentProvider, _ => Task.FromResult(htmlDocument), (_, __) => Task.FromResult(htmlDocument));

        /// <summary>
        /// Gets an <see cref="IHtmlDocument"/> by parsing the content of an <see cref="IDocument"/>.
        /// </summary>
        /// <remarks>
        /// The default <see cref="HtmlParser"/> has <see cref="HtmlParserOptions.IsNotConsumingCharacterReferences"/>
        /// set to <c>true</c> so that character references are not decoded when parsing (which is important for passing
        /// encoded characters like <c>@</c> through to engines like Razor that require them to be encoded if literal).
        /// </remarks>
        /// <param name="document">The document to parse.</param>
        /// <param name="clone">
        /// Set to <c>true</c> if potentially modifying the result, <c>false</c> if only using as read-only.
        /// When <c>true</c> the resulting HTML document (found in the cache or parsed) is cloned before returning.
        /// If the HTML document is cloned, use <c>IExecutionContext.GetContentProvider(IHtmlDocument)</c> to
        /// get an updated content provider for the mutated HTML document and update the internal HTML document cache
        /// with the new content provider and HTML content.
        /// </param>
        /// <returns>The parsed HTML document.</returns>
        public static async Task<IHtmlDocument> ParseHtmlAsync(this IDocument document, bool clone = true)
        {
            IHtmlDocument htmlDocument = await _htmlDocumentCache.GetOrAdd(
                document.ContentProvider,
                async (_, doc) =>
                {
                    try
                    {
                        using (Stream stream = doc.GetContentStream())
                        {
                            return await HtmlHelper.DefaultHtmlParser.ParseDocumentAsync(stream);
                        }
                    }
                    catch (Exception ex)
                    {
                        doc.LogWarning($"Exception while parsing HTML: {ex.Message}");
                    }
                    return null;
                },
                document);

            // If we're cloning, it means we're also intending to update the document so replace it in the cache
            if (clone)
            {
                htmlDocument = (IHtmlDocument)htmlDocument.Clone();
            }
            return htmlDocument;
        }
    }
}