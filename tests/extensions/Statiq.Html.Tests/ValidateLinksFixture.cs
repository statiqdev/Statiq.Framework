using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Parser.Html;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Html.Tests
{
    [TestFixture]
    public class ValidateLinksFixture : BaseFixture
    {
        public class ExecuteTests : ValidateLinksFixture
        {
            [Test]
            public async Task DoesNotThrowForInvalidLink()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.TestLoggerProvider.ThrowLogLevel = LogLevel.None;
                TestDocument document = new TestDocument("<html><head></head><body><a href=\"http://example.com<>\">foo</a></body></html>");
                ValidateLinks module = new ValidateLinks();

                // When
                await ExecuteAsync(document, context, module);

                // Then
                context.LogMessages.ShouldHaveSingleItem().LogLevel.ShouldBe(LogLevel.Warning);
            }

            [TestCase("../target.html")]
            [TestCase("../target.html?query=parameter")]
            [TestCase("../target.html#fragment")]
            [TestCase("../target.html?query=parameter#fragment")]
            public async Task CanValidateRelativeLink(string link)
            {
                // Given
                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/output");
                fileProvider.AddFile("/output/target.html", "<html><head></head><body><h1>Hello World!</h1></body></html>");
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                TestExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                TestDocument target = new TestDocument(
                    new NormalizedPath("/target.html"),
                    new NormalizedPath("target.html"),
                    "<html><head></head><body><h1>Hello World!</h1></body></html>");
                TestDocument document = new TestDocument(
                    new NormalizedPath("/test/document.html"),
                    new NormalizedPath("test/document.html"),
                    $"<html><head></head><body><a href=\"{link}\">foo</a></body></html>");
                ValidateLinks module = new ValidateLinks();

                // When
                await ExecuteAsync(new[] { target, document }, context, module);

                // Then
                context.LogMessages.ShouldNotContain(x => x.LogLevel == LogLevel.Warning);
            }
        }

        public class GatherLinksTests : ValidateLinksFixture
        {
            [TestCase("<a href=\"/foo/bar\">baz</a>", "/foo/bar")]
            [TestCase("<a href=\"/foo/bar.html\">baz</a>", "/foo/bar.html")]
            [TestCase("<a href=\"http://foo.com/bar\">baz</a>", "http://foo.com/bar")]
            [TestCase("<a href=\"http://foo.com/bar.html\">baz</a>", "http://foo.com/bar.html")]
            [TestCase("<img src=\"/foo/bar.png\"></img>", "/foo/bar.png")]
            [TestCase("<img src=\"http://foo/bar.png\"></img>", "http://foo/bar.png")]
            [TestCase("<script src=\"/foo/bar.js\"></script>", "/foo/bar.js")]
            [TestCase("<script src=\"http://foo.com/bar.js\"></script>", "http://foo.com/bar.js")]
            public async Task FindsLinksInBody(string tag, string link)
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument($"<html><head></head><body>{tag}</body></html>");
                HtmlParser parser = new HtmlParser();
                ConcurrentDictionary<string, ConcurrentBag<(IDocument source, string outerHtml)>> links =
                    new ConcurrentDictionary<string, ConcurrentBag<(IDocument source, string outerHtml)>>();

                // When
                await ValidateLinks.GatherLinksAsync(document, context, parser, links);

                // Then
                Assert.That(links.Count, Is.EqualTo(1));
                Assert.That(links.First().Key, Is.EqualTo(link));
            }

            [TestCase("<link href=\"/foo/bar.css\" rel=\"stylesheet\" />", "/foo/bar.css")]
            [TestCase("<link href=\"http://foo.com/bar.css\" rel=\"stylesheet\" />", "http://foo.com/bar.css")]
            [TestCase("<link rel=\"icon\" href=\"/foo/favicon.ico\" type=\"image/x-icon\">", "/foo/favicon.ico")]
            [TestCase("<script src=\"/foo/bar.js\"></script>", "/foo/bar.js")]
            [TestCase("<script src=\"http://foo.com/bar.js\"></script>", "http://foo.com/bar.js")]
            public async Task FindsLinksInHead(string tag, string link)
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument($"<html><head>{tag}</head><body></body></html>");
                HtmlParser parser = new HtmlParser();
                ConcurrentDictionary<string, ConcurrentBag<(IDocument source, string outerHtml)>> links =
                    new ConcurrentDictionary<string, ConcurrentBag<(IDocument source, string outerHtml)>>();

                // When
                await ValidateLinks.GatherLinksAsync(document, context, parser, links);

                // Then
                Assert.That(links.Count, Is.EqualTo(1));
                Assert.That(links.First().Key, Is.EqualTo(link));
            }
        }
    }
}
