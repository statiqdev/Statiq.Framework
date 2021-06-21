using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Lunr
{
    /// <summary>
    /// A search item for a document.
    /// </summary>
    public class LunrIndexDocItem : ILunrIndexItem
    {
        private readonly string _title;
        private readonly string _content;

        /// <summary>
        /// Creates a search item from a document using <see cref="IDocumentExtensions.GetTitle(IDocument)"/>
        /// for the title and the document content as the search item content.
        /// </summary>
        public LunrIndexDocItem(IDocument document)
        {
            Document = document.ThrowIfNull(nameof(document));
        }

        /// <summary>
        /// Creates a search item from a document using the specified title and content.
        /// </summary>
        /// <param name="document">The document this search item should point to.</param>
        /// <param name="title">The title to use for the search item, or <c>null</c> to use <see cref="IDocumentExtensions.GetTitle(IDocument)"/>.</param>
        /// <param name="content">The content to use for the search item, or <c>null to use the document content</c>.</param>
        public LunrIndexDocItem(IDocument document, string title, string content)
        {
            Document = document.ThrowIfNull(nameof(document));
            _title = title;
            _content = content;
        }

        /// <summary>
        /// The document the search item points to.
        /// </summary>
        public IDocument Document { get; }

        /// <inheritdoc />
        public string Title => _title is object ? _title : Document.GetTitle();

        /// <inheritdoc />
        public string Description { get; set; }

        /// <inheritdoc />
        public string Tags { get; set; }

        /// <inheritdoc />
        public async Task<string> GetContentAsync()
        {
            // Return the explicit content if we have some...
            if (_content is object)
            {
                return _content;
            }

            // ...otherwise return the document content
            return await Document.GetContentStringAsync();
        }

        /// <inheritdoc />
        public string GetLink(IExecutionContext context, bool includeHost) =>
            context.GetLink(Document, includeHost);
    }
}