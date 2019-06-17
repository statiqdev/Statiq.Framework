using NUnit.Framework;
using Shouldly;
using Statiq.Common.Execution;
using Statiq.Common.IO;
using Statiq.Common.Meta;
using Statiq.Testing;
using Statiq.Testing.Attributes;
using Statiq.Testing.Documents;
using Statiq.Testing.Execution;
using Statiq.Testing.Meta;

namespace Statiq.Common.Tests.Execution
{
    [TestFixture]
    public class IExecutionContextLinkExtensionsFixture : BaseFixture
    {
        public class GetLinkTests : IExecutionContextLinkExtensionsFixture
        {
            [TestCase("http://foo.com/bar", false, "http://foo.com/bar")]
            [TestCase("http://foo.com/bar", true, "http://foo.com/bar")]
            [TestCase("https://foo.com/bar", false, "https://foo.com/bar")]
            [TestCase("https://foo.com/bar", true, "https://foo.com/bar")]
            [TestCase("foo/bar", false, "/foo/bar")]
            [TestCase("foo/bar", true, "http://domain.com/foo/bar")]
            [TestCase("/foo/bar", false, "/foo/bar")]
            [TestCase("/foo/bar", true, "http://domain.com/foo/bar")]
            [TestCase("//foo/bar", false, "/foo/bar")]
            [TestCase("//foo/bar", true, "http://domain.com/foo/bar")]
            public void UsesAbsoluteLinkIfProvided(string value, bool includeHost, string expected)
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.Host] = "domain.com";
                TestDocument document = new TestDocument
                {
                    { "Path", value }
                };

                // When
                string result = context.GetLink(document, "Path", includeHost);

                // Then
                result.ShouldBe(expected);
            }

            [TestCase("foo/bar.txt", false, "/foo/bar.txt")]
            [TestCase("foo/bar.txt", true, "http://domain.com/foo/bar.txt")]
            [TestCase("/foo/bar.txt", false, "/foo/bar.txt")]
            [TestCase("/foo/bar.txt", true, "http://domain.com/foo/bar.txt")]
            [TestCase("//foo/bar.txt", false, "/foo/bar.txt")]
            [TestCase("//foo/bar.txt", true, "http://domain.com/foo/bar.txt")]
            [WindowsTestCase("C:/foo/bar.txt", false, "/C:/foo/bar.txt")]
            [WindowsTestCase("C:/foo/bar.txt", true, "http://domain.com/C:/foo/bar.txt")]
            public void GetsLinkFromDestination(string destination, bool includeHost, string expected)
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.Host] = "domain.com";
                TestDocument document = new TestDocument(null, new FilePath(destination));

                // When
                string result = context.GetLink(document, includeHost);

                // Then
                result.ShouldBe(expected);
            }
        }
    }
}
