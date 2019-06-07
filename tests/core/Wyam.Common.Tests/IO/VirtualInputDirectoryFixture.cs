using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.IO;
using Wyam.Testing;
using Wyam.Testing.IO;

namespace Wyam.Common.Tests.IO
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class VirtualInputDirectoryFixture : BaseFixture
    {
        public class ConstructorTests : VirtualInputDirectoryFixture
        {
            [Test]
            public void ThrowsForNullFileSystem()
            {
                // Given, When, Then
                Assert.Throws<ArgumentNullException>(() => new VirtualInputDirectory(null, new DirectoryPath("A")));
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
                Assert.Throws<ArgumentException>(() => new VirtualInputDirectory(new TestFileSystem(), new DirectoryPath("/A")));
            }
        }

        public class GetDirectoriesTests : VirtualInputDirectoryFixture
        {
            [TestCase(".", SearchOption.AllDirectories, new[] { "c", "c/1", "d", "a", "a/b" })]
            [TestCase(".", SearchOption.TopDirectoryOnly, new[] { "c", "d", "a" })]
            public async Task RootVirtualDirectoryDoesNotIncludeSelf(string virtualPath, SearchOption searchOption, string[] expectedPaths)
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(virtualPath);

                // When
                IEnumerable<IDirectory> directories = await directory.GetDirectoriesAsync(searchOption);

                // Then
                CollectionAssert.AreEquivalent(expectedPaths, directories.Select(x => x.Path.FullPath));
            }

            [TestCase("a", SearchOption.AllDirectories, new[] { "a/b" })]
            [TestCase("a", SearchOption.TopDirectoryOnly, new[] { "a/b" })]
            public async Task NonRootVirtualDirectoryIncludesSelf(string virtualPath, SearchOption searchOption, string[] expectedPaths)
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(virtualPath);

                // When
                IEnumerable<IDirectory> directories = await directory.GetDirectoriesAsync(searchOption);

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
            public async Task GetsFiles(string virtualPath, SearchOption searchOption, string[] expectedPaths)
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(virtualPath);

                // When
                IEnumerable<IFile> files = await directory.GetFilesAsync(searchOption);

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
            public async Task GetsInputFile(string virtualPath, string filePath, string expectedPath, bool expectedExists)
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(virtualPath);

                // When
                IFile file = await directory.GetFileAsync(filePath);

                // Then
                Assert.AreEqual(expectedPath, file.Path.FullPath);
                Assert.AreEqual(expectedExists, await file.GetExistsAsync());
            }

            [Test]
            public async Task GetsInputFileAboveInputDirectory()
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem();
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("b/d");
                fileSystem.InputPaths.Add("alt:///foo");
                fileSystem.FileProviders.Add(NormalizedPath.DefaultFileProvider.Scheme, GetFileProviderA());
                fileSystem.FileProviders.Add("alt", GetFileProviderB());
                VirtualInputDirectory directory = new VirtualInputDirectory(fileSystem, ".");

                // When
                IFile file = await directory.GetFileAsync("../c/foo.txt");

                // Then
                Assert.AreEqual("/a/b/c/foo.txt", file.Path.FullPath);
            }

            [Test]
            public async Task ThrowsForNullPath()
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(".");

                // When, Then
                await Should.ThrowAsync<ArgumentNullException>(async () => await directory.GetFileAsync(null));
            }

            [Test]
            public async Task ThrowsForAbsolutePath()
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(".");
                FilePath filePath = "/a/test.txt";

                // When, Then
                await Should.ThrowAsync<ArgumentException>(async () => await directory.GetFileAsync(filePath));
            }
        }

        public class GetDirectoryTests : VirtualInputDirectoryFixture
        {
            [TestCase("a/b", "..", "a")]
            [TestCase("a/b/", "..", "a")]
            [TestCase("a/b/../c", "..", "a")]
            [TestCase(".", "..", ".")]
            [TestCase("a", "..", "a")]
            [TestCase("a/b", "c", "a/b/c")]
            public async Task ShouldReturnDirectory(string virtualPath, string path, string expected)
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(virtualPath);

                // When
                IDirectory result = await directory.GetDirectoryAsync(path);

                // Then
                Assert.AreEqual(expected, result.Path.FullPath);
            }

            [Test]
            public async Task ThrowsForNullPath()
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(".");

                // When, Then
                await Should.ThrowAsync<ArgumentNullException>(async () => await directory.GetDirectoryAsync(null));
            }

            [Test]
            public async Task ThrowsForAbsolutePath()
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(".");
                DirectoryPath directoryPath = "/a/b";

                // When, Then
                await Should.ThrowAsync<ArgumentException>(async () => await directory.GetDirectoryAsync(directoryPath));
            }
        }

        public class GetParentTests : VirtualInputDirectoryFixture
        {
            [TestCase("a/b", "a")]
            [TestCase("a/b/", "a")]
            [TestCase(".", null)]
            [TestCase("a", null)]
            public async Task ShouldReturnParentDirectory(string virtualPath, string expected)
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(virtualPath);

                // When
                IDirectory result = await directory.GetParentAsync();

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
            public async Task ShouldReturnTrueForExistingPaths(string virtualPath)
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(virtualPath);

                // When
                bool exists = await directory.GetExistsAsync();

                // Then
                exists.ShouldBeTrue();
            }

            [TestCase("x")]
            [TestCase("bar")]
            [TestCase("baz")]
            [TestCase("a/b/c")]
            [TestCase("q/w/e")]
            public async Task ShouldReturnFalseForNonExistingPaths(string virtualPath)
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(virtualPath);

                // When
                bool exists = await directory.GetExistsAsync();

                // Then
                exists.ShouldBeFalse();
            }
        }

        public class CreateTests : VirtualInputDirectoryFixture
        {
            [Test]
            public async Task ShouldThrow()
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem();
                VirtualInputDirectory directory = new VirtualInputDirectory(fileSystem, ".");

                // When, Then
                await Should.ThrowAsync<NotSupportedException>(async () => await directory.CreateAsync());
            }
        }

        public class DeleteTests : VirtualInputDirectoryFixture
        {
            [Test]
            public async Task ShouldThrow()
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem();
                VirtualInputDirectory directory = new VirtualInputDirectory(fileSystem, ".");

                // When, Then
                await Should.ThrowAsync<NotSupportedException>(async () => await directory.DeleteAsync(false));
            }
        }

        private VirtualInputDirectory GetVirtualInputDirectory(string path)
        {
            TestFileSystem fileSystem = new TestFileSystem();
            fileSystem.RootPath = "/a";
            fileSystem.InputPaths.Add("b");
            fileSystem.InputPaths.Add("alt:///foo");
            fileSystem.FileProviders.Add(NormalizedPath.DefaultFileProvider.Scheme, GetFileProviderA());
            fileSystem.FileProviders.Add("alt", GetFileProviderB());
            return new VirtualInputDirectory(fileSystem, path);
        }

        private IFileProvider GetFileProviderA()
        {
            TestFileProvider fileProvider = new TestFileProvider();

            fileProvider.AddDirectory("/a");
            fileProvider.AddDirectory("/a/b");
            fileProvider.AddDirectory("/a/b/c");
            fileProvider.AddDirectory("/a/b/c/1");
            fileProvider.AddDirectory("/a/b/d");
            fileProvider.AddDirectory("/a/x");
            fileProvider.AddDirectory("/a/y");
            fileProvider.AddDirectory("/a/y/z");

            fileProvider.AddFile("/a/b/c/foo.txt");
            fileProvider.AddFile("/a/b/c/baz.txt");
            fileProvider.AddFile("/a/b/c/1/2.txt");
            fileProvider.AddFile("/a/b/d/baz.txt");
            fileProvider.AddFile("/a/x/bar.txt");

            return fileProvider;
        }

        private IFileProvider GetFileProviderB()
        {
            TestFileProvider fileProvider = new TestFileProvider();

            fileProvider.AddDirectory("/foo");
            fileProvider.AddDirectory("/foo/a");
            fileProvider.AddDirectory("/foo/a/b");
            fileProvider.AddDirectory("/foo/c");
            fileProvider.AddDirectory("/bar");

            fileProvider.AddFile("/foo/baz.txt");
            fileProvider.AddFile("/foo/c/baz.txt");
            fileProvider.AddFile("/bar/baz.txt");

            return fileProvider;
        }
    }
}
