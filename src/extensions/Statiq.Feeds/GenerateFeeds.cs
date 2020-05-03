using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;
using Statiq.Feeds.Syndication;

namespace Statiq.Feeds
{
    /// <summary>
    /// Generates syndication feeds including RSS, Atom, and RDF.
    /// </summary>
    /// <remarks>
    /// Each input document represents an item in the feed, up to the specified maximum number of
    /// documents. Note that documents will be sorted in descending order of their published date
    /// to correspond with conventional feed behavior. If you want to use a different ordering,
    /// sort the documents before using this module and call <see cref="PreserveOrdering(bool)"/>.
    /// You should also configure the "Host" setting since that's used to create absolute links.
    /// This module outputs a document for each of the selected feed types. Input documents
    /// are not output by this module.
    /// </remarks>
    /// <metadata cref="FeedKeys.Title" usage="Input"/>
    /// <metadata cref="FeedKeys.Description" usage="Input"/>
    /// <metadata cref="FeedKeys.Author" usage="Input"/>
    /// <metadata cref="FeedKeys.Image" usage="Input"/>
    /// <metadata cref="FeedKeys.Copyright" usage="Input"/>
    /// <metadata cref="FeedKeys.Excerpt" usage="Input"/>
    /// <metadata cref="FeedKeys.Published" usage="Input"/>
    /// <metadata cref="FeedKeys.Updated" usage="Input"/>
    /// <metadata cref="FeedKeys.Content" usage="Input"/>
    /// <category>Content</category>
    public class GenerateFeeds : Module
    {
        /// <summary>
        /// The default path for RSS files.
        /// </summary>
        public static readonly NormalizedPath DefaultRssPath = new NormalizedPath("feed.rss");

        /// <summary>
        /// The default path for Atom files.
        /// </summary>
        public static readonly NormalizedPath DefaultAtomPath = new NormalizedPath("feed.atom");

        /// <summary>
        /// The default path for RDF files.
        /// </summary>
        public static readonly NormalizedPath DefaultRdfPath = null;

        private int _maximumItems = 20;
        private bool _preserveOrdering = false;
        private NormalizedPath _rssPath = DefaultRssPath;
        private NormalizedPath _atomPath = DefaultAtomPath;
        private NormalizedPath _rdfPath = DefaultRdfPath;

        private Uri _feedId;
        private string _feedTitle;
        private string _feedDescription;
        private string _feedAuthor;
        private DateTime? _feedPublished;
        private DateTime? _feedUpdated;
        private Uri _feedLink;
        private Uri _feedImageLink;
        private string _feedCopyright;

        private Config<Uri> _itemId = Config.FromDocument((doc, ctx) => TypeHelper.Convert<Uri>(ctx.GetLink(doc, true)));
        private Config<string> _itemTitle = Config.FromDocument(doc => doc.GetString(FeedKeys.Title));
        private Config<string> _itemDescription = Config.FromDocument(doc => doc.GetString(FeedKeys.Description) ?? doc.GetString(FeedKeys.Excerpt));
        private Config<string> _itemAuthor = Config.FromDocument(doc => doc.GetString(FeedKeys.Author));
        private Config<DateTime?> _itemPublished = Config.FromDocument(doc => doc.Get<DateTime?>(FeedKeys.Published));
        private Config<DateTime?> _itemUpdated = Config.FromDocument(doc => doc.Get<DateTime?>(FeedKeys.Updated));
        private Config<Uri> _itemLink = Config.FromDocument((doc, ctx) => TypeHelper.Convert<Uri>(ctx.GetLink(doc, true)));
        private Config<Uri> _itemImageLink = Config.FromDocument((doc, ctx) => TypeHelper.Convert<Uri>(ctx.GetLink(doc, FeedKeys.Image, true)));
        private Config<string> _itemContent = Config.FromDocument(async doc => doc.GetString(FeedKeys.Content) ?? await doc.GetContentStringAsync());
        private Config<Uri> _itemThreadLink = null;
        private Config<int> _itemThreadCount = null;
        private Config<DateTime?> _itemThreadUpdated = null;

        /// <summary>
        /// Sets how many items the feed will contain. The default value is 20.
        /// Note that documents will be used in the order in which they are input
        /// into this module, so a <c>OrderBy</c> module or similar should be used
        /// to order the documents prior to this module. Use a value of 0 to include
        /// all input documents.
        /// </summary>
        /// <param name="maximumItems">The maximum number of items.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds MaximumItems(int maximumItems)
        {
            _maximumItems = maximumItems;
            return this;
        }

        /// <summary>
        /// By default documents are sorted in descending publish order. Use this
        /// method to preserve their input document ordering instead.
        /// </summary>
        /// <param name="preserveOrdering"><c>true</c> to preserve input ordering, <c>false</c> otherwise.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds PreserveOrdering(bool preserveOrdering = true)
        {
            _preserveOrdering = true;
            return this;
        }

        /// <summary>
        /// Sets the path to the generated RSS file. The default behavior is to generate a RSS feed with
        /// a path of "feed.rss".
        /// </summary>
        /// <param name="rssPath">A delegate that should return a <see cref="NormalizedPath"/> for the RSS path.
        /// If the delegate is <c>null</c> or returns <c>null</c>, no RSS file will be generated.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithRssPath(NormalizedPath rssPath)
        {
            _rssPath = rssPath;
            return this;
        }

        /// <summary>
        /// Sets the path to the generated Atom file. The default behavior is to generate a RSS feed with
        /// a path of "feed.atom".
        /// </summary>
        /// <param name="atomPath">A delegate that should return a <see cref="NormalizedPath"/> for the Atom path.
        /// If the delegate is <c>null</c> or returns <c>null</c>, no Atom file will be generated.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithAtomPath(NormalizedPath atomPath)
        {
            _atomPath = atomPath;
            return this;
        }

        /// <summary>
        /// Sets the path to the generated RDF file. The default behavior is not to generate a RDF feed.
        /// </summary>
        /// <param name="rdfPath">A delegate that should return a <see cref="NormalizedPath"/> for the RDF path.
        /// If the delegate is <c>null</c> or returns <c>null</c>, no RDF file will be generated.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithRdfPath(NormalizedPath rdfPath)
        {
            _rdfPath = rdfPath;
            return this;
        }

        /// <summary>
        /// Sets the feed identifier. The default value is a link to the site.
        /// </summary>
        /// <param name="feedId">A delegate that should return a <c>Uri</c> with
        /// the feed identifier.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithFeedId(Uri feedId)
        {
            _feedId = feedId;
            return this;
        }

        /// <summary>
        /// Sets the feed title. The default value is the value for the "Title" key
        /// in the global metadata.
        /// </summary>
        /// <param name="feedTitle">A delegate that should return a <c>string</c> with
        /// the feed title.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithFeedTitle(string feedTitle)
        {
            _feedTitle = feedTitle;
            return this;
        }

        /// <summary>
        /// Sets the feed description. The default value is the value for the "Description" key
        /// in the global metadata.
        /// </summary>
        /// <param name="feedDescription">A delegate that should return a <c>string</c> with
        /// the feed description.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithFeedDescription(string feedDescription)
        {
            _feedDescription = feedDescription;
            return this;
        }

        /// <summary>
        /// Sets the feed author. The default value is the value for the "Author" key
        /// in the global metadata.
        /// </summary>
        /// <param name="feedAuthor">A delegate that should return a <c>string</c> with
        /// the feed author.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithFeedAuthor(string feedAuthor)
        {
            _feedAuthor = feedAuthor;
            return this;
        }

        /// <summary>
        /// Sets the feed published time. The default value is the current UTC time.
        /// </summary>
        /// <param name="feedPublished">A delegate that should return a <c>DateTime</c> with
        /// the feed published time.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithFeedPublished(DateTime? feedPublished)
        {
            _feedPublished = feedPublished;
            return this;
        }

        /// <summary>
        /// Sets the feed updated time. The default value is the current UTC time.
        /// </summary>
        /// <param name="feedUpdated">A delegate that should return a <c>DateTime</c> with
        /// the feed updated time.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithFeedUpdated(DateTime? feedUpdated)
        {
            _feedUpdated = feedUpdated;
            return this;
        }

        /// <summary>
        /// Sets the feed image link. The default value is the site link.
        /// </summary>
        /// <param name="feedLink">A delegate that should return a <c>Uri</c> with
        /// the feed link.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithFeedLink(Uri feedLink)
        {
            _feedLink = feedLink;
            return this;
        }

        /// <summary>
        /// Sets the feed image link. The default value is the value for the "Image" key
        /// in the global metadata.
        /// </summary>
        /// <param name="feedImageLink">A delegate that should return a <c>Uri</c> with
        /// the feed image link.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithFeedImageLink(Uri feedImageLink)
        {
            _feedImageLink = feedImageLink;
            return this;
        }

        /// <summary>
        /// Sets the feed copyright. The default value is the value for the "Copyright" key
        /// in the global metadata.
        /// </summary>
        /// <param name="feedCopyright">A delegate that should return a <c>string</c> with
        /// the feed copyright.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithFeedCopyright(string feedCopyright)
        {
            _feedCopyright = feedCopyright;
            return this;
        }

        /// <summary>
        /// Sets the item identifier. The default value is the absolute link to the input document.
        /// </summary>
        /// <param name="itemId">A delegate that should return a <c>Uri</c> with
        /// the item identifier.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemId(Config<Uri> itemId)
        {
            _itemId = itemId;
            return this;
        }

        /// <summary>
        /// Sets the item title. The default value is the value for the "Title" key
        /// in the input document.
        /// </summary>
        /// <param name="itemTitle">A delegate that should return a <c>string</c> with
        /// the item title.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemTitle(Config<string> itemTitle)
        {
            _itemTitle = itemTitle;
            return this;
        }

        /// <summary>
        /// Sets the item description. The default value is the value for the "Description" key
        /// in the input document.
        /// </summary>
        /// <param name="itemDescription">A delegate that should return a <c>string</c> with
        /// the item description.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemDescription(Config<string> itemDescription)
        {
            _itemDescription = itemDescription;
            return this;
        }

        /// <summary>
        /// Sets the item author. The default value is the value for the "Author" key
        /// in the input document.
        /// </summary>
        /// <param name="itemAuthor">A delegate that should return a <c>string</c> with
        /// the item author.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemAuthor(Config<string> itemAuthor)
        {
            _itemAuthor = itemAuthor;
            return this;
        }

        /// <summary>
        /// Sets the item published time. The default value is the value for the "Published" key
        /// in the input document.
        /// </summary>
        /// <param name="itemPublished">A delegate that should return a <c>DateTime</c> with
        /// the item published time.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemPublished(Config<DateTime?> itemPublished)
        {
            _itemPublished = itemPublished;
            return this;
        }

        /// <summary>
        /// Sets the item updated time. The default value is the value for the "Updated" key
        /// in the input document.
        /// </summary>
        /// <param name="itemUpdated">A delegate that should return a <c>DateTime</c> with
        /// the item updated time.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemUpdated(Config<DateTime?> itemUpdated)
        {
            _itemUpdated = itemUpdated;
            return this;
        }

        /// <summary>
        /// Sets the item link. The default value is the absolute link to the input document.
        /// </summary>
        /// <param name="itemLink">A delegate that should return a <c>Uri</c> with
        /// the item link.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemLink(Config<Uri> itemLink)
        {
            _itemLink = itemLink;
            return this;
        }

        /// <summary>
        /// Sets the item image link. The default value is the value for the "Image" key
        /// in the input document.
        /// </summary>
        /// <param name="itemImageLink">A delegate that should return a <c>Uri</c> with
        /// the item image link.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemImageLink(Config<Uri> itemImageLink)
        {
            _itemImageLink = itemImageLink;
            return this;
        }

        /// <summary>
        /// Sets the content of the item. The default value is the value for the "Content" key
        /// in the input document. If that is undefined, the current document content will be used.
        /// </summary>
        /// <param name="itemContent">A delegate that should return a <c>string</c> with
        /// the content of the item.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemContent(Config<string> itemContent)
        {
            _itemContent = itemContent;
            return this;
        }

        /// <summary>
        /// Sets the item thread link. By default, no thread link is specified.
        /// </summary>
        /// <param name="itemThreadLink">A delegate that should return a <c>Uri</c> with
        /// the item thread link.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemThreadLink(Config<Uri> itemThreadLink)
        {
            _itemThreadLink = itemThreadLink;
            return this;
        }

        /// <summary>
        /// Sets the item thread count. By default, no thread count is specified.
        /// </summary>
        /// <param name="itemThreadCount">A delegate that should return an <c>int</c> with
        /// the item thread count.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemThreadCount(Config<int> itemThreadCount)
        {
            _itemThreadCount = itemThreadCount;
            return this;
        }

        /// <summary>
        /// Sets the item thread updated. By default, no thread updated time is specified.
        /// </summary>
        /// <param name="itemThreadUpdated">A delegate that should return a <c>DateTime</c> with
        /// the item thread updated time.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemThreadUpdated(Config<DateTime?> itemThreadUpdated)
        {
            _itemThreadUpdated = itemThreadUpdated;
            return this;
        }

        /// <inheritdoc />
        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            // Get the feed
            Feed feed = new Feed
            {
                ID = _feedId ?? TypeHelper.Convert<Uri>(context.GetLink()),
                Title = _feedTitle ?? context.Settings.GetString(FeedKeys.Title),
                Description = _feedDescription ?? context.Settings.GetString(FeedKeys.Description),
                Author = _feedAuthor ?? context.Settings.GetString(FeedKeys.Author),
                Published = _feedPublished ?? DateTime.UtcNow,
                Updated = _feedUpdated ?? DateTime.UtcNow,
                Link = _feedLink ?? TypeHelper.Convert<Uri>(context.GetLink()),
                ImageLink = _feedImageLink ?? TypeHelper.Convert<Uri>(context.GetLink(context.Settings, FeedKeys.Image, true)),
                Copyright = _feedCopyright ?? context.Settings.GetString(FeedKeys.Copyright) ?? DateTime.UtcNow.Year.ToString()
            };

            // Sort the items and take the maximum items
            List<(IDocument, DateTime?)> items = new List<(IDocument, DateTime?)>();
            if (_preserveOrdering)
            {
                // Preserve the input order, take first so we don't calculate unnecessary published dates
                foreach (IDocument input in _maximumItems <= 0 ? context.Inputs : context.Inputs.Take(_maximumItems))
                {
                    items.Add((input, await _itemPublished.GetValueAsync(input, context)));
                }
            }
            else
            {
                // Order by published date then take the maximum items
                foreach (IDocument input in context.Inputs)
                {
                    items.Add((input, await _itemPublished.GetValueAsync(input, context)));
                }
                IEnumerable<(IDocument, DateTime?)> orderedItems = items.OrderByDescending(x => x.Item2);
                items = _maximumItems <= 0 ? orderedItems.ToList() : orderedItems.Take(_maximumItems).ToList();
            }

            // Add items
            foreach ((IDocument, DateTime?) item in items)
            {
                feed.Items.Add(new FeedItem
                {
                    ID = await _itemId.GetValueAsync(item.Item1, context),
                    Title = await _itemTitle.GetValueAsync(item.Item1, context),
                    Description = await _itemDescription.GetValueAsync(item.Item1, context),
                    Author = await _itemAuthor.GetValueAsync(item.Item1, context),
                    Published = item.Item2,
                    Updated = await _itemUpdated.GetValueAsync(item.Item1, context),
                    Link = await _itemLink.GetValueAsync(item.Item1, context),
                    ImageLink = await _itemImageLink.GetValueAsync(item.Item1, context),
                    Content = await _itemContent.GetValueAsync(item.Item1, context),
                    ThreadLink = await _itemThreadLink.GetValueAsync(item.Item1, context),
                    ThreadCount = await _itemThreadCount.GetValueAsync(item.Item1, context),
                    ThreadUpdated = await _itemThreadUpdated.GetValueAsync(item.Item1, context)
                });
            }

            // Generate the feeds
            return new[]
            {
                await GenerateFeedAsync(FeedType.Rss, feed, _rssPath, context),
                await GenerateFeedAsync(FeedType.Atom, feed, _atomPath, context),
                await GenerateFeedAsync(FeedType.Rdf, feed, _rdfPath, context)
            }.Where(x => x != null);
        }

        private async Task<IDocument> GenerateFeedAsync(FeedType feedType, Feed feed, NormalizedPath path, IExecutionContext context)
        {
            // Get the output path
            if (path.IsNull)
            {
                return null;
            }
            if (!path.IsRelative)
            {
                throw new ArgumentException("The feed output path must be relative");
            }

            // Generate the feed and document
            using (Stream contentStream = await context.GetContentStreamAsync())
            {
                FeedSerializer.SerializeXml(feedType, feed, contentStream);
                return context.CreateDocument(path, context.GetContentProvider(contentStream, feedType.MediaType));
            }
        }
    }
}
