using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Shortcodes.Html
{
    [TestFixture]
    public class LinkShortcodeFixture : BaseFixture
    {
        public class ExecuteTests : LinkShortcodeFixture
        {
            [TestCase("http://foo.com/bar", "http://foo.com/bar")]
            [TestCase("https://foo.com/bar", "https://foo.com/bar")]
            [TestCase("foo/bar", "/foo/bar")]
            [TestCase("/foo/bar", "/foo/bar")]
            [TestCase("//foo/bar", "/foo/bar")]
            public async Task RendersLink(string path, string expected)
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Path", path)
                };
                LinkShortcode shortcode = new LinkShortcode();

                // When
                TestDocument result = (TestDocument)await shortcode.ExecuteAsync(args, string.Empty, document, context);

                // Then
                result.Content.ShouldBe(expected);
            }

            [TestCase("http://foo.com/bar", "http://foo.com/bar")]
            [TestCase("https://foo.com/bar", "https://foo.com/bar")]
            [TestCase("foo/bar", "http://domain.com/foo/bar")]
            [TestCase("/foo/bar", "http://domain.com/foo/bar")]
            [TestCase("//foo/bar", "http://domain.com/foo/bar")]
            public async Task RendersLinkWithHost(string path, string expected)
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.Host] = "domain.com";
                TestDocument document = new TestDocument();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Path", path),
                    new KeyValuePair<string, string>("IncludeHost", "true")
                };
                LinkShortcode shortcode = new LinkShortcode();

                // When
                TestDocument result = (TestDocument)await shortcode.ExecuteAsync(args, string.Empty, document, context);

                // Then
                result.Content.ShouldBe(expected);
            }

            [TestCase("http://foo.com/bar", "http://foo.com/bar")]
            [TestCase("https://foo.com/bar", "https://foo.com/bar")]
            [TestCase("foo/bar", "http://google.com/foo/bar")]
            [TestCase("/foo/bar", "http://google.com/foo/bar")]
            [TestCase("//foo/bar", "http://google.com/foo/bar")]
            public async Task RendersLinkWithAlternateHost(string path, string expected)
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.Host] = "domain.com";
                TestDocument document = new TestDocument();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Path", path),
                    new KeyValuePair<string, string>("Host", "google.com")
                };
                LinkShortcode shortcode = new LinkShortcode();

                // When
                TestDocument result = (TestDocument)await shortcode.ExecuteAsync(args, string.Empty, document, context);

                // Then
                result.Content.ShouldBe(expected);
            }
        }
    }
}
