using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Lunr
{
    /// <summary>
    /// A search item with an arbitrary URL.
    /// </summary>
    public class LunrIndexItem : ILunrIndexItem
    {
        /// <summary>
        /// Creates the search item.
        /// </summary>
        /// <param name="url">The URL this search item should point to.</param>
        /// <param name="title">The title of the search item.</param>
        /// <param name="content">The search item content.</param>
        public LunrIndexItem(string url, string title, string content)
        {
            Url = url;
            Title = title;
            Content = content;
        }

        /// <summary>
        /// The URL of the search item.
        /// </summary>
        public string Url { get; set; }

        /// <inheritdoc />
        public string Title { get; set; }

        /// <inheritdoc />
        public string Description { get; set; }

        public string Content { get; set; }

        /// <inheritdoc />
        public string Tags { get; set; }

        /// <inheritdoc />
        public Task<string> GetContentAsync() => Task.FromResult(Content);

        /// <inheritdoc />
        public string GetLink(IExecutionContext context, bool includeHost) =>
            context.GetLink(new NormalizedPath(Url), includeHost);
    }
}
