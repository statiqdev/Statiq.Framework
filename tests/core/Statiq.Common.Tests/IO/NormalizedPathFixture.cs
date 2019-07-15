using System;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.IO
{
    [TestFixture]
    public class NormalizedPathFixture : BaseFixture
    {
        private class TestPath : NormalizedPath
        {
            public TestPath(string path, PathKind pathKind = PathKind.RelativeOrAbsolute)
                : base(path, pathKind)
            {
            }
        }

        public class ConstructorTests : NormalizedPathFixture
        {
            [Test]
            public void ShouldThrowIfPathIsNull()
            {
                // Given, When, Then
                Assert.Throws<ArgumentNullException>(() => new TestPath(null));
            }

            [TestCase("")]
            [TestCase("\t ")]
            public void ShouldThrowIfPathIsEmpty(string fullPath)
            {
                // Given, When, Then
                Assert.Throws<ArgumentException>(() => new TestPath(fullPath));
            }

            [Test]
            public void CurrentDirectoryReturnsDot()
            {
                // Given, When
                TestPath path = new TestPath("./");

                // Then
                Assert.AreEqual(".", path.FullPath);
            }

            [Test]
            public void ShouldNormalizePathSeparators()
            {
                // Given, When
                TestPath path = new TestPath("shaders\\basic");

                // Then
                Assert.AreEqual("shaders/basic", path.FullPath);
            }

            [Test]
            public void ShouldTrimWhiteSpaceFromPathAndLeaveSpaces()
            {
                // Given, When
                TestPath path = new TestPath("\t\r\nshaders/basic ");

                // Then
                Assert.AreEqual("shaders/basic ", path.FullPath);
            }

            [Test]
            public void ShouldNotRemoveWhiteSpaceWithinPath()
            {
                // Given, When
                TestPath path = new TestPath("my awesome shaders/basic");

                // Then
                Assert.AreEqual("my awesome shaders/basic", path.FullPath);
            }

            [TestCase("/Hello/World/", "/Hello/World")]
            [TestCase("\\Hello\\World\\", "/Hello/World")]
            [TestCase("file.txt/", "file.txt")]
            [TestCase("file.txt\\", "file.txt")]
            [TestCase("Temp/file.txt/", "Temp/file.txt")]
            [TestCase("Temp\\file.txt\\", "Temp/file.txt")]
            public void ShouldRemoveTrailingSlashes(string value, string expected)
            {
                // Given, When
                TestPath path = new TestPath(value);

                // Then
                Assert.AreEqual(expected, path.FullPath);
            }

            [TestCase("\\")]
            [TestCase("/")]
            public void ShouldNotRemoveSingleTrailingSlash(string value)
            {
                // Given, When
                TestPath path = new TestPath(value);

                // Then
                Assert.AreEqual("/", path.FullPath);
            }

            [TestCase("./Hello/World/", "Hello/World")]
            [TestCase(".\\Hello/World/", "Hello/World")]
            [TestCase("./file.txt", "file.txt")]
            [TestCase("./Temp/file.txt", "Temp/file.txt")]
            public void ShouldRemoveRelativePrefix(string value, string expected)
            {
                // Given, When
                TestPath path = new TestPath(value);

                // Then
                Assert.AreEqual(expected, path.FullPath);
            }

            [TestCase("\\")]
            [TestCase("/")]
            public void ShouldNotRemoveOnlyRelativePart(string value)
            {
                // Given, When
                TestPath path = new TestPath(value);

                // Then
                Assert.AreEqual("/", path.FullPath);
            }
        }

        public class SegmentsTests : NormalizedPathFixture
        {
            [TestCase("Hello/World")]
            [TestCase("/Hello/World")]
            [TestCase("/Hello/World/")]
            [TestCase("./Hello/World/")]
            public void ShouldReturnSegmentsOfPath(string pathName)
            {
                // Given, When
                TestPath path = new TestPath(pathName);

                // Then
                Assert.AreEqual(2, path.Segments.Length);
                Assert.AreEqual("Hello", path.Segments[0].ToString());
                Assert.AreEqual("World", path.Segments[1].ToString());
            }
        }

        public class FullPathTests : NormalizedPathFixture
        {
            [Test]
            public void ShouldReturnFullPath()
            {
                // Given, When
                const string expected = "shaders/basic";
                TestPath path = new TestPath(expected);

                // Then
                path.FullPath.ShouldBe(expected);
            }

            [Test]
            public void ShouldReturnFullPathForInferedAbsolutePath()
            {
                // Given, When
                const string expected = "/shaders/basic";
                TestPath path = new TestPath(expected);

                // Then
                path.FullPath.ShouldBe(expected);
            }

            [Test]
            public void ShouldReturnFullPathForExplicitAbsolutePath()
            {
                // Given, When
                const string expected = "shaders/basic";
                TestPath path = new TestPath(expected, PathKind.Absolute);

                // Then
                path.FullPath.ShouldBe(expected);
            }

            [WindowsTest]
            public void ShouldNotPrependSlashForRootedPath()
            {
                // Given, When
                TestPath path = new TestPath("C:/shaders/basic");

                // Then
                path.FullPath.ShouldBe("C:\\shaders/basic");
            }
        }

        public class RootTests : NormalizedPathFixture
        {
            [TestCase(@"\a\b\c", "/")]
            [TestCase("/a/b/c", "/")]
            [TestCase("a/b/c", ".")]
            [TestCase(@"a\b\c", ".")]
            [TestCase("foo.txt", ".")]
            [TestCase("foo", ".")]
            [WindowsTestCase(@"c:\a\b\c", "c:/")]
            [WindowsTestCase("c:/a/b/c", "c:/")]
            public void ShouldReturnRootPath(string fullPath, string expected)
            {
                // Given
                TestPath path = new TestPath(fullPath);

                // When
                DirectoryPath root = path.Root;

                // Then
                Assert.AreEqual(expected, root.FullPath);
            }

            [TestCase(@"\a\b\c")]
            [TestCase("/a/b/c")]
            [TestCase("a/b/c")]
            [TestCase(@"a\b\c")]
            [TestCase("foo.txt")]
            [TestCase("foo")]
            [WindowsTestCase(@"c:\a\b\c")]
            [WindowsTestCase("c:/a/b/c")]
            public void ShouldReturnDottedRootForExplicitRelativePath(string fullPath)
            {
                // Given
                TestPath path = new TestPath(fullPath, PathKind.Relative);

                // When
                DirectoryPath root = path.Root;

                // Then
                Assert.AreEqual(".", root.FullPath);
            }
        }

        public class IsRelativeTests : NormalizedPathFixture
        {
            [TestCase("assets/shaders", true)]
            [TestCase("assets/shaders/basic.frag", true)]
            [TestCase("/assets/shaders", false)]
            [TestCase("/assets/shaders/basic.frag", false)]
            public void ShouldReturnWhetherOrNotAPathIsRelative(string fullPath, bool expected)
            {
                // Given, When
                TestPath path = new TestPath(fullPath);

                // Then
                Assert.AreEqual(expected, path.IsRelative);
            }

            [WindowsTestCase("c:/assets/shaders", false)]
            [WindowsTestCase("c:/assets/shaders/basic.frag", false)]
            [WindowsTestCase("c:/", false)]
            [WindowsTestCase("c:", false)]
            public void ShouldReturnWhetherOrNotAPathIsRelativeOnWindows(string fullPath, bool expected)
            {
                // Given, When
                TestPath path = new TestPath(fullPath);

                // Then
                Assert.AreEqual(expected, path.IsRelative);
            }
        }

        public class ToStringTests : NormalizedPathFixture
        {
            [TestCase("temp/hello", "temp/hello")]
            [TestCase("/temp/hello", "/temp/hello")]
            [WindowsTestCase("c:/temp/hello", "c:/temp/hello")]
            public void ShouldReturnStringRepresentation(string path, string expected)
            {
                // Given, When
                TestPath testPath = new TestPath(path);

                // Then
                Assert.AreEqual(expected, testPath.ToString());
            }
        }

        public class GetFullPathAndSegmentsTests : NormalizedPathFixture
        {
            [TestCase("hello/temp/test/../world", "hello/temp/world")]
            [TestCase("../hello/temp/test/../world", "../hello/temp/world")]
            [TestCase("../hello/world", "../hello/world")]
            [TestCase("hello/temp/test/../../world", "hello/world")]
            [TestCase("hello/temp/../temp2/../world", "hello/world")]
            [TestCase("/hello/temp/test/../../world", "/hello/world")]
            [TestCase("/hello/../../../../../../temp", "/../../../../../temp")]
            [TestCase("/hello/../../foo/../../../../temp", "/../../../../temp")]
            [TestCase(".", ".")]
            [TestCase("..", "..")]
            [TestCase("/..", "/..")]
            [TestCase("/.", "/", new[] { "/" })]
            [TestCase("/", "/")]
            [TestCase("./.././foo", "./../foo")]
            [TestCase("./a", "./a")]
            [TestCase("./..", "./..")]
            [TestCase("a/./b", "a/b")]
            [TestCase("/a/./b", "/a/b")]
            [TestCase("a/b/.", "a/b")]
            [TestCase("/a/b/.", "/a/b")]
            [TestCase("/./a/b", "/a/b")]
            [TestCase("/././a/b", "/a/b")]
            [TestCase("/a/b/c/../d/baz.txt", "/a/b/d/baz.txt")]
            [TestCase("../d/baz.txt", "../d/baz.txt")]
            [TestCase("../a/b/c/../d/baz.txt", "../a/b/d/baz.txt")]
            [TestCase("/a/b/c/../d", "/a/b/d")]
            [WindowsTestCase("c:/hello/temp/test/../../world", "c:/hello/world")]
            [WindowsTestCase("c:/../../../../../../temp", "c:/../../../../../../temp")]
            [WindowsTestCase("c:/../../foo/../../../../temp", "c:/../../../../../temp")]
            [WindowsTestCase("c:/a/b/c/../d/baz.txt", "c:/a/b/d/baz.txt")]
            [WindowsTestCase("c:/a/b/c/../d", "c:/a/b/d")]
            public void ShouldCollapsePath(string fullPath, string expectedFullPath, string[] expectedSegments = null)
            {
                // Given, When
                (string, ReadOnlyMemory<char>[]) fullPathAndSegments = NormalizedPath.GetFullPathAndSegments(fullPath.AsSpan());

                // Then
                fullPathAndSegments.Item1.ShouldBe(expectedFullPath);
                fullPathAndSegments.Item2.ToStrings().ShouldBe(expectedSegments ?? expectedFullPath.Split('/', StringSplitOptions.RemoveEmptyEntries));
            }

            [Test]
            public void SegmentsShouldBeEmptyForRoot()
            {
                // Given, When
                (string, ReadOnlyMemory<char>[]) fullPathAndSegments = NormalizedPath.GetFullPathAndSegments("/".AsSpan());

                // Then
                fullPathAndSegments.Item1.ShouldBe("/");
                fullPathAndSegments.Item2.ShouldBeEmpty();
            }
        }

        public class EqualsTests : NormalizedPathFixture
        {
            [TestCase(true)]
            [TestCase(false)]
            public void SameAssetInstancesIsConsideredEqual(bool isCaseSensitive)
            {
                // Given, When
                FilePath path = new FilePath("shaders/basic.vert");

                // Then
                Assert.True(path.Equals(path));
            }

            [TestCase(true)]
            [TestCase(false)]
            public void PathsAreConsideredInequalIfAnyIsNull(bool isCaseSensitive)
            {
                // Given, When
                bool result = new FilePath("test.txt").Equals(null);

                // Then
                Assert.False(result);
            }

            [TestCase(true)]
            [TestCase(false)]
            public void SamePathsAreConsideredEqual(bool isCaseSensitive)
            {
                // Given, When
                FilePath first = new FilePath("shaders/basic.vert");
                FilePath second = new FilePath("shaders/basic.vert");

                // Then
                Assert.True(first.Equals(second));
                Assert.True(second.Equals(first));
            }

            [Test]
            public void DifferentPathsAreNotConsideredEqual()
            {
                // Given, When
                FilePath first = new FilePath("shaders/basic.vert");
                FilePath second = new FilePath("shaders/basic.frag");

                // Then
                Assert.False(first.Equals(second));
                Assert.False(second.Equals(first));
            }

            [NonParallelizable]
            [TestCase(StringComparison.Ordinal, false)]
            [TestCase(StringComparison.OrdinalIgnoreCase, true)]
            public void SamePathsButDifferentCasingFollowComparison(StringComparison comparisonType, bool expected)
            {
                // Given
                NormalizedPath.PathComparisonType = comparisonType;
                FilePath first = new FilePath("shaders/basic.vert");
                FilePath second = new FilePath("SHADERS/BASIC.VERT");

                // When
                bool firstResult = first.Equals(second);
                bool secondResult = second.Equals(first);

                // Then
                firstResult.ShouldBe(expected);
                secondResult.ShouldBe(expected);
            }
        }

        public class GetHashCodeTests : NormalizedPathFixture
        {
            [Test]
            public void SamePathsGetSameHashCode()
            {
                // Given, When
                FilePath first = new FilePath("shaders/basic.vert");
                FilePath second = new FilePath("shaders/basic.vert");

                // Then
                Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
            }

            [Test]
            public void DifferentPathsGetDifferentHashCodes()
            {
                // Given, When
                FilePath first = new FilePath("shaders/basic.vert");
                FilePath second = new FilePath("shaders/basic.frag");

                // Then
                Assert.AreNotEqual(first.GetHashCode(), second.GetHashCode());
            }

            [NonParallelizable]
            [TestCase(StringComparison.Ordinal, false)]
            [TestCase(StringComparison.OrdinalIgnoreCase, true)]
            public void SamePathsButDifferentCasingFollowComparison(StringComparison comparisonType, bool expected)
            {
                // Given
                NormalizedPath.PathComparisonType = comparisonType;
                FilePath first = new FilePath("shaders/basic.vert");
                FilePath second = new FilePath("SHADERS/BASIC.VERT");

                // When
                bool result = first.GetHashCode().Equals(second.GetHashCode());

                // Then
                result.ShouldBe(expected);
            }
        }
    }
}
