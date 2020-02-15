using System;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.IO
{
    [TestFixture]
    public class NormalizedPathFixture : BaseFixture
    {
        public class ConstructorTests : NormalizedPathFixture
        {
            [Test]
            public void ShouldThrowIfPathIsNull()
            {
                // Given, When, Then
                Should.Throw<ArgumentNullException>(() => new NormalizedPath(null));
            }

            [TestCase("")]
            [TestCase("\t ")]
            public void ShouldThrowIfPathIsEmpty(string fullPath)
            {
                // Given, When, Then
                Should.Throw<ArgumentException>(() => new NormalizedPath(fullPath));
            }

            [Test]
            public void CurrentDirectoryReturnsDot()
            {
                // Given, When
                NormalizedPath path = new NormalizedPath("./");

                // Then
                path.FullPath.ShouldBe(".");
            }

            [Test]
            public void ShouldNormalizePathSeparators()
            {
                // Given, When
                NormalizedPath path = new NormalizedPath("shaders\\basic");

                // Then
                path.FullPath.ShouldBe("shaders/basic");
            }

            [Test]
            public void ShouldTrimWhiteSpaceFromPathAndLeaveSpaces()
            {
                // Given, When
                NormalizedPath path = new NormalizedPath("\t\r\nshaders/basic ");

                // Then
                path.FullPath.ShouldBe("shaders/basic ");
            }

            [Test]
            public void ShouldNotRemoveWhiteSpaceWithinPath()
            {
                // Given, When
                NormalizedPath path = new NormalizedPath("my awesome shaders/basic");

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
                NormalizedPath path = new NormalizedPath(value);

                // Then
                path.FullPath.ShouldBe(expected);
            }

            [TestCase("\\")]
            [TestCase("/")]
            public void ShouldNotRemoveSingleTrailingSlash(string value)
            {
                // Given, When
                NormalizedPath path = new NormalizedPath(value);

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
                NormalizedPath path = new NormalizedPath(value);

                // Then
                path.FullPath.ShouldBe(expected);
            }

            [TestCase("\\")]
            [TestCase("/")]
            public void ShouldNotRemoveOnlyRelativePart(string value)
            {
                // Given, When
                NormalizedPath path = new NormalizedPath(value);

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
                NormalizedPath path = new NormalizedPath(pathName);

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
                NormalizedPath path = new NormalizedPath(expected);

                // Then
                path.FullPath.ShouldBe(expected);
            }

            [Test]
            public void ShouldReturnFullPathForInferedAbsolutePath()
            {
                // Given, When
                const string expected = "/shaders/basic";
                NormalizedPath path = new NormalizedPath(expected);

                // Then
                path.FullPath.ShouldBe(expected);
            }

            [Test]
            public void ShouldReturnFullPathForExplicitAbsolutePath()
            {
                // Given, When
                const string expected = "shaders/basic";
                NormalizedPath path = new NormalizedPath(expected, PathKind.Absolute);

                // Then
                path.FullPath.ShouldBe(expected);
            }

            [WindowsTest]
            public void ShouldNotPrependSlashForRootedPath()
            {
                // Given, When
                NormalizedPath path = new NormalizedPath("C:/shaders/basic");

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
                NormalizedPath path = new NormalizedPath(fullPath);

                // When
                NormalizedPath root = path.Root;

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
                NormalizedPath path = new NormalizedPath(fullPath, PathKind.Relative);

                // When
                NormalizedPath root = path.Root;

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
                NormalizedPath path = new NormalizedPath(fullPath);

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
                NormalizedPath path = new NormalizedPath(fullPath);

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
                NormalizedPath testPath = new NormalizedPath(path);

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
                (string, ReadOnlyMemory<char>[]) fullPathAndSegments = Common.NormalizedPath.GetFullPathAndSegments(fullPath.AsSpan());

                // Then
                fullPathAndSegments.Item1.ShouldBe(expectedFullPath);
                fullPathAndSegments.Item2.ToStrings().ShouldBe(expectedSegments ?? expectedFullPath.Split('/', StringSplitOptions.RemoveEmptyEntries));
            }

            [Test]
            public void SegmentsShouldBeEmptyForRoot()
            {
                // Given, When
                (string, ReadOnlyMemory<char>[]) fullPathAndSegments = Common.NormalizedPath.GetFullPathAndSegments("/".AsSpan());

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
                NormalizedPath path = new NormalizedPath("shaders/basic.vert");

                // Then
                path.Equals(path, comparisonType).ShouldBeTrue();
            }

            [TestCase(StringComparison.Ordinal)]
            [TestCase(StringComparison.OrdinalIgnoreCase)]
            public void PathsAreInequalIfAnyIsNull(StringComparison comparisonType)
            {
                // Given, When
                bool result = new NormalizedPath("test.txt").Equals(null, comparisonType);

                // Then
                result.ShouldBeFalse();
            }

            [TestCase(StringComparison.Ordinal)]
            [TestCase(StringComparison.OrdinalIgnoreCase)]
            public void SamePathsAreEqual(StringComparison comparisonType)
            {
                // Given, When
                NormalizedPath first = new NormalizedPath("shaders/basic.vert");
                NormalizedPath second = new NormalizedPath("shaders/basic.vert");

                // Then
                first.Equals(second, comparisonType).ShouldBeTrue();
                second.Equals(first, comparisonType).ShouldBeTrue();
            }

            [TestCase(StringComparison.Ordinal)]
            [TestCase(StringComparison.OrdinalIgnoreCase)]
            public void DifferentPathsAreNotEqual(StringComparison comparisonType)
            {
                // Given, When
                NormalizedPath first = new NormalizedPath("shaders/basic.vert");
                NormalizedPath second = new NormalizedPath("shaders/basic.frag");

                // Then
                first.Equals(second, comparisonType).ShouldBeFalse();
                second.Equals(first, comparisonType).ShouldBeFalse();
            }

            [TestCase(StringComparison.Ordinal, false)]
            [TestCase(StringComparison.OrdinalIgnoreCase, true)]
            public void SamePathsButDifferentCasingFollowComparison(StringComparison comparisonType, bool expected)
            {
                // Given
                NormalizedPath first = new NormalizedPath("shaders/basic.vert");
                NormalizedPath second = new NormalizedPath("SHADERS/BASIC.VERT");

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
                NormalizedPath first = new NormalizedPath("shaders/basic.vert");
                NormalizedPath second = new NormalizedPath("shaders/basic.vert");

                // Then
                first.GetHashCode(comparisonType).ShouldBe(second.GetHashCode(comparisonType));
            }

            [TestCase(StringComparison.Ordinal)]
            [TestCase(StringComparison.OrdinalIgnoreCase)]
            public void DifferentPathsGetDifferentHashCodes(StringComparison comparisonType)
            {
                // Given, When
                NormalizedPath first = new NormalizedPath("shaders/basic.vert");
                NormalizedPath second = new NormalizedPath("shaders/basic.frag");

                // Then
                first.GetHashCode(comparisonType).ShouldNotBe(second.GetHashCode(comparisonType));
            }

            [TestCase(StringComparison.Ordinal, false)]
            [TestCase(StringComparison.OrdinalIgnoreCase, true)]
            public void SamePathsButDifferentCasingFollowComparison(StringComparison comparisonType, bool expected)
            {
                // Given
                NormalizedPath first = new NormalizedPath("shaders/basic.vert");
                NormalizedPath second = new NormalizedPath("SHADERS/BASIC.VERT");

                // When
                bool result = first.GetHashCode(comparisonType).Equals(second.GetHashCode(comparisonType));

                // Then
                result.ShouldBe(expected);
            }
        }

        public class EqualityOperatorTests : NormalizedPathFixture
        {
            [Test]
            public void PathsAreInequalIfAnyIsNull()
            {
                // Given, When
                NormalizedPath result = new NormalizedPath("test.txt");

                // Then
                result.IsNull.ShouldBeFalse();
            }

            [Test]
            public void SamePathsAreEqual()
            {
                // Given, When
                NormalizedPath first = new NormalizedPath("shaders/basic.vert");
                NormalizedPath second = new NormalizedPath("shaders/basic.vert");

                // Then
                (first == second).ShouldBeTrue();
                (second == first).ShouldBeTrue();
            }

            [Test]
            public void DifferentPathsAreNotEqual()
            {
                // Given, When
                NormalizedPath first = new NormalizedPath("shaders/basic.vert");
                NormalizedPath second = new NormalizedPath("shaders/basic.frag");

                // Then
                (first == second).ShouldBeFalse();
                (second == first).ShouldBeFalse();
            }

            [Test]
            public void StringPathsAreEqual()
            {
                // Given, When
                NormalizedPath first = new NormalizedPath("shaders/basic.vert");
                string second = "shaders/basic.vert";

                // Then
                (first == second).ShouldBeTrue();
            }

            [Test]
            public void DifferentStringPathIsNotEqual()
            {
                // Given, When
                NormalizedPath first = new NormalizedPath("shaders/basic.vert");
                string second = "shaders/basic.frag";

                // Then
                (first == second).ShouldBeFalse();
            }

            [Test]
            public void AbsoluteStringAndPathAreNotEqual()
            {
                // Given, When
                NormalizedPath first = new NormalizedPath("shaders/basic.vert");
                string second = "/shaders/basic.frag";

                // Then
                (first == second).ShouldBeFalse();
            }

            [Test]
            public void StringAndAbsolutePathAreNotEqual()
            {
                // Given, When
                NormalizedPath first = new NormalizedPath("/shaders/basic.vert");
                string second = "shaders/basic.frag";

                // Then
                (first == second).ShouldBeFalse();
            }

            [Test]
            public void BothNullAreEqual()
            {
                // Given, When
                NormalizedPath first = null;

                // Then
                first.IsNull.ShouldBeTrue();
            }
        }
        public class HasExtensionTests : NormalizedPathFixture
        {
            [TestCase("assets/shaders/basic.txt", true)]
            [TestCase("assets/shaders/basic", false)]
            [TestCase("assets/shaders/basic/", false)]
            public void CanSeeIfAPathHasAnExtension(string fullPath, bool expected)
            {
                // Given, When
                NormalizedPath path = new NormalizedPath(fullPath);

                // Then
                Assert.AreEqual(expected, path.HasExtension);
            }
        }

        public class ExtensionTests : NormalizedPathFixture
        {
            [TestCase("assets/shaders/basic.frag", ".frag")]
            [TestCase("assets/shaders/basic.frag/test.vert", ".vert")]
            [TestCase("assets/shaders/basic", null)]
            [TestCase("assets/shaders/basic.frag/test", null)]
            public void CanGetExtension(string fullPath, string expected)
            {
                // Given
                NormalizedPath result = new NormalizedPath(fullPath);

                // When
                string extension = result.Extension;

                // Then
                Assert.AreEqual(expected, extension);
            }
        }

        public class DirectoryTests : NormalizedPathFixture
        {
            [Test]
            public void CanGetDirectoryForFilePath()
            {
                // Given
                NormalizedPath path = new NormalizedPath("temp/hello.txt");

                // When
                NormalizedPath directory = path.Parent;

                // Then
                Assert.AreEqual("temp", directory.FullPath);
            }

            [Test]
            public void CanGetDirectoryForFilePathInRoot()
            {
                // Given
                NormalizedPath path = new NormalizedPath("hello.txt");

                // When
                NormalizedPath directory = path.Parent;

                // Then
                Assert.AreEqual(".", directory.FullPath);
            }
        }

        public class RootRelativeTests : NormalizedPathFixture
        {
            [TestCase(@"\a\b\c", "a/b/c")]
            [TestCase("/a/b/c", "a/b/c")]
            [TestCase("a/b/c", "a/b/c")]
            [TestCase(@"a\b\c", "a/b/c")]
            [TestCase("foo.txt", "foo.txt")]
            [TestCase("foo", "foo")]
            [WindowsTestCase(@"c:\a\b\c", "a/b/c")]
            [WindowsTestCase("c:/a/b/c", "a/b/c")]
            public void ShouldReturnRootRelativePath(string fullPath, string expected)
            {
                // Given
                NormalizedPath path = new NormalizedPath(fullPath);

                // When
                NormalizedPath rootRelative = path.RootRelative;

                // Then
                Assert.AreEqual(expected, rootRelative.FullPath);
            }

            [TestCase(@"\a\b\c")]
            [TestCase("/a/b/c")]
            [TestCase("a/b/c")]
            [TestCase(@"a\b\c")]
            [TestCase("foo.txt")]
            [TestCase("foo")]
            [WindowsTestCase(@"c:\a\b\c")]
            [WindowsTestCase("c:/a/b/c")]
            public void ShouldReturnSelfForExplicitRelativePath(string fullPath)
            {
                // Given
                NormalizedPath path = new NormalizedPath(fullPath, PathKind.Relative);

                // When
                NormalizedPath rootRelative = path.RootRelative;

                // Then
                Assert.AreEqual(path.FullPath, rootRelative.FullPath);
            }
        }

        public class ChangeExtensionTests : NormalizedPathFixture
        {
            [TestCase(".dat", "temp/hello.dat")]
            [TestCase("dat", "temp/hello.dat")]
            [TestCase(".txt", "temp/hello.txt")]
            [TestCase("txt", "temp/hello.txt")]
            [TestCase("", "temp/hello.")]
            [TestCase(null, "temp/hello")]
            public void ShouldChangeExtension(string extension, string expected)
            {
                // Given
                NormalizedPath path = new NormalizedPath("temp/hello.txt");

                // When
                path = path.ChangeExtension(extension);

                // Then
                Assert.AreEqual(expected, path.ToString());
            }
        }

        public class AppendExtensionTests : NormalizedPathFixture
        {
            [Test]
            public void ShouldThrowIfExtensionIsNull()
            {
                // Given
                NormalizedPath path = new NormalizedPath("temp/hello.txt");

                // When
                TestDelegate test = () => path.AppendExtension(null);

                // Then
                Assert.Throws<ArgumentNullException>(test);
            }

            [TestCase("dat", "temp/hello.txt.dat")]
            [TestCase(".dat", "temp/hello.txt.dat")]
            public void CanAppendExtensionToPath(string extension, string expected)
            {
                // Given
                NormalizedPath path = new NormalizedPath("temp/hello.txt");

                // When
                path = path.AppendExtension(extension);

                // Then
                Assert.AreEqual(expected, path.ToString());
            }
        }

        public class InsertSuffixTests : NormalizedPathFixture
        {
            [Test]
            public void ShouldThrowIfSuffixIsNull()
            {
                // Given
                NormalizedPath path = new NormalizedPath("temp/hello.txt");

                // When
                TestDelegate test = () => path.InsertSuffix(null);

                // Then
                Assert.Throws<ArgumentNullException>(test);
            }

            [TestCase("temp/hello.txt", "123", "temp/hello123.txt")]
            [TestCase("/hello.txt", "123", "/hello123.txt")]
            [TestCase("temp/hello", "123", "temp/hello123")]
            [TestCase("temp/hello.txt.dat", "123", "temp/hello.txt123.dat")]
            public void CanInsertSuffixToPath(string path, string suffix, string expected)
            {
                // Given
                NormalizedPath filePath = new NormalizedPath(path);

                // When
                filePath = filePath.InsertSuffix(suffix);

                // Then
                Assert.AreEqual(expected, filePath.FullPath);
            }
        }

        public class InserPrefixTests : NormalizedPathFixture
        {
            [Test]
            public void ShouldThrowIfPRefixIsNull()
            {
                // Given
                NormalizedPath path = new NormalizedPath("temp/hello.txt");

                // When
                TestDelegate test = () => path.InsertPrefix(null);

                // Then
                Assert.Throws<ArgumentNullException>(test);
            }

            [TestCase("temp/hello.txt", "123", "temp/123hello.txt")]
            [TestCase("/hello.txt", "123", "/123hello.txt")]
            [TestCase("hello.txt", "123", "123hello.txt")]
            [TestCase("temp/hello", "123", "temp/123hello")]
            [TestCase("temp/hello.txt.dat", "123", "temp/123hello.txt.dat")]
            public void CanInsertPrefixToPath(string path, string prefix, string expected)
            {
                // Given
                NormalizedPath filePath = new NormalizedPath(path);

                // When
                filePath = filePath.InsertPrefix(prefix);

                // Then
                Assert.AreEqual(expected, filePath.FullPath);
            }
        }

        public class FileNameTests : NormalizedPathFixture
        {
            [Test]
            public void CanGetFilenameFromPath()
            {
                // Given
                NormalizedPath path = new NormalizedPath("/input/test.txt");

                // When
                NormalizedPath result = path.FileName;

                // Then
                Assert.AreEqual("test.txt", result.FullPath);
            }

            [Test]
            public void GetsFileNameIfJustFileName()
            {
                // Given
                NormalizedPath path = new NormalizedPath("test.txt");

                // When
                NormalizedPath result = path.FileName;

                // Then
                Assert.AreEqual("test.txt", result.FullPath);
            }
        }

        public class FileNameWithoutExtensionTests : NormalizedPathFixture
        {
            [TestCase("/input/test.txt", "test")]
            [TestCase("/input/test", "test")]
            [TestCase("test.txt", "test")]
            [TestCase("test", "test")]
            public void ShouldReturnFilenameWithoutExtensionFromPath(string fullPath, string expected)
            {
                // Given
                NormalizedPath path = new NormalizedPath(fullPath);

                // When
                NormalizedPath result = path.FileNameWithoutExtension;

                // Then
                Assert.AreEqual(expected, result.FullPath);
            }

            [TestCase("/input/.test")]
            [TestCase(".test")]
            public void ShouldReturnNullIfOnlyExtension(string fullPath)
            {
                // Given
                NormalizedPath path = new NormalizedPath(fullPath);

                // When
                NormalizedPath result = path.FileNameWithoutExtension;

                // Then
                Assert.IsNull(result);
            }
        }

        public class ChangeFileNameTests : NormalizedPathFixture
        {
            [TestCase("/input/test.txt", "foo.bar", "/input/foo.bar")]
            [TestCase("/input/test", "foo.bar", "/input/foo.bar")]
            [TestCase("input/test.txt", "foo.bar", "input/foo.bar")]
            [TestCase("input/test", "foo.bar", "input/foo.bar")]
            [TestCase("/test.txt", "foo.bar", "/foo.bar")]
            [TestCase("/test", "foo.bar", "/foo.bar")]
            [TestCase("test.txt", "foo.bar", "foo.bar")]
            [TestCase("test", "foo.bar", "foo.bar")]
            [TestCase("/input/test.txt", "foo", "/input/foo")]
            [TestCase("/input/test", "foo", "/input/foo")]
            [TestCase("input/test.txt", "foo", "input/foo")]
            [TestCase("input/test", "foo", "input/foo")]
            [TestCase("/test.txt", "foo", "/foo")]
            [TestCase("/test", "foo", "/foo")]
            [TestCase("test.txt", "foo", "foo")]
            [TestCase("test", "foo", "foo")]
            public void ShouldChangeFileName(string fullPath, string fileName, string expected)
            {
                // Given
                NormalizedPath path = new NormalizedPath(fullPath);

                // When
                NormalizedPath result = path.ChangeFileName(fileName);

                // Then
                result.FullPath.ShouldBe(expected);
            }
        }

        public class NameTests : NormalizedPathFixture
        {
            [TestCase("/a/b", "b")]
            [TestCase("/a/b/", "b")]
            [TestCase("/a/b/../c", "c")]
            [TestCase("/a/b/..", "a")]
            [TestCase("/a", "a")]
            [TestCase("/", "/")]
            [WindowsTestCase("C:/", "C:")]
            [WindowsTestCase("C:", "C:")]
            [WindowsTestCase("C:/Data", "Data")]
            [WindowsTestCase("C:/Data/Work", "Work")]
            [WindowsTestCase("C:/Data/Work/file.txt", "file.txt")]
            public void ShouldReturnDirectoryName(string directoryPath, string name)
            {
                // Given
                NormalizedPath path = new NormalizedPath(directoryPath);

                // When
                string result = path.Name;

                // Then
                Assert.AreEqual(name, result);
            }
        }

        public class ParentTests : NormalizedPathFixture
        {
            [TestCase("/a/b", "/a")]
            [TestCase("/a/b/", "/a")]
            [TestCase("/a/b/../c", "/a")]
            [TestCase("/a", "/")]
            [WindowsTestCase("C:/a/b", "C:/a")]
            [WindowsTestCase("C:/a", "C:/")]
            public void ReturnsParent(string directoryPath, string expected)
            {
                // Given
                NormalizedPath path = new NormalizedPath(directoryPath);

                // When
                NormalizedPath parent = path.Parent;

                // Then
                Assert.AreEqual(expected, parent.FullPath);
            }

            [TestCase(".")]
            [TestCase("/")]
            [TestCase("a")]
            [WindowsTestCase("C:")]
            public void RootDirectoryReturnsNullParent(string directoryPath)
            {
                // Given
                NormalizedPath path = new NormalizedPath(directoryPath);

                // When
                NormalizedPath parent = path.Parent;

                // Then
                Assert.IsNull(parent);
            }
        }

        public class GetFilePathTests : NormalizedPathFixture
        {
            [Test]
            public void ShouldThrowIfPathIsNull()
            {
                // Given
                NormalizedPath path = new NormalizedPath("assets");

                // When
                TestDelegate test = () => path.GetFilePath(null);

                // Then
                Assert.Throws<ArgumentNullException>(test);
            }

            [WindowsTestCase("c:/assets/shaders/", "simple.frag", "c:/assets/shaders/simple.frag")]
            [WindowsTestCase("c:/", "simple.frag", "c:/simple.frag")]
            [WindowsTestCase("c:/", "c:/simple.frag", "c:/simple.frag")]
            [WindowsTestCase("c:/", "c:/test/simple.frag", "c:/simple.frag")]
            [WindowsTestCase("c:/assets/shaders/", "test/simple.frag", "c:/assets/shaders/simple.frag")]
            [WindowsTestCase("c:/", "test/simple.frag", "c:/simple.frag")]
            [TestCase("assets/shaders", "simple.frag", "assets/shaders/simple.frag")]
            [TestCase("assets/shaders/", "simple.frag", "assets/shaders/simple.frag")]
            [TestCase("/assets/shaders/", "simple.frag", "/assets/shaders/simple.frag")]
            [TestCase("assets/shaders", "test/simple.frag", "assets/shaders/simple.frag")]
            [TestCase("assets/shaders", "/test/simple.frag", "assets/shaders/simple.frag")]
            [TestCase("assets/shaders/", "test/simple.frag", "assets/shaders/simple.frag")]
            [TestCase("assets/shaders/", "/test/simple.frag", "assets/shaders/simple.frag")]
            [TestCase("/assets/shaders/", "test/simple.frag", "/assets/shaders/simple.frag")]
            [TestCase("/assets/shaders/", "/test/simple.frag", "/assets/shaders/simple.frag")]
            public void ShouldCombinePaths(string first, string second, string expected)
            {
                // Given
                NormalizedPath path = new NormalizedPath(first);

                // When
                NormalizedPath result = path.GetFilePath(new NormalizedPath(second));

                // Then
                Assert.AreEqual(expected, result.FullPath);
            }
        }

        public class CombineFileTests : NormalizedPathFixture
        {
            [Test]
            public void ShouldThrowIfPathIsNull()
            {
                // Given
                NormalizedPath path = new NormalizedPath("assets");

                // When
                TestDelegate test = () => path.Combine(null);

                // Then
                Assert.Throws<ArgumentNullException>(test);
            }

            [WindowsTestCase("c:/assets/shaders/", "simple.frag", "c:/assets/shaders/simple.frag")]
            [WindowsTestCase("c:/", "simple.frag", "c:/simple.frag")]
            [WindowsTestCase("c:/assets/shaders/", "test/simple.frag", "c:/assets/shaders/test/simple.frag")]
            [WindowsTestCase("c:/", "test/simple.frag", "c:/test/simple.frag")]
            [WindowsTestCase("c:/", "c:/test/simple.frag", "c:/test/simple.frag")]
            [TestCase("assets/shaders", "simple.frag", "assets/shaders/simple.frag")]
            [TestCase("assets/shaders/", "simple.frag", "assets/shaders/simple.frag")]
            [TestCase("/assets/shaders/", "simple.frag", "/assets/shaders/simple.frag")]
            [TestCase("assets/shaders", "test/simple.frag", "assets/shaders/test/simple.frag")]
            [TestCase("assets/shaders/", "test/simple.frag", "assets/shaders/test/simple.frag")]
            [TestCase("/assets/shaders/", "test/simple.frag", "/assets/shaders/test/simple.frag")]
            [TestCase("assets", "/other/asset.txt", "/other/asset.txt")]
            [TestCase(".", "asset.txt", "asset.txt")]
            [TestCase(".", "other/asset.txt", "other/asset.txt")]
            [TestCase(".", "/other/asset.txt", "/other/asset.txt")]
            public void ShouldCombinePaths(string first, string second, string expected)
            {
                // Given
                NormalizedPath path = new NormalizedPath(first);

                // When
                NormalizedPath result = path.Combine(new NormalizedPath(second));

                // Then
                Assert.AreEqual(expected, result.FullPath);
            }
        }

        public class CombineTests : NormalizedPathFixture
        {
            [WindowsTestCase("c:/assets/shaders/", "simple", "c:/assets/shaders/simple")]
            [WindowsTestCase("c:/", "simple", "c:/simple")]
            [WindowsTestCase("c:/assets/shaders/", "c:/simple", "c:/simple")]
            [TestCase("assets/shaders", "simple", "assets/shaders/simple")]
            [TestCase("assets/shaders/", "simple", "assets/shaders/simple")]
            [TestCase("/assets/shaders/", "simple", "/assets/shaders/simple")]
            [TestCase("assets", "/other/assets", "/other/assets")]
            public void ShouldCombinePaths(string first, string second, string expected)
            {
                // Given
                NormalizedPath path = new NormalizedPath(first);

                // When
                NormalizedPath result = path.Combine(new NormalizedPath(second));

                // Then
                Assert.AreEqual(expected, result.FullPath);
            }

            [Test]
            public void ShouldThrowIfPathIsNull()
            {
                // Given
                NormalizedPath path = new NormalizedPath("assets");

                // When
                TestDelegate test = () => path.Combine(null);

                // Then
                Assert.Throws<ArgumentNullException>(test);
            }
        }

        public class ContainsChildTests : NormalizedPathFixture
        {
            [TestCase("/a/b/c", "/a/b/test.txt", false)]
            [TestCase("/a/b/c", "/a/b/c/test.txt", true)]
            [TestCase("/a/b/c", "/a/b/c/d/test.txt", false)]
            public void ShouldCheckFilePath(string directory, string path, bool expected)
            {
                // Given
                NormalizedPath directoryPath = new NormalizedPath(directory);
                NormalizedPath filePath = new NormalizedPath(path);

                // When
                bool result = directoryPath.ContainsChild(filePath);

                // Then
                result.ShouldBe(expected);
            }

            [TestCase("/a/b/c", "/a/b", false)]
            [TestCase("/a/b/c", "/a/b/c", false)]
            [TestCase("/a/b/c", "/a/b/c/d", true)]
            [TestCase("/a/b/c", "/a/b/c/d/e", false)]
            public void ShouldCheckDirectoryPath(string directory, string path, bool expected)
            {
                // Given
                NormalizedPath directoryPath = new NormalizedPath(directory);
                NormalizedPath filePath = new NormalizedPath(path);

                // When
                bool result = directoryPath.ContainsChild(filePath);

                // Then
                result.ShouldBe(expected);
            }
        }

        public class ContainsDescendantTests : NormalizedPathFixture
        {
            [TestCase("/a/b/c", "/a/b/test.txt", false)]
            [TestCase("/a/b/c", "/a/b/c/test.txt", true)]
            [TestCase("/a/b/c", "/a/b/c/d/test.txt", true)]
            public void ShouldCheckFilePath(string directory, string path, bool expected)
            {
                // Given
                NormalizedPath directoryPath = new NormalizedPath(directory);
                NormalizedPath filePath = new NormalizedPath(path);

                // When
                bool result = directoryPath.ContainsDescendant(filePath);

                // Then
                result.ShouldBe(expected);
            }

            [TestCase("/a/b/c", "/a/b", false)]
            [TestCase("/a/b/c", "/a/b/c", false)]
            [TestCase("/a/b/c", "/a/b/c/d", true)]
            [TestCase("/a/b/c", "/a/b/c/d/e", true)]
            public void ShouldCheckDirectoryPath(string directory, string path, bool expected)
            {
                // Given
                NormalizedPath directoryPath = new NormalizedPath(directory);
                NormalizedPath filePath = new NormalizedPath(path);

                // When
                bool result = directoryPath.ContainsDescendant(filePath);

                // Then
                result.ShouldBe(expected);
            }
        }
    }
}
