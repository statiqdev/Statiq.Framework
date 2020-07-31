using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.IO
{
    [TestFixture]
    public class VirtualInputDirectoryFixture : BaseFixture
    {
        public class ConstructorTests : VirtualInputDirectoryFixture
        {
            [Test]
            public void ThrowsForNullFileSystem()
            {
                // Given, When, Then
                Assert.Throws<ArgumentNullException>(() => new VirtualInputDirectory(null, new NormalizedPath("A")));
            }

            [Test]
            public void ThrowsForNullDirectoryPath()
            {
                // Given, When, Then
                Assert.Throws<ArgumentNullException>(() => new VirtualInputDirectory(new TestFileSystem(), null));
            }

            [Test]
            public void ThrowsForNonRelativePath()
            {
                // Given, When, Then
                Assert.Throws<ArgumentException>(() => new VirtualInputDirectory(new TestFileSystem(), new NormalizedPath("/A")));
            }
        }

        public class GetDirectoriesTests : VirtualInputDirectoryFixture
        {
            [TestCase(".", SearchOption.AllDirectories, new[] { "c", "c/1", "d", "a", "a/b" })]
            [TestCase(".", SearchOption.TopDirectoryOnly, new[] { "c", "d", "a" })]
            public void RootVirtualDirectoryDoesNotIncludeSelf(string virtualPath, SearchOption searchOption, string[] expectedPaths)
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(virtualPath);

                // When
                IEnumerable<IDirectory> directories = directory.GetDirectories(searchOption);

                // Then
                CollectionAssert.AreEquivalent(expectedPaths, directories.Select(x => x.Path.FullPath));
            }

            [TestCase("a", SearchOption.AllDirectories, new[] { "a/b" })]
            [TestCase("a", SearchOption.TopDirectoryOnly, new[] { "a/b" })]
            public void NonRootVirtualDirectoryIncludesSelf(string virtualPath, SearchOption searchOption, string[] expectedPaths)
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(virtualPath);

                // When
                IEnumerable<IDirectory> directories = directory.GetDirectories(searchOption);

                // Then
                CollectionAssert.AreEquivalent(expectedPaths, directories.Select(x => x.Path.FullPath));
            }
        }

        public class GetFilesTests : VirtualInputDirectoryFixture
        {
            [TestCase(".", SearchOption.AllDirectories, new[] { "/a/b/c/foo.txt", "/a/b/c/1/2.txt", "/a/b/d/baz.txt", "/foo/baz.txt", "/foo/c/baz.txt" })]
            [TestCase(".", SearchOption.TopDirectoryOnly, new[] { "/foo/baz.txt" })]
            [TestCase("c", SearchOption.AllDirectories, new[] { "/a/b/c/foo.txt", "/a/b/c/1/2.txt", "/foo/c/baz.txt" })]
            [TestCase("c", SearchOption.TopDirectoryOnly, new[] { "/a/b/c/foo.txt", "/foo/c/baz.txt" })]
            public void GetsFiles(string virtualPath, SearchOption searchOption, string[] expectedPaths)
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(virtualPath);

                // When
                IEnumerable<IFile> files = directory.GetFiles(searchOption);

                // Then
                CollectionAssert.AreEquivalent(expectedPaths, files.Select(x => x.Path.FullPath));
            }
        }

        public class GetFileTests : VirtualInputDirectoryFixture
        {
            [TestCase(".", "c/foo.txt", "/a/b/c/foo.txt", true)]
            [TestCase(".", "baz.txt", "/foo/baz.txt", true)]
            [TestCase("c", "foo.txt", "/a/b/c/foo.txt", true)]
            [TestCase("c", "1/2.txt", "/a/b/c/1/2.txt", true)]
            [TestCase("c", "1/3.txt", "/foo/c/1/3.txt", false)]
            [TestCase("c", "baz.txt", "/foo/c/baz.txt", true)]
            [TestCase("c", "bar.txt", "/foo/c/bar.txt", false)]
            [TestCase("x/y/z", "bar.txt", "/foo/x/y/z/bar.txt", false)]
            public void GetsInputFile(string virtualPath, string filePath, string expectedPath, bool expectedExists)
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(virtualPath);

                // When
                IFile file = directory.GetFile(filePath);

                // Then
                Assert.AreEqual(expectedPath, file.Path.FullPath);
                Assert.AreEqual(expectedExists, file.Exists);
            }

            [Test]
            public void GetsInputFileAboveInputDirectory()
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("b/d");
                fileSystem.InputPaths.Add("/foo");
                VirtualInputDirectory directory = new VirtualInputDirectory(fileSystem, ".");

                // When
                IFile file = directory.GetFile("../c/foo.txt");

                // Then
                Assert.AreEqual("/a/b/c/foo.txt", file.Path.FullPath);
            }

            [Test]
            public void ThrowsForNullPath()
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(".");

                // When, Then
                Should.Throw<ArgumentNullException>(() => directory.GetFile(null));
            }

            [Test]
            public void ThrowsForAbsolutePath()
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(".");
                NormalizedPath filePath = "/a/test.txt";

                // When, Then
                Should.Throw<ArgumentException>(() => directory.GetFile(filePath));
            }
        }

        public class GetDirectoryTests : VirtualInputDirectoryFixture
        {
            [TestCase("a/b", "..", "a")]
            [TestCase("a/b/", "..", "a")]
            [TestCase("a/b/../c", "..", "a")]
            [TestCase(".", "..", "..")]
            [TestCase("a", "..", "")]
            [TestCase("a/b", "c", "a/b/c")]
            public void ShouldReturnDirectory(string virtualPath, string path, string expected)
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(virtualPath);

                // When
                IDirectory result = directory.GetDirectory(path);

                // Then
                Assert.AreEqual(expected, result.Path.FullPath);
            }

            [Test]
            public void ThrowsForNullPath()
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(".");

                // When, Then
                Should.Throw<ArgumentNullException>(() => directory.GetDirectory(null));
            }

            [Test]
            public void ThrowsForAbsolutePath()
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(".");
                NormalizedPath directoryPath = "/a/b";

                // When, Then
                Should.Throw<ArgumentException>(() => directory.GetDirectory(directoryPath));
            }
        }

        public class GetParentTests : VirtualInputDirectoryFixture
        {
            [TestCase("a/b", "a")]
            [TestCase("a/b/", "a")]
            [TestCase(".", "")]
            [TestCase("a", "")]
            public void ShouldReturnParentDirectory(string virtualPath, string expected)
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(virtualPath);

                // When
                IDirectory result = directory.Parent;

                // Then
                Assert.AreEqual(expected, result?.Path.FullPath);
            }
        }

        public class ExistsTests : VirtualInputDirectoryFixture
        {
            [TestCase(".")]
            [TestCase("c")]
            [TestCase("c/1")]
            [TestCase("a/b")]
            public void ShouldReturnTrueForExistingPaths(string virtualPath)
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(virtualPath);

                // When
                bool exists = directory.Exists;

                // Then
                exists.ShouldBeTrue();
            }

            [TestCase("x")]
            [TestCase("bar")]
            [TestCase("baz")]
            [TestCase("a/b/c")]
            [TestCase("q/w/e")]
            public void ShouldReturnFalseForNonExistingPaths(string virtualPath)
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(virtualPath);

                // When
                bool exists = directory.Exists;

                // Then
                exists.ShouldBeFalse();
            }

            [TestCase("a")]
            [TestCase("a/b")]
            public void ShouldReturnFalseForExcludedPath(string excluded)
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.ExcludedPaths.Add(excluded);
                VirtualInputDirectory directory = GetVirtualInputDirectory("a/b", fileSystem);

                // When
                bool exists = directory.Exists;

                // Then
                exists.ShouldBeFalse();
            }
        }

        public class CreateTests : VirtualInputDirectoryFixture
        {
            [Test]
            public void ShouldThrow()
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem();
                VirtualInputDirectory directory = new VirtualInputDirectory(fileSystem, ".");

                // When, Then
                Should.Throw<NotSupportedException>(() => directory.Create());
            }
        }

        public class DeleteTests : VirtualInputDirectoryFixture
        {
            [Test]
            public void ShouldThrow()
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem();
                VirtualInputDirectory directory = new VirtualInputDirectory(fileSystem, ".");

                // When, Then
                Should.Throw<NotSupportedException>(() => directory.Delete(false));
            }
        }

        private VirtualInputDirectory GetVirtualInputDirectory(string path) =>
            GetVirtualInputDirectory(path, new TestFileSystem(GetFileProvider()));

        private VirtualInputDirectory GetVirtualInputDirectory(string path, TestFileSystem fileSystem)
        {
            fileSystem.RootPath = "/a";
            fileSystem.InputPaths.Add("b");
            fileSystem.InputPaths.Add("/foo");
            return new VirtualInputDirectory(fileSystem, path);
        }

        private IFileProvider GetFileProvider()
        {
            TestFileProvider fileProvider = new TestFileProvider
            {
                { "/a/b/c/foo.txt" },
                { "/a/b/c/baz.txt" },
                { "/a/b/c/1/2.txt" },
                { "/a/b/d/baz.txt" },
                { "/a/x/bar.txt" },
                { "/foo/baz.txt" },
                { "/foo/c/baz.txt" },
                { "/bar/baz.txt" },
            };

            // Directories that don't exist as files
            fileProvider.AddDirectory("/a/y/z");
            fileProvider.AddDirectory("/foo/a/b");

            return fileProvider;
        }
    }
}
