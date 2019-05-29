using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Content;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Feeds.Tests
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
                    new FilePath("/input/fizz/buzz"),
                    new FilePath("fizz/buzz"),
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
        }
    }
}
