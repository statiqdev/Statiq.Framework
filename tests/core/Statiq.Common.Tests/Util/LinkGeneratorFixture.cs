using System;
using NUnit.Framework;
using Statiq.Testing;

namespace Statiq.Common.Tests.Util
{
    [TestFixture]
    public class LinkGeneratorFixture : BaseFixture
    {
        public class GetLinkTests : LinkGeneratorFixture
        {
            [TestCase(".", "/")]
            [TestCase("/foo/bar/index.html", "/foo/bar/index.html")]
            [TestCase("/foo/bar/index.htm", "/foo/bar/index.htm")]
            [TestCase("/foo/bar/baz.html", "/foo/bar/baz.html")]
            [TestCase("/index.html", "/index.html")]
            [TestCase("index.html", "/index.html")]
            [TestCase("/foo.html", "/foo.html")]
            [TestCase("foo.html", "/foo.html")]
            [TestCase("C:/bar/foo.html", "/C:/bar/foo.html")]
            [TestCase("C:/bar/foo.html", "/C:/bar/foo.html")]
            [TestCase(null, "/")]
            public void ShouldReturnLinkForFilePath(string path, string expected)
            {
                // Given
                NormalizedPath filePath = path is null ? null : new NormalizedPath(path);

                // When
                string link = LinkGenerator.GetLink(filePath, null, null, null, null, null, false);

                // Then
                Assert.AreEqual(expected, link);
            }

            [TestCase(null, null, "/foo/bar/abc.html", "/foo/bar/abc.html")]
            [TestCase(null, null, "foo/bar/abc.html", "/foo/bar/abc.html")]
            [TestCase(null, "baz", "/foo/bar/abc.html", "/baz/foo/bar/abc.html")]
            [TestCase(null, "baz/", "/foo/bar/abc.html", "/baz/foo/bar/abc.html")]
            [TestCase("www.google.com", null, "/foo/bar/abc.html", "http://www.google.com/foo/bar/abc.html")]
            [TestCase("www.google.com", null, "/foo/bar/abc.html", "http://www.google.com/foo/bar/abc.html")]
            [TestCase("www.google.com", "/xyz", "/foo/bar/abc.html", "http://www.google.com/xyz/foo/bar/abc.html")]
            [TestCase("www.google.com", "/xyz/", "/foo/bar/abc.html", "http://www.google.com/xyz/foo/bar/abc.html")]
            [TestCase(null, "baz", null, "/baz")]
            [TestCase("www.google.com", null, null, "http://www.google.com/")]
            [TestCase("www.google.com", "/xyz", null, "http://www.google.com/xyz")]
            public void ShouldJoinHostAndRootForFilePath(string host, string root, string path, string expected)
            {
                // Given
                NormalizedPath filePath = path is null ? null : new NormalizedPath(path);

                // When
                string link = LinkGenerator.GetLink(filePath, host, root is null ? null : new NormalizedPath(root), null, null, null, false);

                // Then
                Assert.AreEqual(expected, link);
            }

            [TestCase("/foo/bar/index.html", "/foo/bar")]
            [TestCase("/index.html", "/")]
            [TestCase("index.html", "/")]
            [TestCase("/foo/bar/baz.html", "/foo/bar/baz.html")]
            public void ShouldHideIndexPagesForFilePath(string path, string expected)
            {
                // Given
                NormalizedPath filePath = new NormalizedPath(path);

                // When
                string link = LinkGenerator.GetLink(filePath, null, null, null, new[] { "index.html" }, null, false);

                // Then
                Assert.AreEqual(expected, link);
            }

            [TestCase("/foo/bar/abc.html", "/foo/bar/abc")]
            [TestCase("/foo/bar/abc.htm", "/foo/bar/abc")]
            [TestCase("/foo/bar/abc.xyz", "/foo/bar/abc")]
            [TestCase("/abc.html", "/abc")]
            [TestCase("/abc.htm", "/abc")]
            [TestCase("abc.html", "/abc")]
            [TestCase("abc.htm", "/abc")]
            [TestCase("/foo/bar/index.html", "/foo/bar/index")]
            [TestCase("/foo/bar/index.htm", "/foo/bar/index")]
            public void ShouldHideExtensionsForFilePath(string path, string expected)
            {
                // Given
                NormalizedPath filePath = new NormalizedPath(path);

                // When
                string link = LinkGenerator.GetLink(filePath, null, null, null, null, Array.Empty<string>(), false);

                // Then
                Assert.AreEqual(expected, link);
            }

            [TestCase("/foo/bar/abc.html", "/foo/bar/abc")]
            [TestCase("/foo/bar/abc.htm", "/foo/bar/abc")]
            [TestCase("/foo/bar/abc.xyz", "/foo/bar/abc.xyz")]
            public void ShouldHideSpecificExtensionsForFilePath(string path, string expected)
            {
                // Given
                NormalizedPath filePath = new NormalizedPath(path);

                // When
                string link = LinkGenerator.GetLink(filePath, null, null, null, null, new[] { "html", ".htm" }, false);

                // Then
                Assert.AreEqual(expected, link);
            }

            [TestCase(null, "/", ".", "/")]
            [TestCase(null, null, ".", "/")]
            [TestCase(null, null, null, "/")]
            [TestCase(null, "/", "foo/bar", "/foo/bar")]
            [TestCase(null, "/", "/foo/bar", "/foo/bar")]
            [TestCase(null, "/", "/foo/baz/../bar", "/foo/bar")]
            [TestCase(null, null, "/foo/bar", "/foo/bar")]
            [TestCase(null, "baz", "/foo/bar", "/baz/foo/bar")]
            [TestCase(null, "/baz/", "/foo/bar", "/baz/foo/bar")]
            [TestCase("www.google.com", null, "/foo/bar", "http://www.google.com/foo/bar")]
            [TestCase("www.google.com", null, "/foo/bar", "http://www.google.com/foo/bar")]
            [TestCase("www.google.com", "xyz", "/foo/bar", "http://www.google.com/xyz/foo/bar")]
            [TestCase("www.google.com", "/xyz/", "/foo/bar", "http://www.google.com/xyz/foo/bar")]
            [TestCase("www.google.com", null, null, "http://www.google.com/")]
            [TestCase("www.google.com", "xyz", null, "http://www.google.com/xyz")]
            public void ShouldJoinHostAndRootForDirectoryPath(string host, string root, string path, string expected)
            {
                // Given
                NormalizedPath directoryPath = path is null ? null : new NormalizedPath(path);

                // When
                string link = LinkGenerator.GetLink(directoryPath, host, root is null ? null : new NormalizedPath(root), null, null, null, false);

                // Then
                Assert.AreEqual(expected, link);
            }

            [Test]
            public void ShouldUseSpecifiedScheme()
            {
                // Given
                NormalizedPath directoryPath = new NormalizedPath("/foo/bar");

                // When
                string link = LinkGenerator.GetLink(directoryPath, "www.google.com", null, "https", null, null, false);

                // Then
                Assert.AreEqual("https://www.google.com/foo/bar", link);
            }

            [Test]
            public void SupportsSingleSlash()
            {
                // Given
                NormalizedPath path = new NormalizedPath("/");

                // When
                string link = LinkGenerator.GetLink(path, null, null, null, null, null, false);

                // Then
                Assert.AreEqual("/", link);
            }

            [Test]
            public void SupportsSingleSlashWithRoot()
            {
                // Given
                NormalizedPath path = new NormalizedPath("/");

                // When
                string link = LinkGenerator.GetLink(path, null, "root", null, null, null, false);

                // Then
                Assert.AreEqual("/root/", link);
            }

            [Test]
            public void SupportsSingleSlashWithHidePages()
            {
                // Given
                NormalizedPath path = new NormalizedPath("/");

                // When
                string link = LinkGenerator.GetLink(path, null, null, null, new[] { "index" }, null, false);

                // Then
                Assert.AreEqual("/", link);
            }

            [Test]
            public void SupportsSingleSlashWithHideExtensions()
            {
                // Given
                NormalizedPath path = new NormalizedPath("/");

                // When
                string link = LinkGenerator.GetLink(path, null, null, null, null, new[] { "html" }, false);

                // Then
                Assert.AreEqual("/", link);
            }

            [Test]
            public void ShouldGenerateMixedCaseLinks()
            {
                // Given
                NormalizedPath directoryPath = new NormalizedPath("/Foo/Bar");

                // When
                string link = LinkGenerator.GetLink(directoryPath, "www.google.com", null, "http", null, null, false);

                // Then
                Assert.AreEqual("http://www.google.com/Foo/Bar", link);
            }

            [Test]
            public void ShouldGenerateLowercaseLinks()
            {
                // Given
                NormalizedPath directoryPath = new NormalizedPath("/Foo/Bar");

                // When
                string link = LinkGenerator.GetLink(directoryPath, "www.google.com", null, "http", null, null, true);

                // Then
                Assert.AreEqual("http://www.google.com/foo/bar", link);
            }
        }
    }
}
