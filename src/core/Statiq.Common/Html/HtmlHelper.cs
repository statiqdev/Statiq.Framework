using AngleSharp.Html.Parser;

namespace Statiq.Common
{
    public static class HtmlHelper
    {
        /// <summary>
        /// A default shared <see cref="HtmlParser"/> that does not consume (decode)
        /// character references.
        /// </summary>
        public static readonly HtmlParser DefaultHtmlParser = new HtmlParser(
            new HtmlParserOptions
            {
                IsNotConsumingCharacterReferences = true,
            });
    }
}