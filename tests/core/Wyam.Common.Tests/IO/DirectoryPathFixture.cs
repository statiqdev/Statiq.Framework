using System;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.IO;
using Wyam.Testing;
using Wyam.Testing.Attributes;

namespace Wyam.Common.Tests.IO
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class DirectoryPathFixture : BaseFixture
    {
        public class NameTests : DirectoryPathFixture
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
                DirectoryPath path = new DirectoryPath(directoryPath);

                // When
                string result = path.Name;

                // Then
                Assert.AreEqual(name, result);
            }
        }

        public class ParentTests : FilePathFixture
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
                DirectoryPath path = new DirectoryPath(directoryPath);

                // When
                DirectoryPath parent = path.Parent;

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
                DirectoryPath path = new DirectoryPath(directoryPath);

                // When
                DirectoryPath parent = path.Parent;

                // Then
                Assert.IsNull(parent);
            }
        }

        public class GetFilePathTests : DirectoryPathFixture
        {
            [Test]
            public void ShouldThrowIfPathIsNull()
            {
                // Given
                DirectoryPath path = new DirectoryPath("assets");

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
                DirectoryPath path = new DirectoryPath(first);

                // When
                FilePath result = path.GetFilePath(new FilePath(second));

                // Then
                Assert.AreEqual(expected, result.FullPath);
            }
        }

        public class RootRelativeTests : FilePathFixture
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
                DirectoryPath path = new DirectoryPath(fullPath);

                // When
                DirectoryPath rootRelative = path.RootRelative;

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
                DirectoryPath path = new DirectoryPath(fullPath, PathKind.Relative);

                // When
                DirectoryPath rootRelative = path.RootRelative;

                // Then
                Assert.AreEqual(path.FullPath, rootRelative.FullPath);
            }
        }

        public class CombineFileTests : DirectoryPathFixture
        {
            [Test]
            public void ShouldThrowIfPathIsNull()
            {
                // Given
                DirectoryPath path = new DirectoryPath("assets");

                // When
                TestDelegate test = () => path.CombineFile(null);

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
                DirectoryPath path = new DirectoryPath(first);

                // When
                FilePath result = path.CombineFile(new FilePath(second));

                // Then
                Assert.AreEqual(expected, result.FullPath);
            }
        }

        public class CombineTests : DirectoryPathFixture
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
                DirectoryPath path = new DirectoryPath(first);

                // When
                DirectoryPath result = path.Combine(new DirectoryPath(second));

                // Then
                Assert.AreEqual(expected, result.FullPath);
            }

            [Test]
            public void ShouldThrowIfPathIsNull()
            {
                // Given
                DirectoryPath path = new DirectoryPath("assets");

                // When
                TestDelegate test = () => path.Combine(null);

                // Then
                Assert.Throws<ArgumentNullException>(test);
            }
        }
    }
}
