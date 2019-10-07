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
                Should.Throw<ArgumentNullException>(() => new TestPath(null));
            }

            [TestCase("")]
            [TestCase("\t ")]
            public void ShouldThrowIfPathIsEmpty(string fullPath)
            {
                // Given, When, Then
                Should.Throw<ArgumentException>(() => new TestPath(fullPath));
            }

            [Test]
            public void CurrentDirectoryReturnsDot()
            {
                // Given, When
                TestPath path = new TestPath("./");

                // Then
                path.FullPath.ShouldBe(".");
            }

            [Test]
            public void ShouldNormalizePathSeparators()
            {
                // Given, When
                TestPath path = new TestPath("shaders\\basic");

                // Then
                path.FullPath.ShouldBe("shaders/basic");
            }

            [Test]
            public void ShouldTrimWhiteSpaceFromPathAndLeaveSpaces()
            {
                // Given, When
                TestPath path = new TestPath("\t\r\nshaders/basic ");

                // Then
                path.FullPath.ShouldBe("shaders/basic ");
            }

            [Test]
            public void ShouldNotRemoveWhiteSpaceWithinPath()
            {
                // Given, When
                TestPath path = new TestPath("my awesome shaders/basic");

                // Then
                path.FullPath.ShouldBe("my awesome shaders/basic");
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
                path.FullPath.ShouldBe(expected);
            }

            [TestCase("\\")]
            [TestCase("/")]
            public void ShouldNotRemoveSingleTrailingSlash(string value)
            {
                // Given, When
                TestPath path = new TestPath(value);

                // Then
                path.FullPath.ShouldBe("/");
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
                path.FullPath.ShouldBe(expected);
            }

            [TestCase("\\")]
            [TestCase("/")]
            public void ShouldNotRemoveOnlyRelativePart(string value)
            {
                // Given, When
                TestPath path = new TestPath(value);

                // Then
                path.FullPath.ShouldBe("/");
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
                path.Segments.Length.ShouldBe(2);
                path.Segments[0].ToString().ShouldBe("Hello");
                path.Segments[1].ToString().ShouldBe("World");
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
                root.FullPath.ShouldBe(expected);
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
                root.FullPath.ShouldBe(".");
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
                path.IsRelative.ShouldBe(expected);
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
                path.IsRelative.ShouldBe(expected);
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
                testPath.ToString().ShouldBe(expected);
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
            [TestCase(StringComparison.Ordinal)]
            [TestCase(StringComparison.OrdinalIgnoreCase)]
            public void SameAssetInstanceIsEqual(StringComparison comparisonType)
            {
                // Given, When
                FilePath path = new FilePath("shaders/basic.vert");

                // Then
                path.Equals(path, comparisonType).ShouldBeTrue();
            }

            [TestCase(StringComparison.Ordinal)]
            [TestCase(StringComparison.OrdinalIgnoreCase)]
            public void PathsAreInequalIfAnyIsNull(StringComparison comparisonType)
            {
                // Given, When
                bool result = new FilePath("test.txt").Equals(null, comparisonType);

                // Then
                result.ShouldBeFalse();
            }

            [TestCase(StringComparison.Ordinal)]
            [TestCase(StringComparison.OrdinalIgnoreCase)]
            public void SamePathsAreEqual(StringComparison comparisonType)
            {
                // Given, When
                FilePath first = new FilePath("shaders/basic.vert");
                FilePath second = new FilePath("shaders/basic.vert");

                // Then
                first.Equals(second, comparisonType).ShouldBeTrue();
                second.Equals(first, comparisonType).ShouldBeTrue();
            }

            [TestCase(StringComparison.Ordinal)]
            [TestCase(StringComparison.OrdinalIgnoreCase)]
            public void DifferentPathsAreNotEqual(StringComparison comparisonType)
            {
                // Given, When
                FilePath first = new FilePath("shaders/basic.vert");
                FilePath second = new FilePath("shaders/basic.frag");

                // Then
                first.Equals(second, comparisonType).ShouldBeFalse();
                second.Equals(first, comparisonType).ShouldBeFalse();
            }

            [TestCase(StringComparison.Ordinal, false)]
            [TestCase(StringComparison.OrdinalIgnoreCase, true)]
            public void SamePathsButDifferentCasingFollowComparison(StringComparison comparisonType, bool expected)
            {
                // Given
                FilePath first = new FilePath("shaders/basic.vert");
                FilePath second = new FilePath("SHADERS/BASIC.VERT");

                // When
                bool firstResult = first.Equals(second, comparisonType);
                bool secondResult = second.Equals(first, comparisonType);

                // Then
                firstResult.ShouldBe(expected);
                secondResult.ShouldBe(expected);
            }
        }

        public class GetHashCodeTests : NormalizedPathFixture
        {
            [TestCase(StringComparison.Ordinal)]
            [TestCase(StringComparison.OrdinalIgnoreCase)]
            public void SamePathsGetSameHashCode(StringComparison comparisonType)
            {
                // Given, When
                FilePath first = new FilePath("shaders/basic.vert");
                FilePath second = new FilePath("shaders/basic.vert");

                // Then
                first.GetHashCode(comparisonType).ShouldBe(second.GetHashCode(comparisonType));
            }

            [TestCase(StringComparison.Ordinal)]
            [TestCase(StringComparison.OrdinalIgnoreCase)]
            public void DifferentPathsGetDifferentHashCodes(StringComparison comparisonType)
            {
                // Given, When
                FilePath first = new FilePath("shaders/basic.vert");
                FilePath second = new FilePath("shaders/basic.frag");

                // Then
                first.GetHashCode(comparisonType).ShouldNotBe(second.GetHashCode(comparisonType));
            }

            [TestCase(StringComparison.Ordinal, false)]
            [TestCase(StringComparison.OrdinalIgnoreCase, true)]
            public void SamePathsButDifferentCasingFollowComparison(StringComparison comparisonType, bool expected)
            {
                // Given
                FilePath first = new FilePath("shaders/basic.vert");
                FilePath second = new FilePath("SHADERS/BASIC.VERT");

                // When
                bool result = first.GetHashCode(comparisonType).Equals(second.GetHashCode(comparisonType));

                // Then
                result.ShouldBe(expected);
            }
        }

        public class EqualityOperatorTests : NormalizedPathFixture
        {
            [Test]
            public void SameAssetInstanceIsEqual()
            {
                // Given, When
                FilePath path = new FilePath("shaders/basic.vert");

                // Then
#pragma warning disable CS1718 // Comparison made to same variable
                (path == path).ShouldBeTrue();
#pragma warning restore CS1718 // Comparison made to same variable
            }

            [Test]
            public void PathsAreInequalIfAnyIsNull()
            {
                // Given, When
                FilePath result = new FilePath("test.txt");

                // Then
                (result == null).ShouldBeFalse();
            }

            [Test]
            public void SamePathsAreEqual()
            {
                // Given, When
                FilePath first = new FilePath("shaders/basic.vert");
                FilePath second = new FilePath("shaders/basic.vert");

                // Then
                (first == second).ShouldBeTrue();
                (second == first).ShouldBeTrue();
            }

            [Test]
            public void DifferentPathsAreNotEqual()
            {
                // Given, When
                FilePath first = new FilePath("shaders/basic.vert");
                FilePath second = new FilePath("shaders/basic.frag");

                // Then
                (first == second).ShouldBeFalse();
                (second == first).ShouldBeFalse();
            }

            [Test]
            public void StringPathsAreEqual()
            {
                // Given, When
                FilePath first = new FilePath("shaders/basic.vert");
                string second = "shaders/basic.vert";

                // Then
                (first == second).ShouldBeTrue();
            }

            [Test]
            public void DifferentStringPathIsNotEqual()
            {
                // Given, When
                FilePath first = new FilePath("shaders/basic.vert");
                string second = "shaders/basic.frag";

                // Then
                (first == second).ShouldBeFalse();
            }

            [Test]
            public void AbsoluteStringAndPathAreNotEqual()
            {
                // Given, When
                FilePath first = new FilePath("shaders/basic.vert");
                string second = "/shaders/basic.frag";

                // Then
                (first == second).ShouldBeFalse();
            }

            [Test]
            public void StringAndAbsolutePathAreNotEqual()
            {
                // Given, When
                FilePath first = new FilePath("/shaders/basic.vert");
                string second = "shaders/basic.frag";

                // Then
                (first == second).ShouldBeFalse();
            }

            [Test]
            public void BothNullAreEqual()
            {
                // Given, When
                FilePath first = null;

                // Then
                (first == null).ShouldBeTrue();
            }
        }
    }
}
