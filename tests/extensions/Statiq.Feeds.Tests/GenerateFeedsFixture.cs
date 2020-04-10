using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Feeds.Tests
{
    [TestFixture]
    public class GenerateFeedsFixture : BaseFixture
    {
        public class ExecuteTests : GenerateFeedsFixture
        {
            [Test]
            public async Task DoesNotChangeImageDomain()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.Host] = "buzz.com";
                TestDocument document = new TestDocument(
                    new NormalizedPath("/input/fizz/buzz"),
                    new NormalizedPath("fizz/buzz"),
                    new Dictionary<string, object>
                    {
                        { FeedKeys.Image, new Uri("http://foo.com/bar/baz.png") }
                    });
                GenerateFeeds module = new GenerateFeeds();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, context, module);

                // Then
                results.Select(x => x.Destination.FullPath).ShouldBe(new[] { "feed.rss", "feed.atom" }, true);
                results[0].Content.ShouldContain("http://foo.com/bar/baz.png");
            }

            [Test]
            public async Task OrdersByPublish()
            {
                // Given
                TestDocument a = new TestDocument(new NormalizedPath("a.txt"))
                {
                    { FeedKeys.Title, "A" },
                    { FeedKeys.Published, new DateTime(2010, 1, 1) }
                };
                TestDocument b = new TestDocument(new NormalizedPath("b.txt"))
                {
                    { FeedKeys.Title, "B" },
                    { FeedKeys.Published, new DateTime(2010, 2, 1) }
                };
                GenerateFeeds module = new GenerateFeeds();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a, b }, module);

                // Then
                TestDocument result = results.Single(x => x.Destination.FullPath == "feed.rss");
                result.Content.IndexOf("<title>B</title>").ShouldBeLessThan(result.Content.IndexOf("<title>A</title>"));
            }

            [Test]
            public async Task PreservesInputOrder()
            {
                // Given
                TestDocument a = new TestDocument(new NormalizedPath("a.txt"))
                {
                    { FeedKeys.Title, "A" },
                    { FeedKeys.Published, new DateTime(2010, 1, 1) }
                };
                TestDocument b = new TestDocument(new NormalizedPath("b.txt"))
                {
                    { FeedKeys.Title, "B" },
                    { FeedKeys.Published, new DateTime(2010, 2, 1) }
                };
                GenerateFeeds module = new GenerateFeeds().PreserveOrdering();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a, b }, module);

                // Then
                TestDocument result = results.Single(x => x.Destination.FullPath == "feed.rss");
                result.Content.IndexOf("<title>A</title>").ShouldBeLessThan(result.Content.IndexOf("<title>B</title>"));
            }
        }
    }
}
