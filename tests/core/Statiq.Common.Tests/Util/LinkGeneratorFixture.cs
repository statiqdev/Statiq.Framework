using System;
using NUnit.Framework;
using Shouldly;
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
                LinkGenerator linkGenerator = new LinkGenerator();

                // When
                string link = linkGenerator.GetLink(filePath, null, null, null, null, null, false, makeAbsolute);

                // Then
                link.ShouldBe(expected);
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
                LinkGenerator linkGenerator = new LinkGenerator();

                // When
                string link = linkGenerator.GetLink(filePath, host, root is null ? null : new NormalizedPath(root), null, null, null, false, makeAbsolute);

                // Then
                link.ShouldBe(expected);
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
                LinkGenerator linkGenerator = new LinkGenerator();

                // When
                string link = linkGenerator.GetLink(filePath, null, null, null, new[] { "index.html" }, null, false, makeAbsolute);

                // Then
                link.ShouldBe(expected);
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
                LinkGenerator linkGenerator = new LinkGenerator();

                // When
                string link = linkGenerator.GetLink(filePath, null, null, null, null, Array.Empty<string>(), false, makeAbsolute);

                // Then
                link.ShouldBe(expected);
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
                LinkGenerator linkGenerator = new LinkGenerator();

                // When
                string link = linkGenerator.GetLink(filePath, null, null, null, null, new[] { "html", ".htm" }, false, makeAbsolute);

                // Then
                link.ShouldBe(expected);
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
                LinkGenerator linkGenerator = new LinkGenerator();

                // When
                string link = linkGenerator.GetLink(directoryPath, host, root is null ? null : new NormalizedPath(root), null, null, null, false);

                // Then
                link.ShouldBe(expected);
            }

            [Test]
            public void ShouldUseSpecifiedScheme()
            {
                // Given
                NormalizedPath directoryPath = new NormalizedPath("/foo/bar");
                LinkGenerator linkGenerator = new LinkGenerator();

                // When
                string link = linkGenerator.GetLink(directoryPath, "www.google.com", null, "https", null, null, false);

                // Then
                link.ShouldBe("https://www.google.com/foo/bar");
            }

            [Test]
            public void SupportsSingleSlash()
            {
                // Given
                NormalizedPath path = new NormalizedPath("/");
                LinkGenerator linkGenerator = new LinkGenerator();

                // When
                string link = linkGenerator.GetLink(path, null, null, null, null, null, false);

                // Then
                link.ShouldBe("/");
            }

            [Test]
            public void SupportsSingleSlashWithRoot()
            {
                // Given
                NormalizedPath path = new NormalizedPath("/");
                LinkGenerator linkGenerator = new LinkGenerator();

                // When
                string link = linkGenerator.GetLink(path, null, "root", null, null, null, false);

                // Then
                link.ShouldBe("/root/");
            }

            [Test]
            public void SupportsSingleSlashWithHidePages()
            {
                // Given
                NormalizedPath path = new NormalizedPath("/");
                LinkGenerator linkGenerator = new LinkGenerator();

                // When
                string link = linkGenerator.GetLink(path, null, null, null, new[] { "index" }, null, false);

                // Then
                link.ShouldBe("/");
            }

            [Test]
            public void SupportsSingleSlashWithHideExtensions()
            {
                // Given
                NormalizedPath path = new NormalizedPath("/");
                LinkGenerator linkGenerator = new LinkGenerator();

                // When
                string link = linkGenerator.GetLink(path, null, null, null, null, new[] { "html" }, false);

                // Then
                link.ShouldBe("/");
            }

            [Test]
            public void ShouldGenerateMixedCaseLinks()
            {
                // Given
                NormalizedPath directoryPath = new NormalizedPath("/Foo/Bar");
                LinkGenerator linkGenerator = new LinkGenerator();

                // When
                string link = linkGenerator.GetLink(directoryPath, "www.google.com", null, "http", null, null, false);

                // Then
                link.ShouldBe("http://www.google.com/Foo/Bar");
            }

            [Test]
            public void ShouldGenerateLowercaseLinks()
            {
                // Given
                NormalizedPath directoryPath = new NormalizedPath("/Foo/Bar");
                LinkGenerator linkGenerator = new LinkGenerator();

                // When
                string link = linkGenerator.GetLink(directoryPath, "www.google.com", null, "http", null, null, true);

                // Then
                link.ShouldBe("http://www.google.com/foo/bar");
            }

            [Test]
            public void EscapesSpecialCharacters()
            {
                // Given
                NormalizedPath directoryPath = new NormalizedPath("/a/b/c%d");
                LinkGenerator linkGenerator = new LinkGenerator();

                // When
                string link = linkGenerator.GetLink(directoryPath, "www.google.com", null, "http", null, null, false, true);

                // Then
                link.ShouldBe("http://www.google.com/a/b/c%25d");
            }

            [Test]
            public void EscapesSpecialCharactersForRealtivePath()
            {
                // Given
                NormalizedPath directoryPath = new NormalizedPath("a/b/c%d");
                LinkGenerator linkGenerator = new LinkGenerator();

                // When
                string link = linkGenerator.GetLink(directoryPath, null, null, null, null, null, false, false);

                // Then
                link.ShouldBe("a/b/c%25d");
            }

            [Test]
            public void ConvertsSlashes()
            {
                // Given
                NormalizedPath directoryPath = new NormalizedPath("/a/b/c\\d");
                LinkGenerator linkGenerator = new LinkGenerator();

                // When
                string link = linkGenerator.GetLink(directoryPath, "www.google.com", null, "http", null, null, true);

                // Then
                link.ShouldBe("http://www.google.com/a/b/c/d");
            }

            // makeAbsolute = true
            [TestCase(".#fizz", true, "/#fizz")]
            [TestCase("/foo/bar/index.html#fizz", true, "/foo/bar/index.html#fizz")]
            [TestCase("/foo/bar/index.htm#fizz", true, "/foo/bar/index.htm#fizz")]
            [TestCase("/foo/bar/baz.html#fizz", true, "/foo/bar/baz.html#fizz")]
            [TestCase("/index.html#fizz", true, "/index.html#fizz")]
            [TestCase("index.html#fizz", true, "/index.html#fizz")]
            [TestCase("/foo.html#fizz", true, "/foo.html#fizz")]
            [TestCase("foo.html#fizz", true, "/foo.html#fizz")]
            [TestCase("C:/bar/foo.html#fizz", true, "/C:/bar/foo.html#fizz")]
            [TestCase("C:/bar/foo.html#fizz", true, "/C:/bar/foo.html#fizz")]

            // makeAbsolute = false
            [TestCase(".#fizz", false, ".#fizz")]
            [TestCase("/foo/bar/index.html#fizz", false, "/foo/bar/index.html#fizz")]
            [TestCase("/foo/bar/index.htm#fizz", false, "/foo/bar/index.htm#fizz")]
            [TestCase("/foo/bar/baz.html#fizz", false, "/foo/bar/baz.html#fizz")]
            [TestCase("/index.html#fizz", false, "/index.html#fizz")]
            [TestCase("index.html#fizz", false, "index.html#fizz")]
            [TestCase("/foo.html#fizz", false, "/foo.html#fizz")]
            [TestCase("foo.html#fizz", false, "foo.html#fizz")]
            [TestCase("C:/bar/foo.html#fizz", false, "C:/bar/foo.html#fizz")]
            [TestCase("C:/bar/foo.html#fizz", false, "C:/bar/foo.html#fizz")]
            public void FragmentForFilePath(string path, bool makeAbsolute, string expected)
            {
                // Given
                NormalizedPath filePath = path is null ? null : new NormalizedPath(path);
                LinkGenerator linkGenerator = new LinkGenerator();

                // When
                string link = linkGenerator.GetLink(filePath, null, null, null, null, null, false, makeAbsolute);

                // Then
                link.ShouldBe(expected);
            }

            // makeAbsolute = true
            [TestCase(".#", true, "/#")]
            [TestCase("/foo/bar/index.html#", true, "/foo/bar/index.html#")]
            [TestCase("/foo/bar/index.htm#", true, "/foo/bar/index.htm#")]
            [TestCase("/foo/bar/baz.html#", true, "/foo/bar/baz.html#")]
            [TestCase("/index.html#", true, "/index.html#")]
            [TestCase("index.html#", true, "/index.html#")]
            [TestCase("/foo.html#", true, "/foo.html#")]
            [TestCase("foo.html#", true, "/foo.html#")]
            [TestCase("C:/bar/foo.html#", true, "/C:/bar/foo.html#")]
            [TestCase("C:/bar/foo.html#", true, "/C:/bar/foo.html#")]

            // makeAbsolute = false
            [TestCase(".#", false, ".#")]
            [TestCase("/foo/bar/index.html#", false, "/foo/bar/index.html#")]
            [TestCase("/foo/bar/index.htm#", false, "/foo/bar/index.htm#")]
            [TestCase("/foo/bar/baz.html#", false, "/foo/bar/baz.html#")]
            [TestCase("/index.html#", false, "/index.html#")]
            [TestCase("index.html#", false, "index.html#")]
            [TestCase("/foo.html#", false, "/foo.html#")]
            [TestCase("foo.html#", false, "foo.html#")]
            [TestCase("C:/bar/foo.html#", false, "C:/bar/foo.html#")]
            [TestCase("C:/bar/foo.html#", false, "C:/bar/foo.html#")]
            public void EmptyFragment(string path, bool makeAbsolute, string expected)
            {
                // Given
                NormalizedPath filePath = path is null ? null : new NormalizedPath(path);
                LinkGenerator linkGenerator = new LinkGenerator();

                // When
                string link = linkGenerator.GetLink(filePath, null, null, null, null, null, false, makeAbsolute);

                // Then
                link.ShouldBe(expected);
            }

            // makeAbsolute = true
            [TestCase(null, null, "/foo/bar/abc.html#fizz", true, "/foo/bar/abc.html#fizz")]
            [TestCase(null, null, "foo/bar/abc.html#fizz", true, "/foo/bar/abc.html#fizz")]
            [TestCase(null, "baz", "/foo/bar/abc.html#fizz", true, "/baz/foo/bar/abc.html#fizz")]
            [TestCase(null, "baz/", "/foo/bar/abc.html#fizz", true, "/baz/foo/bar/abc.html#fizz")]
            [TestCase(null, "baz", "foo/bar/abc.html#fizz", true, "/baz/foo/bar/abc.html#fizz")]
            [TestCase(null, "baz/", "foo/bar/abc.html#fizz", true, "/baz/foo/bar/abc.html#fizz")]
            [TestCase("www.google.com", null, "/foo/bar/abc.html#fizz", true, "http://www.google.com/foo/bar/abc.html#fizz")]
            [TestCase("www.google.com", null, "/foo/bar/abc.html#fizz", true, "http://www.google.com/foo/bar/abc.html#fizz")]
            [TestCase("www.google.com", "/xyz", "/foo/bar/abc.html#fizz", true, "http://www.google.com/xyz/foo/bar/abc.html#fizz")]
            [TestCase("www.google.com", "/xyz/", "/foo/bar/abc.html#fizz", true, "http://www.google.com/xyz/foo/bar/abc.html#fizz")]
            [TestCase("www.google.com", null, "foo/bar/abc.html#fizz", true, "http://www.google.com/foo/bar/abc.html#fizz")]
            [TestCase("www.google.com", null, "foo/bar/abc.html#fizz", true, "http://www.google.com/foo/bar/abc.html#fizz")]
            [TestCase("www.google.com", "/xyz", "foo/bar/abc.html#fizz", true, "http://www.google.com/xyz/foo/bar/abc.html#fizz")]
            [TestCase("www.google.com", "/xyz/", "foo/bar/abc.html#fizz", true, "http://www.google.com/xyz/foo/bar/abc.html#fizz")]
            [TestCase(null, "baz", "#fizz", true, "/baz/#fizz")]
            [TestCase("www.google.com", null, "#fizz", true, "http://www.google.com/#fizz")]
            [TestCase("www.google.com", "/xyz", "#fizz", true, "http://www.google.com/xyz/#fizz")]

            // makeAbsolute = false
            [TestCase(null, null, "/foo/bar/abc.html#fizz", false, "/foo/bar/abc.html#fizz")]
            [TestCase(null, null, "foo/bar/abc.html#fizz", false, "foo/bar/abc.html#fizz")]
            [TestCase(null, "baz", "/foo/bar/abc.html#fizz", false, "/baz/foo/bar/abc.html#fizz")]
            [TestCase(null, "baz/", "/foo/bar/abc.html#fizz", false, "/baz/foo/bar/abc.html#fizz")]
            [TestCase(null, "baz", "foo/bar/abc.html#fizz", false, "foo/bar/abc.html#fizz")]
            [TestCase(null, "baz/", "foo/bar/abc.html#fizz", false, "foo/bar/abc.html#fizz")]
            [TestCase("www.google.com", null, "/foo/bar/abc.html#fizz", false, "http://www.google.com/foo/bar/abc.html#fizz")]
            [TestCase("www.google.com", null, "/foo/bar/abc.html#fizz", false, "http://www.google.com/foo/bar/abc.html#fizz")]
            [TestCase("www.google.com", "/xyz", "/foo/bar/abc.html#fizz", false, "http://www.google.com/xyz/foo/bar/abc.html#fizz")]
            [TestCase("www.google.com", "/xyz/", "/foo/bar/abc.html#fizz", false, "http://www.google.com/xyz/foo/bar/abc.html#fizz")]
            [TestCase("www.google.com", null, "foo/bar/abc.html#fizz", false, "foo/bar/abc.html#fizz")]
            [TestCase("www.google.com", null, "foo/bar/abc.html#fizz", false, "foo/bar/abc.html#fizz")]
            [TestCase("www.google.com", "/xyz", "foo/bar/abc.html#fizz", false, "foo/bar/abc.html#fizz")]
            [TestCase("www.google.com", "/xyz/", "foo/bar/abc.html#fizz", false, "foo/bar/abc.html#fizz")]
            [TestCase(null, "baz", "#fizz", false, "#fizz")]
            [TestCase("www.google.com", null, "#fizz", false, "#fizz")]
            [TestCase("www.google.com", "/xyz", "#fizz", false, "#fizz")]
            public void FragmentForFilePathWithHostAndRoot(string host, string root, string path, bool makeAbsolute, string expected)
            {
                // Given
                NormalizedPath filePath = path is null ? null : new NormalizedPath(path);
                LinkGenerator linkGenerator = new LinkGenerator();

                // When
                string link = linkGenerator.GetLink(filePath, host, root is null ? null : new NormalizedPath(root), null, null, null, false, makeAbsolute);

                // Then
                link.ShouldBe(expected);
            }

            // makeAbsolute = true
            [TestCase(null, null, "/foo/bar/abc.html#", true, "/foo/bar/abc.html#")]
            [TestCase(null, null, "foo/bar/abc.html#", true, "/foo/bar/abc.html#")]
            [TestCase(null, "baz", "/foo/bar/abc.html#", true, "/baz/foo/bar/abc.html#")]
            [TestCase(null, "baz/", "/foo/bar/abc.html#", true, "/baz/foo/bar/abc.html#")]
            [TestCase(null, "baz", "foo/bar/abc.html#", true, "/baz/foo/bar/abc.html#")]
            [TestCase(null, "baz/", "foo/bar/abc.html#", true, "/baz/foo/bar/abc.html#")]
            [TestCase("www.google.com", null, "/foo/bar/abc.html#", true, "http://www.google.com/foo/bar/abc.html#")]
            [TestCase("www.google.com", null, "/foo/bar/abc.html#", true, "http://www.google.com/foo/bar/abc.html#")]
            [TestCase("www.google.com", "/xyz", "/foo/bar/abc.html#", true, "http://www.google.com/xyz/foo/bar/abc.html#")]
            [TestCase("www.google.com", "/xyz/", "/foo/bar/abc.html#", true, "http://www.google.com/xyz/foo/bar/abc.html#")]
            [TestCase("www.google.com", null, "foo/bar/abc.html#", true, "http://www.google.com/foo/bar/abc.html#")]
            [TestCase("www.google.com", null, "foo/bar/abc.html#", true, "http://www.google.com/foo/bar/abc.html#")]
            [TestCase("www.google.com", "/xyz", "foo/bar/abc.html#", true, "http://www.google.com/xyz/foo/bar/abc.html#")]
            [TestCase("www.google.com", "/xyz/", "foo/bar/abc.html#", true, "http://www.google.com/xyz/foo/bar/abc.html#")]
            [TestCase(null, "baz", "#", true, "/baz/#")]
            [TestCase("www.google.com", null, "#", true, "http://www.google.com/#")]
            [TestCase("www.google.com", "/xyz", "#", true, "http://www.google.com/xyz/#")]

            // makeAbsolute = false
            [TestCase(null, null, "/foo/bar/abc.html#", false, "/foo/bar/abc.html#")]
            [TestCase(null, null, "foo/bar/abc.html#", false, "foo/bar/abc.html#")]
            [TestCase(null, "baz", "/foo/bar/abc.html#", false, "/baz/foo/bar/abc.html#")]
            [TestCase(null, "baz/", "/foo/bar/abc.html#", false, "/baz/foo/bar/abc.html#")]
            [TestCase(null, "baz", "foo/bar/abc.html#", false, "foo/bar/abc.html#")]
            [TestCase(null, "baz/", "foo/bar/abc.html#", false, "foo/bar/abc.html#")]
            [TestCase("www.google.com", null, "/foo/bar/abc.html#", false, "http://www.google.com/foo/bar/abc.html#")]
            [TestCase("www.google.com", null, "/foo/bar/abc.html#", false, "http://www.google.com/foo/bar/abc.html#")]
            [TestCase("www.google.com", "/xyz", "/foo/bar/abc.html#", false, "http://www.google.com/xyz/foo/bar/abc.html#")]
            [TestCase("www.google.com", "/xyz/", "/foo/bar/abc.html#", false, "http://www.google.com/xyz/foo/bar/abc.html#")]
            [TestCase("www.google.com", null, "foo/bar/abc.html#", false, "foo/bar/abc.html#")]
            [TestCase("www.google.com", null, "foo/bar/abc.html#", false, "foo/bar/abc.html#")]
            [TestCase("www.google.com", "/xyz", "foo/bar/abc.html#", false, "foo/bar/abc.html#")]
            [TestCase("www.google.com", "/xyz/", "foo/bar/abc.html#", false, "foo/bar/abc.html#")]
            [TestCase(null, "baz", "#", false, "#")]
            [TestCase("www.google.com", null, "#", false, "#")]
            [TestCase("www.google.com", "/xyz", "#", false, "#")]
            public void EmptyFragmentForFilePathWithHostAndRoot(string host, string root, string path, bool makeAbsolute, string expected)
            {
                // Given
                NormalizedPath filePath = path is null ? null : new NormalizedPath(path);
                LinkGenerator linkGenerator = new LinkGenerator();

                // When
                string link = linkGenerator.GetLink(filePath, host, root is null ? null : new NormalizedPath(root), null, null, null, false, makeAbsolute);

                // Then
                link.ShouldBe(expected);
            }

            // makeAbsolute = true
            [TestCase(".?qw=er", true, "/?qw=er")]
            [TestCase("/foo/bar/index.html?qw=er", true, "/foo/bar/index.html?qw=er")]
            [TestCase("/foo/bar/index.htm?qw=er", true, "/foo/bar/index.htm?qw=er")]
            [TestCase("/foo/bar/baz.html?qw=er", true, "/foo/bar/baz.html?qw=er")]
            [TestCase("/index.html?qw=er", true, "/index.html?qw=er")]
            [TestCase("index.html?qw=er", true, "/index.html?qw=er")]
            [TestCase("/foo.html?qw=er", true, "/foo.html?qw=er")]
            [TestCase("foo.html?qw=er", true, "/foo.html?qw=er")]
            [TestCase("C:/bar/foo.html?qw=er", true, "/C:/bar/foo.html?qw=er")]
            [TestCase("C:/bar/foo.html?qw=er", true, "/C:/bar/foo.html?qw=er")]

            // makeAbsolute = false
            [TestCase(".?qw=er", false, ".?qw=er")]
            [TestCase("/foo/bar/index.html?qw=er", false, "/foo/bar/index.html?qw=er")]
            [TestCase("/foo/bar/index.htm?qw=er", false, "/foo/bar/index.htm?qw=er")]
            [TestCase("/foo/bar/baz.html?qw=er", false, "/foo/bar/baz.html?qw=er")]
            [TestCase("/index.html?qw=er", false, "/index.html?qw=er")]
            [TestCase("index.html?qw=er", false, "index.html?qw=er")]
            [TestCase("/foo.html?qw=er", false, "/foo.html?qw=er")]
            [TestCase("foo.html?qw=er", false, "foo.html?qw=er")]
            [TestCase("C:/bar/foo.html?qw=er", false, "C:/bar/foo.html?qw=er")]
            [TestCase("C:/bar/foo.html?qw=er", false, "C:/bar/foo.html?qw=er")]
            public void QueryForFilePath(string path, bool makeAbsolute, string expected)
            {
                // Given
                NormalizedPath filePath = path is null ? null : new NormalizedPath(path);
                LinkGenerator linkGenerator = new LinkGenerator();

                // When
                string link = linkGenerator.GetLink(filePath, null, null, null, null, null, false, makeAbsolute);

                // Then
                link.ShouldBe(expected);
            }

            // makeAbsolute = true
            [TestCase(null, null, "/foo/bar/abc.html?qw=er", true, "/foo/bar/abc.html?qw=er")]
            [TestCase(null, null, "foo/bar/abc.html?qw=er", true, "/foo/bar/abc.html?qw=er")]
            [TestCase(null, "baz", "/foo/bar/abc.html?qw=er", true, "/baz/foo/bar/abc.html?qw=er")]
            [TestCase(null, "baz/", "/foo/bar/abc.html?qw=er", true, "/baz/foo/bar/abc.html?qw=er")]
            [TestCase(null, "baz", "foo/bar/abc.html?qw=er", true, "/baz/foo/bar/abc.html?qw=er")]
            [TestCase(null, "baz/", "foo/bar/abc.html?qw=er", true, "/baz/foo/bar/abc.html?qw=er")]
            [TestCase("www.google.com", null, "/foo/bar/abc.html?qw=er", true, "http://www.google.com/foo/bar/abc.html?qw=er")]
            [TestCase("www.google.com", null, "/foo/bar/abc.html?qw=er", true, "http://www.google.com/foo/bar/abc.html?qw=er")]
            [TestCase("www.google.com", "/xyz", "/foo/bar/abc.html?qw=er", true, "http://www.google.com/xyz/foo/bar/abc.html?qw=er")]
            [TestCase("www.google.com", "/xyz/", "/foo/bar/abc.html?qw=er", true, "http://www.google.com/xyz/foo/bar/abc.html?qw=er")]
            [TestCase("www.google.com", null, "foo/bar/abc.html?qw=er", true, "http://www.google.com/foo/bar/abc.html?qw=er")]
            [TestCase("www.google.com", null, "foo/bar/abc.html?qw=er", true, "http://www.google.com/foo/bar/abc.html?qw=er")]
            [TestCase("www.google.com", "/xyz", "foo/bar/abc.html?qw=er", true, "http://www.google.com/xyz/foo/bar/abc.html?qw=er")]
            [TestCase("www.google.com", "/xyz/", "foo/bar/abc.html?qw=er", true, "http://www.google.com/xyz/foo/bar/abc.html?qw=er")]
            [TestCase(null, "baz", "?qw=er", true, "/baz/?qw=er")]
            [TestCase("www.google.com", null, "?qw=er", true, "http://www.google.com/?qw=er")]
            [TestCase("www.google.com", "/xyz", "?qw=er", true, "http://www.google.com/xyz/?qw=er")]

            // makeAbsolute = false
            [TestCase(null, null, "/foo/bar/abc.html?qw=er", false, "/foo/bar/abc.html?qw=er")]
            [TestCase(null, null, "foo/bar/abc.html?qw=er", false, "foo/bar/abc.html?qw=er")]
            [TestCase(null, "baz", "/foo/bar/abc.html?qw=er", false, "/baz/foo/bar/abc.html?qw=er")]
            [TestCase(null, "baz/", "/foo/bar/abc.html?qw=er", false, "/baz/foo/bar/abc.html?qw=er")]
            [TestCase(null, "baz", "foo/bar/abc.html?qw=er", false, "foo/bar/abc.html?qw=er")]
            [TestCase(null, "baz/", "foo/bar/abc.html?qw=er", false, "foo/bar/abc.html?qw=er")]
            [TestCase("www.google.com", null, "/foo/bar/abc.html?qw=er", false, "http://www.google.com/foo/bar/abc.html?qw=er")]
            [TestCase("www.google.com", null, "/foo/bar/abc.html?qw=er", false, "http://www.google.com/foo/bar/abc.html?qw=er")]
            [TestCase("www.google.com", "/xyz", "/foo/bar/abc.html?qw=er", false, "http://www.google.com/xyz/foo/bar/abc.html?qw=er")]
            [TestCase("www.google.com", "/xyz/", "/foo/bar/abc.html?qw=er", false, "http://www.google.com/xyz/foo/bar/abc.html?qw=er")]
            [TestCase("www.google.com", null, "foo/bar/abc.html?qw=er", false, "foo/bar/abc.html?qw=er")]
            [TestCase("www.google.com", null, "foo/bar/abc.html?qw=er", false, "foo/bar/abc.html?qw=er")]
            [TestCase("www.google.com", "/xyz", "foo/bar/abc.html?qw=er", false, "foo/bar/abc.html?qw=er")]
            [TestCase("www.google.com", "/xyz/", "foo/bar/abc.html?qw=er", false, "foo/bar/abc.html?qw=er")]
            [TestCase(null, "baz", "?qw=er", false, "?qw=er")]
            [TestCase("www.google.com", null, "?qw=er", false, "?qw=er")]
            [TestCase("www.google.com", "/xyz", "?qw=er", false, "?qw=er")]
            public void QueryForFilePathWithHostAndRoot(string host, string root, string path, bool makeAbsolute, string expected)
            {
                // Given
                NormalizedPath filePath = path is null ? null : new NormalizedPath(path);
                LinkGenerator linkGenerator = new LinkGenerator();

                // When
                string link = linkGenerator.GetLink(filePath, host, root is null ? null : new NormalizedPath(root), null, null, null, false, makeAbsolute);

                // Then
                link.ShouldBe(expected);
            }

            // makeAbsolute = true
            [TestCase(".?qw=er#fizz", true, "/?qw=er#fizz")]
            [TestCase("/foo/bar/index.html?qw=er#fizz", true, "/foo/bar/index.html?qw=er#fizz")]
            [TestCase("/foo/bar/index.htm?qw=er#fizz", true, "/foo/bar/index.htm?qw=er#fizz")]
            [TestCase("/foo/bar/baz.html?qw=er#fizz", true, "/foo/bar/baz.html?qw=er#fizz")]
            [TestCase("/index.html?qw=er#fizz", true, "/index.html?qw=er#fizz")]
            [TestCase("index.html?qw=er#fizz", true, "/index.html?qw=er#fizz")]
            [TestCase("/foo.html?qw=er#fizz", true, "/foo.html?qw=er#fizz")]
            [TestCase("foo.html?qw=er#fizz", true, "/foo.html?qw=er#fizz")]
            [TestCase("C:/bar/foo.html?qw=er#fizz", true, "/C:/bar/foo.html?qw=er#fizz")]
            [TestCase("C:/bar/foo.html?qw=er#fizz", true, "/C:/bar/foo.html?qw=er#fizz")]

            // makeAbsolute = false
            [TestCase(".?qw=er#fizz", false, ".?qw=er#fizz")]
            [TestCase("/foo/bar/index.html?qw=er#fizz", false, "/foo/bar/index.html?qw=er#fizz")]
            [TestCase("/foo/bar/index.htm?qw=er#fizz", false, "/foo/bar/index.htm?qw=er#fizz")]
            [TestCase("/foo/bar/baz.html?qw=er#fizz", false, "/foo/bar/baz.html?qw=er#fizz")]
            [TestCase("/index.html?qw=er#fizz", false, "/index.html?qw=er#fizz")]
            [TestCase("index.html?qw=er#fizz", false, "index.html?qw=er#fizz")]
            [TestCase("/foo.html?qw=er#fizz", false, "/foo.html?qw=er#fizz")]
            [TestCase("foo.html?qw=er#fizz", false, "foo.html?qw=er#fizz")]
            [TestCase("C:/bar/foo.html?qw=er#fizz", false, "C:/bar/foo.html?qw=er#fizz")]
            [TestCase("C:/bar/foo.html?qw=er#fizz", false, "C:/bar/foo.html?qw=er#fizz")]
            public void QueryAndFragmentForFilePath(string path, bool makeAbsolute, string expected)
            {
                // Given
                NormalizedPath filePath = path is null ? null : new NormalizedPath(path);
                LinkGenerator linkGenerator = new LinkGenerator();

                // When
                string link = linkGenerator.GetLink(filePath, null, null, null, null, null, false, makeAbsolute);

                // Then
                link.ShouldBe(expected);
            }

            // makeAbsolute = true
            [TestCase(".?qw=er#", true, "/?qw=er#")]
            [TestCase("/foo/bar/index.html?qw=er#", true, "/foo/bar/index.html?qw=er#")]
            [TestCase("/foo/bar/index.htm?qw=er#", true, "/foo/bar/index.htm?qw=er#")]
            [TestCase("/foo/bar/baz.html?qw=er#", true, "/foo/bar/baz.html?qw=er#")]
            [TestCase("/index.html?qw=er#", true, "/index.html?qw=er#")]
            [TestCase("index.html?qw=er#", true, "/index.html?qw=er#")]
            [TestCase("/foo.html?qw=er#", true, "/foo.html?qw=er#")]
            [TestCase("foo.html?qw=er#", true, "/foo.html?qw=er#")]
            [TestCase("C:/bar/foo.html?qw=er#", true, "/C:/bar/foo.html?qw=er#")]
            [TestCase("C:/bar/foo.html?qw=er#", true, "/C:/bar/foo.html?qw=er#")]

            // makeAbsolute = false
            [TestCase(".?qw=er#", false, ".?qw=er#")]
            [TestCase("/foo/bar/index.html?qw=er#", false, "/foo/bar/index.html?qw=er#")]
            [TestCase("/foo/bar/index.htm?qw=er#", false, "/foo/bar/index.htm?qw=er#")]
            [TestCase("/foo/bar/baz.html?qw=er#", false, "/foo/bar/baz.html?qw=er#")]
            [TestCase("/index.html?qw=er#", false, "/index.html?qw=er#")]
            [TestCase("index.html?qw=er#", false, "index.html?qw=er#")]
            [TestCase("/foo.html?qw=er#", false, "/foo.html?qw=er#")]
            [TestCase("foo.html?qw=er#", false, "foo.html?qw=er#")]
            [TestCase("C:/bar/foo.html?qw=er#", false, "C:/bar/foo.html?qw=er#")]
            [TestCase("C:/bar/foo.html?qw=er#", false, "C:/bar/foo.html?qw=er#")]
            public void QueryAndEmptyFragment(string path, bool makeAbsolute, string expected)
            {
                // Given
                NormalizedPath filePath = path is null ? null : new NormalizedPath(path);
                LinkGenerator linkGenerator = new LinkGenerator();

                // When
                string link = linkGenerator.GetLink(filePath, null, null, null, null, null, false, makeAbsolute);

                // Then
                link.ShouldBe(expected);
            }

            // makeAbsolute = true
            [TestCase(null, null, "/foo/bar/abc.html?qw=er#fizz", true, "/foo/bar/abc.html?qw=er#fizz")]
            [TestCase(null, null, "foo/bar/abc.html?qw=er#fizz", true, "/foo/bar/abc.html?qw=er#fizz")]
            [TestCase(null, "baz", "/foo/bar/abc.html?qw=er#fizz", true, "/baz/foo/bar/abc.html?qw=er#fizz")]
            [TestCase(null, "baz/", "/foo/bar/abc.html?qw=er#fizz", true, "/baz/foo/bar/abc.html?qw=er#fizz")]
            [TestCase(null, "baz", "foo/bar/abc.html?qw=er#fizz", true, "/baz/foo/bar/abc.html?qw=er#fizz")]
            [TestCase(null, "baz/", "foo/bar/abc.html?qw=er#fizz", true, "/baz/foo/bar/abc.html?qw=er#fizz")]
            [TestCase("www.google.com", null, "/foo/bar/abc.html?qw=er#fizz", true, "http://www.google.com/foo/bar/abc.html?qw=er#fizz")]
            [TestCase("www.google.com", null, "/foo/bar/abc.html?qw=er#fizz", true, "http://www.google.com/foo/bar/abc.html?qw=er#fizz")]
            [TestCase("www.google.com", "/xyz", "/foo/bar/abc.html?qw=er#fizz", true, "http://www.google.com/xyz/foo/bar/abc.html?qw=er#fizz")]
            [TestCase("www.google.com", "/xyz/", "/foo/bar/abc.html?qw=er#fizz", true, "http://www.google.com/xyz/foo/bar/abc.html?qw=er#fizz")]
            [TestCase("www.google.com", null, "foo/bar/abc.html?qw=er#fizz", true, "http://www.google.com/foo/bar/abc.html?qw=er#fizz")]
            [TestCase("www.google.com", null, "foo/bar/abc.html?qw=er#fizz", true, "http://www.google.com/foo/bar/abc.html?qw=er#fizz")]
            [TestCase("www.google.com", "/xyz", "foo/bar/abc.html?qw=er#fizz", true, "http://www.google.com/xyz/foo/bar/abc.html?qw=er#fizz")]
            [TestCase("www.google.com", "/xyz/", "foo/bar/abc.html?qw=er#fizz", true, "http://www.google.com/xyz/foo/bar/abc.html?qw=er#fizz")]
            [TestCase(null, "baz", "?qw=er#fizz", true, "/baz/?qw=er#fizz")]
            [TestCase("www.google.com", null, "?qw=er#fizz", true, "http://www.google.com/?qw=er#fizz")]
            [TestCase("www.google.com", "/xyz", "?qw=er#fizz", true, "http://www.google.com/xyz/?qw=er#fizz")]

            // makeAbsolute = false
            [TestCase(null, null, "/foo/bar/abc.html?qw=er#fizz", false, "/foo/bar/abc.html?qw=er#fizz")]
            [TestCase(null, null, "foo/bar/abc.html?qw=er#fizz", false, "foo/bar/abc.html?qw=er#fizz")]
            [TestCase(null, "baz", "/foo/bar/abc.html?qw=er#fizz", false, "/baz/foo/bar/abc.html?qw=er#fizz")]
            [TestCase(null, "baz/", "/foo/bar/abc.html?qw=er#fizz", false, "/baz/foo/bar/abc.html?qw=er#fizz")]
            [TestCase(null, "baz", "foo/bar/abc.html?qw=er#fizz", false, "foo/bar/abc.html?qw=er#fizz")]
            [TestCase(null, "baz/", "foo/bar/abc.html?qw=er#fizz", false, "foo/bar/abc.html?qw=er#fizz")]
            [TestCase("www.google.com", null, "/foo/bar/abc.html?qw=er#fizz", false, "http://www.google.com/foo/bar/abc.html?qw=er#fizz")]
            [TestCase("www.google.com", null, "/foo/bar/abc.html?qw=er#fizz", false, "http://www.google.com/foo/bar/abc.html?qw=er#fizz")]
            [TestCase("www.google.com", "/xyz", "/foo/bar/abc.html?qw=er#fizz", false, "http://www.google.com/xyz/foo/bar/abc.html?qw=er#fizz")]
            [TestCase("www.google.com", "/xyz/", "/foo/bar/abc.html?qw=er#fizz", false, "http://www.google.com/xyz/foo/bar/abc.html?qw=er#fizz")]
            [TestCase("www.google.com", null, "foo/bar/abc.html?qw=er#fizz", false, "foo/bar/abc.html?qw=er#fizz")]
            [TestCase("www.google.com", null, "foo/bar/abc.html?qw=er#fizz", false, "foo/bar/abc.html?qw=er#fizz")]
            [TestCase("www.google.com", "/xyz", "foo/bar/abc.html?qw=er#fizz", false, "foo/bar/abc.html?qw=er#fizz")]
            [TestCase("www.google.com", "/xyz/", "foo/bar/abc.html?qw=er#fizz", false, "foo/bar/abc.html?qw=er#fizz")]
            [TestCase(null, "baz", "?qw=er#fizz", false, "?qw=er#fizz")]
            [TestCase("www.google.com", null, "?qw=er#fizz", false, "?qw=er#fizz")]
            [TestCase("www.google.com", "/xyz", "?qw=er#fizz", false, "?qw=er#fizz")]
            public void QueryAndFragmentForFilePathWithHostAndRoot(string host, string root, string path, bool makeAbsolute, string expected)
            {
                // Given
                NormalizedPath filePath = path is null ? null : new NormalizedPath(path);
                LinkGenerator linkGenerator = new LinkGenerator();

                // When
                string link = linkGenerator.GetLink(filePath, host, root is null ? null : new NormalizedPath(root), null, null, null, false, makeAbsolute);

                // Then
                link.ShouldBe(expected);
            }

            // makeAbsolute = true
            [TestCase(null, null, "/foo/bar/abc.html?qw=er#", true, "/foo/bar/abc.html?qw=er#")]
            [TestCase(null, null, "foo/bar/abc.html?qw=er#", true, "/foo/bar/abc.html?qw=er#")]
            [TestCase(null, "baz", "/foo/bar/abc.html?qw=er#", true, "/baz/foo/bar/abc.html?qw=er#")]
            [TestCase(null, "baz/", "/foo/bar/abc.html?qw=er#", true, "/baz/foo/bar/abc.html?qw=er#")]
            [TestCase(null, "baz", "foo/bar/abc.html?qw=er#", true, "/baz/foo/bar/abc.html?qw=er#")]
            [TestCase(null, "baz/", "foo/bar/abc.html?qw=er#", true, "/baz/foo/bar/abc.html?qw=er#")]
            [TestCase("www.google.com", null, "/foo/bar/abc.html?qw=er#", true, "http://www.google.com/foo/bar/abc.html?qw=er#")]
            [TestCase("www.google.com", null, "/foo/bar/abc.html?qw=er#", true, "http://www.google.com/foo/bar/abc.html?qw=er#")]
            [TestCase("www.google.com", "/xyz", "/foo/bar/abc.html?qw=er#", true, "http://www.google.com/xyz/foo/bar/abc.html?qw=er#")]
            [TestCase("www.google.com", "/xyz/", "/foo/bar/abc.html?qw=er#", true, "http://www.google.com/xyz/foo/bar/abc.html?qw=er#")]
            [TestCase("www.google.com", null, "foo/bar/abc.html?qw=er#", true, "http://www.google.com/foo/bar/abc.html?qw=er#")]
            [TestCase("www.google.com", null, "foo/bar/abc.html?qw=er#", true, "http://www.google.com/foo/bar/abc.html?qw=er#")]
            [TestCase("www.google.com", "/xyz", "foo/bar/abc.html?qw=er#", true, "http://www.google.com/xyz/foo/bar/abc.html?qw=er#")]
            [TestCase("www.google.com", "/xyz/", "foo/bar/abc.html?qw=er#", true, "http://www.google.com/xyz/foo/bar/abc.html?qw=er#")]
            [TestCase(null, "baz", "?qw=er#", true, "/baz/?qw=er#")]
            [TestCase("www.google.com", null, "?qw=er#", true, "http://www.google.com/?qw=er#")]
            [TestCase("www.google.com", "/xyz", "?qw=er#", true, "http://www.google.com/xyz/?qw=er#")]

            // makeAbsolute = false
            [TestCase(null, null, "/foo/bar/abc.html?qw=er#", false, "/foo/bar/abc.html?qw=er#")]
            [TestCase(null, null, "foo/bar/abc.html?qw=er#", false, "foo/bar/abc.html?qw=er#")]
            [TestCase(null, "baz", "/foo/bar/abc.html?qw=er#", false, "/baz/foo/bar/abc.html?qw=er#")]
            [TestCase(null, "baz/", "/foo/bar/abc.html?qw=er#", false, "/baz/foo/bar/abc.html?qw=er#")]
            [TestCase(null, "baz", "foo/bar/abc.html?qw=er#", false, "foo/bar/abc.html?qw=er#")]
            [TestCase(null, "baz/", "foo/bar/abc.html?qw=er#", false, "foo/bar/abc.html?qw=er#")]
            [TestCase("www.google.com", null, "/foo/bar/abc.html?qw=er#", false, "http://www.google.com/foo/bar/abc.html?qw=er#")]
            [TestCase("www.google.com", null, "/foo/bar/abc.html?qw=er#", false, "http://www.google.com/foo/bar/abc.html?qw=er#")]
            [TestCase("www.google.com", "/xyz", "/foo/bar/abc.html?qw=er#", false, "http://www.google.com/xyz/foo/bar/abc.html?qw=er#")]
            [TestCase("www.google.com", "/xyz/", "/foo/bar/abc.html?qw=er#", false, "http://www.google.com/xyz/foo/bar/abc.html?qw=er#")]
            [TestCase("www.google.com", null, "foo/bar/abc.html?qw=er#", false, "foo/bar/abc.html?qw=er#")]
            [TestCase("www.google.com", null, "foo/bar/abc.html?qw=er#", false, "foo/bar/abc.html?qw=er#")]
            [TestCase("www.google.com", "/xyz", "foo/bar/abc.html?qw=er#", false, "foo/bar/abc.html?qw=er#")]
            [TestCase("www.google.com", "/xyz/", "foo/bar/abc.html?qw=er#", false, "foo/bar/abc.html?qw=er#")]
            [TestCase(null, "baz", "?qw=er#", false, "?qw=er#")]
            [TestCase("www.google.com", null, "?qw=er#", false, "?qw=er#")]
            [TestCase("www.google.com", "/xyz", "?qw=er#", false, "?qw=er#")]
            public void QueryAndEmptyFragmentForFilePathWithHostAndRoot(string host, string root, string path, bool makeAbsolute, string expected)
            {
                // Given
                NormalizedPath filePath = path is null ? null : new NormalizedPath(path);
                LinkGenerator linkGenerator = new LinkGenerator();

                // When
                string link = linkGenerator.GetLink(filePath, host, root is null ? null : new NormalizedPath(root), null, null, null, false, makeAbsolute);

                // Then
                link.ShouldBe(expected);
            }

            [Test]
            public void EscapesFragmentIdentifierInRootPath()
            {
                // Given
                NormalizedPath filePath = new NormalizedPath("/a/b/c?foo=bar#buzz");
                LinkGenerator linkGenerator = new LinkGenerator();

                // When
                string link = linkGenerator.GetLink(filePath, "www.google.com", new NormalizedPath("x#y/z/"), null, null, null, false, true);

                // Then
                link.ShouldBe("http://www.google.com/x%23y/z/a/b/c?foo=bar#buzz");
            }

            [Test]
            public void EscapesQueryIdentifierInRootPath()
            {
                // Given
                NormalizedPath filePath = new NormalizedPath("/a/b/c?foo=bar#buzz");
                LinkGenerator linkGenerator = new LinkGenerator();

                // When
                string link = linkGenerator.GetLink(filePath, "www.google.com", new NormalizedPath("x?y/z/"), null, null, null, false, true);

                // Then
                link.ShouldBe("http://www.google.com/x%3Fy/z/a/b/c?foo=bar#buzz");
            }
        }
    }
}