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
            // makeAbsolute = true
            [TestCase(".", true, "/")]
            [TestCase("/foo/bar/index.html", true, "/foo/bar/index.html")]
            [TestCase("/foo/bar/index.htm", true, "/foo/bar/index.htm")]
            [TestCase("/foo/bar/baz.html", true, "/foo/bar/baz.html")]
            [TestCase("/index.html", true, "/index.html")]
            [TestCase("index.html", true, "/index.html")]
            [TestCase("/foo.html", true, "/foo.html")]
            [TestCase("foo.html", true, "/foo.html")]
            [TestCase("C:/bar/foo.html", true, "/C:/bar/foo.html")]
            [TestCase("C:/bar/foo.html", true, "/C:/bar/foo.html")]
            [TestCase(null, true, "/")]

            // makeAbsolute = false
            [TestCase(".", false, ".")]
            [TestCase("/foo/bar/index.html", false, "/foo/bar/index.html")]
            [TestCase("/foo/bar/index.htm", false, "/foo/bar/index.htm")]
            [TestCase("/foo/bar/baz.html", false, "/foo/bar/baz.html")]
            [TestCase("/index.html", false, "/index.html")]
            [TestCase("index.html", false, "index.html")]
            [TestCase("/foo.html", false, "/foo.html")]
            [TestCase("foo.html", false, "foo.html")]
            [TestCase("C:/bar/foo.html", false, "C:/bar/foo.html")]
            [TestCase("C:/bar/foo.html", false, "C:/bar/foo.html")]
            [TestCase(null, false, "")]
            public void ShouldReturnLinkForFilePath(string path, bool makeAbsolute, string expected)
            {
                // Given
                NormalizedPath filePath = path is null ? null : new NormalizedPath(path);

                // When
                string link = LinkGenerator.GetLink(filePath, null, null, null, null, null, false, makeAbsolute);

                // Then
                Assert.AreEqual(expected, link);
            }

            // makeAbsolute = true
            [TestCase(null, null, "/foo/bar/abc.html", true, "/foo/bar/abc.html")]
            [TestCase(null, null, "foo/bar/abc.html", true, "/foo/bar/abc.html")]
            [TestCase(null, "baz", "/foo/bar/abc.html", true, "/baz/foo/bar/abc.html")]
            [TestCase(null, "baz/", "/foo/bar/abc.html", true, "/baz/foo/bar/abc.html")]
            [TestCase(null, "baz", "foo/bar/abc.html", true, "/baz/foo/bar/abc.html")]
            [TestCase(null, "baz/", "foo/bar/abc.html", true, "/baz/foo/bar/abc.html")]
            [TestCase("www.google.com", null, "/foo/bar/abc.html", true, "http://www.google.com/foo/bar/abc.html")]
            [TestCase("www.google.com", null, "/foo/bar/abc.html", true, "http://www.google.com/foo/bar/abc.html")]
            [TestCase("www.google.com", "/xyz", "/foo/bar/abc.html", true, "http://www.google.com/xyz/foo/bar/abc.html")]
            [TestCase("www.google.com", "/xyz/", "/foo/bar/abc.html", true, "http://www.google.com/xyz/foo/bar/abc.html")]
            [TestCase("www.google.com", null, "foo/bar/abc.html", true, "http://www.google.com/foo/bar/abc.html")]
            [TestCase("www.google.com", null, "foo/bar/abc.html", true, "http://www.google.com/foo/bar/abc.html")]
            [TestCase("www.google.com", "/xyz", "foo/bar/abc.html", true, "http://www.google.com/xyz/foo/bar/abc.html")]
            [TestCase("www.google.com", "/xyz/", "foo/bar/abc.html", true, "http://www.google.com/xyz/foo/bar/abc.html")]
            [TestCase(null, "baz", null, true, "/baz")]
            [TestCase("www.google.com", null, null, true, "http://www.google.com/")]
            [TestCase("www.google.com", "/xyz", null, true, "http://www.google.com/xyz")]

            // makeAbsolute = false
            [TestCase(null, null, "/foo/bar/abc.html", false, "/foo/bar/abc.html")]
            [TestCase(null, null, "foo/bar/abc.html", false, "foo/bar/abc.html")]
            [TestCase(null, "baz", "/foo/bar/abc.html", false, "/baz/foo/bar/abc.html")]
            [TestCase(null, "baz/", "/foo/bar/abc.html", false, "/baz/foo/bar/abc.html")]
            [TestCase(null, "baz", "foo/bar/abc.html", false, "foo/bar/abc.html")]
            [TestCase(null, "baz/", "foo/bar/abc.html", false, "foo/bar/abc.html")]
            [TestCase("www.google.com", null, "/foo/bar/abc.html", false, "http://www.google.com/foo/bar/abc.html")]
            [TestCase("www.google.com", null, "/foo/bar/abc.html", false, "http://www.google.com/foo/bar/abc.html")]
            [TestCase("www.google.com", "/xyz", "/foo/bar/abc.html", false, "http://www.google.com/xyz/foo/bar/abc.html")]
            [TestCase("www.google.com", "/xyz/", "/foo/bar/abc.html", false, "http://www.google.com/xyz/foo/bar/abc.html")]
            [TestCase("www.google.com", null, "foo/bar/abc.html", false, "foo/bar/abc.html")]
            [TestCase("www.google.com", null, "foo/bar/abc.html", false, "foo/bar/abc.html")]
            [TestCase("www.google.com", "/xyz", "foo/bar/abc.html", false, "foo/bar/abc.html")]
            [TestCase("www.google.com", "/xyz/", "foo/bar/abc.html", false, "foo/bar/abc.html")]
            [TestCase(null, "baz", null, false, "")]
            [TestCase("www.google.com", null, null, false, "")]
            [TestCase("www.google.com", "/xyz", null, false, "")]
            public void ShouldJoinHostAndRootForFilePath(string host, string root, string path, bool makeAbsolute, string expected)
            {
                // Given
                NormalizedPath filePath = path is null ? null : new NormalizedPath(path);

                // When
                string link = LinkGenerator.GetLink(filePath, host, root is null ? null : new NormalizedPath(root), null, null, null, false, makeAbsolute);

                // Then
                Assert.AreEqual(expected, link);
            }

            // makeAbsolute = true
            [TestCase("/foo/bar/index.html", true, "/foo/bar")]
            [TestCase("foo/bar/index.html", true, "/foo/bar")]
            [TestCase("/index.html", true, "/")]
            [TestCase("index.html", true, "/")]
            [TestCase("/foo/bar/baz.html", true, "/foo/bar/baz.html")]
            [TestCase("foo/bar/baz.html", true, "/foo/bar/baz.html")]

            // makeAbsolute = false
            [TestCase("/foo/bar/index.html", false, "/foo/bar")]
            [TestCase("foo/bar/index.html", false, "foo/bar")]
            [TestCase("/index.html", false, "/")]
            [TestCase("index.html", false, ".")] // special case when we remove the index page of a peer relative path
            [TestCase("/foo/bar/baz.html", false, "/foo/bar/baz.html")]
            [TestCase("foo/bar/baz.html", false, "foo/bar/baz.html")]
            public void ShouldHideIndexPagesForFilePath(string path, bool makeAbsolute, string expected)
            {
                // Given
                NormalizedPath filePath = new NormalizedPath(path);

                // When
                string link = LinkGenerator.GetLink(filePath, null, null, null, new[] { "index.html" }, null, false, makeAbsolute);

                // Then
                Assert.AreEqual(expected, link);
            }

            // makeAbsolute = true
            [TestCase("/foo/bar/abc.html", true, "/foo/bar/abc")]
            [TestCase("foo/bar/abc.html", true, "/foo/bar/abc")]
            [TestCase("/foo/bar/abc.htm", true, "/foo/bar/abc")]
            [TestCase("foo/bar/abc.htm", true, "/foo/bar/abc")]
            [TestCase("/foo/bar/abc.xyz", true, "/foo/bar/abc")]
            [TestCase("foo/bar/abc.xyz", true, "/foo/bar/abc")]
            [TestCase("/abc.html", true, "/abc")]
            [TestCase("abc.html", true, "/abc")]
            [TestCase("/abc.htm", true, "/abc")]
            [TestCase("abc.htm", true, "/abc")]
            [TestCase("/foo/bar/index.html", true, "/foo/bar/index")]
            [TestCase("foo/bar/index.html", true, "/foo/bar/index")]
            [TestCase("/foo/bar/index.htm", true, "/foo/bar/index")]
            [TestCase("foo/bar/index.htm", true, "/foo/bar/index")]

            // makeAbsolute = false
            [TestCase("/foo/bar/abc.html", false, "/foo/bar/abc")]
            [TestCase("foo/bar/abc.html", false, "foo/bar/abc")]
            [TestCase("/foo/bar/abc.htm", false, "/foo/bar/abc")]
            [TestCase("foo/bar/abc.htm", false, "foo/bar/abc")]
            [TestCase("/foo/bar/abc.xyz", false, "/foo/bar/abc")]
            [TestCase("foo/bar/abc.xyz", false, "foo/bar/abc")]
            [TestCase("/abc.html", false, "/abc")]
            [TestCase("abc.html", false, "abc")]
            [TestCase("/abc.htm", false, "/abc")]
            [TestCase("abc.htm", false, "abc")]
            [TestCase("/foo/bar/index.html", false, "/foo/bar/index")]
            [TestCase("foo/bar/index.html", false, "foo/bar/index")]
            [TestCase("/foo/bar/index.htm", false, "/foo/bar/index")]
            [TestCase("foo/bar/index.htm", false, "foo/bar/index")]
            public void ShouldHideExtensionsForFilePath(string path, bool makeAbsolute, string expected)
            {
                // Given
                NormalizedPath filePath = new NormalizedPath(path);

                // When
                string link = LinkGenerator.GetLink(filePath, null, null, null, null, Array.Empty<string>(), false, makeAbsolute);

                // Then
                Assert.AreEqual(expected, link);
            }

            // makeAbsolute = true
            [TestCase("/foo/bar/abc.html", true, "/foo/bar/abc")]
            [TestCase("foo/bar/abc.html", true, "/foo/bar/abc")]
            [TestCase("/foo/bar/abc.htm", true, "/foo/bar/abc")]
            [TestCase("foo/bar/abc.htm", true, "/foo/bar/abc")]
            [TestCase("/foo/bar/abc.xyz", true, "/foo/bar/abc.xyz")]
            [TestCase("foo/bar/abc.xyz", true, "/foo/bar/abc.xyz")]

            // makeAbsolute = false
            [TestCase("/foo/bar/abc.html", false, "/foo/bar/abc")]
            [TestCase("foo/bar/abc.html", false, "foo/bar/abc")]
            [TestCase("/foo/bar/abc.htm", false, "/foo/bar/abc")]
            [TestCase("foo/bar/abc.htm", false, "foo/bar/abc")]
            [TestCase("/foo/bar/abc.xyz", false, "/foo/bar/abc.xyz")]
            [TestCase("foo/bar/abc.xyz", false, "foo/bar/abc.xyz")]
            public void ShouldHideSpecificExtensionsForFilePath(string path, bool makeAbsolute, string expected)
            {
                // Given
                NormalizedPath filePath = new NormalizedPath(path);

                // When
                string link = LinkGenerator.GetLink(filePath, null, null, null, null, new[] { "html", ".htm" }, false, makeAbsolute);

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
