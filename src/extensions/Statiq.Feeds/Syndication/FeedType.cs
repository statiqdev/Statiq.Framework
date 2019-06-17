using System;
using Statiq.Feeds.Syndication.Atom;
using Statiq.Feeds.Syndication.Rdf;
using Statiq.Feeds.Syndication.Rss;

namespace Statiq.Feeds.Syndication
{
    public class FeedType
    {
        public static readonly FeedType Rdf = new FeedType(source => new RdfFeed(source));
        public static readonly FeedType Rss = new FeedType(source => new RssFeed(source));
        public static readonly FeedType Atom = new FeedType(source => new AtomFeed(source));

        private readonly Func<IFeed, IFeed> _feedFactory;

        private FeedType(Func<IFeed, IFeed> feedFactory)
        {
            _feedFactory = feedFactory;
        }

        public IFeed GetFeed(IFeed source) => _feedFactory(source);
    }
}