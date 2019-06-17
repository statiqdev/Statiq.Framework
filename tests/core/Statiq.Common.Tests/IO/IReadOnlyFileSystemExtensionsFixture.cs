using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common.IO;
using Statiq.Testing;
using Statiq.Testing.IO;

namespace Statiq.Common.Tests.IO
{
    [TestFixture]
    public class IReadOnlyFileSystemExtensionsFixture : BaseFixture
    {
        public class GetInputFileTests : IReadOnlyFileSystemExtensionsFixture
        {
            [Test]
            [TestCase("foo.txt", "/a/b/c/foo.txt")]
            [TestCase("bar.txt", "/a/x/bar.txt")]
            [TestCase("baz.txt", "/a/y/baz.txt")]
            [TestCase("/a/b/c/foo.txt", "/a/b/c/foo.txt")]
            [TestCase("/z/baz.txt", "/z/baz.txt")]
            public async Task ReturnsInputFile(string input, string expected)
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("b/c");
                fileSystem.InputPaths.Add("b/d");
                fileSystem.InputPaths.Add("x");
                fileSystem.InputPaths.Add("y");

                // When
                IFile result = await fileSystem.GetInputFileAsync(input);

                // Then
                Assert.AreEqual(expected, result.Path.FullPath);
            }

            [Test]
            public async Task ReturnsInputFileAboveInputDirectory()
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("x/t");

                // When
                IFile result = await fileSystem.GetInputFileAsync("../bar.txt");

                // Then
                Assert.AreEqual("/a/x/bar.txt", result.Path.FullPath);
            }

            [Test]
            public async Task ReturnsInputFileWhenInputDirectoryAboveRoot()
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a/b";
                fileSystem.InputPaths.Add("../x");

                // When
                IFile result = await fileSystem.GetInputFileAsync("bar.txt");

                // Then
                Assert.AreEqual("/a/x/bar.txt", result.Path.FullPath);
            }

            [Test]
            public async Task ReturnsInputFileWhenInputDirectoryAndFileAscend()
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a/b";
                fileSystem.InputPaths.Add("../x/y");

                // When
                IFile result = await fileSystem.GetInputFileAsync("../bar.txt");

                // Then
                Assert.AreEqual("/a/x/bar.txt", result.Path.FullPath);
            }
        }

        public class GetInputDirectoryTests : IReadOnlyFileSystemExtensionsFixture
        {
            [Test]
            public async Task ReturnsVirtualInputDirectoryForRelativePath()
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem();

                // When
                IDirectory result = await fileSystem.GetInputDirectoryAsync("A/B/C");

                // Then
                Assert.IsInstanceOf<VirtualInputDirectory>(result);
                Assert.AreEqual("A/B/C", result.Path.FullPath);
            }

            [Test]
            public async Task ReturnsVirtualInputDirectoryForAscendingPath()
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem();

                // When
                IDirectory result = await fileSystem.GetInputDirectoryAsync("../A/B/C");

                // Then
                Assert.IsInstanceOf<VirtualInputDirectory>(result);
                Assert.AreEqual("../A/B/C", result.Path.FullPath);
            }

            [Test]
            public async Task ReturnsVirtualInputDirectoryForNullPath()
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem();

                // When
                IDirectory result = await fileSystem.GetInputDirectoryAsync();

                // Then
                Assert.IsInstanceOf<VirtualInputDirectory>(result);
                Assert.AreEqual(".", result.Path.FullPath);
            }

            [Test]
            public async Task ReturnsDirectoryForAbsolutePath()
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem();

                // When
                IDirectory result = await fileSystem.GetInputDirectoryAsync("/A/B/C");

                // Then
                Assert.AreEqual("/A/B/C", result.Path.FullPath);
            }
        }

        public class GetInputDirectoriesTests : IReadOnlyFileSystemExtensionsFixture
        {
            [Test]
            public async Task ReturnsCombinedInputDirectories()
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("b/c");
                fileSystem.InputPaths.Add("b/d");
                fileSystem.InputPaths.Add("x");
                fileSystem.InputPaths.Add("y");
                fileSystem.InputPaths.Add("../z");

                // When
                IEnumerable<IDirectory> result = await fileSystem.GetInputDirectoriesAsync();

                // Then
                CollectionAssert.AreEquivalent(
                    new[]
                {
                    "/a/theme",
                    "/a/input",
                    "/a/b/c",
                    "/a/b/d",
                    "/a/x",
                    "/a/y",
                    "/z"
                }, result.Select(x => x.Path.FullPath));
            }
        }

        public class GetContainingInputPathForAbsolutePathTests : IReadOnlyFileSystemExtensionsFixture
        {
            [Test]
            public void ThrowsForNullPath()
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem();

                // When, Then
                Should.Throw<ArgumentNullException>(() => fileSystem.GetContainingInputPathForAbsolutePath(null));
            }

            [TestCase("/a/b/c/foo.txt", "/a/b")]
            [TestCase("/a/x/bar.txt", "/a/x")]
            [TestCase("/a/x/baz.txt", "/a/x")]
            [TestCase("/z/baz.txt", null)]
            [TestCase("/a/b/c/../e/foo.txt", "/a/b")]
            [TestCase("/a/b/c", "/a/b")]
            [TestCase("/a/x", "/a/x")]
            public void ShouldReturnContainingPathForAbsolutePath(string path, string expected)
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("b");
                fileSystem.InputPaths.Add("x");

                // When
                DirectoryPath inputPathFromFilePath = fileSystem.GetContainingInputPathForAbsolutePath(new FilePath(path));
                DirectoryPath inputPathFromDirectoryPath = fileSystem.GetContainingInputPathForAbsolutePath(new DirectoryPath(path));

                // Then
                Assert.AreEqual(expected, inputPathFromFilePath?.FullPath);
                Assert.AreEqual(expected, inputPathFromDirectoryPath?.FullPath);
            }

            [TestCase("/a/b/c/foo.txt", "/a/b")]
            [TestCase("/a/x/bar.txt", "/a/x")]
            [TestCase("/a/x/baz.txt", "/a/x")]
            [TestCase("/z/baz.txt", null)]
            [TestCase("/a/b/c/../e/foo.txt", "/a/b")]
            public void ShouldReturnContainingPathForInputPathAboveRootPath(string path, string expected)
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a/y";
                fileSystem.InputPaths.Add("../b");
                fileSystem.InputPaths.Add("../x");

                // When
                DirectoryPath inputPathFromFilePath = fileSystem.GetContainingInputPathForAbsolutePath(new FilePath(path));
                DirectoryPath inputPathFromDirectoryPath = fileSystem.GetContainingInputPathForAbsolutePath(new DirectoryPath(path));

                // Then
                Assert.AreEqual(expected, inputPathFromFilePath?.FullPath);
                Assert.AreEqual(expected, inputPathFromDirectoryPath?.FullPath);
            }
        }

        public class GetContainingInputPathTests : IReadOnlyFileSystemExtensionsFixture
        {
            [Test]
            public async Task ThrowsForNullPath()
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem();

                // When, Then
                await Should.ThrowAsync<ArgumentNullException>(async () => await fileSystem.GetContainingInputPathAsync(null));
            }

            [TestCase("/a/b/c/foo.txt", "/a/b")]
            [TestCase("/a/x/bar.txt", "/a/x")]
            [TestCase("/a/x/baz.txt", "/a/x")]
            [TestCase("/z/baz.txt", null)]
            [TestCase("/a/b/c/../e/foo.txt", "/a/b")]
            [TestCase("/a/b/c", "/a/b")]
            [TestCase("/a/x", "/a/x")]
            public async Task ShouldReturnContainingPathForAbsolutePath(string path, string expected)
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("b");
                fileSystem.InputPaths.Add("x");

                // When
                DirectoryPath inputPathFromFilePath = await fileSystem.GetContainingInputPathAsync(new FilePath(path));
                DirectoryPath inputPathFromDirectoryPath = await fileSystem.GetContainingInputPathAsync(new DirectoryPath(path));

                // Then
                Assert.AreEqual(expected, inputPathFromFilePath?.FullPath);
                Assert.AreEqual(expected, inputPathFromDirectoryPath?.FullPath);
            }

            [TestCase("/a/b/c/foo.txt", "/a/b")]
            [TestCase("/a/x/bar.txt", "/a/x")]
            [TestCase("/a/x/baz.txt", "/a/x")]
            [TestCase("/z/baz.txt", null)]
            [TestCase("/a/b/c/../e/foo.txt", "/a/b")]
            public async Task ShouldReturnContainingPathForInputPathAboveRootPath(string path, string expected)
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a/y";
                fileSystem.InputPaths.Add("../b");
                fileSystem.InputPaths.Add("../x");

                // When
                DirectoryPath inputPathFromFilePath = await fileSystem.GetContainingInputPathAsync(new FilePath(path));
                DirectoryPath inputPathFromDirectoryPath = await fileSystem.GetContainingInputPathAsync(new DirectoryPath(path));

                // Then
                Assert.AreEqual(expected, inputPathFromFilePath?.FullPath);
                Assert.AreEqual(expected, inputPathFromDirectoryPath?.FullPath);
            }

            [TestCase("c/foo.txt", "/a/b")]
            [TestCase("bar.txt", "/a/x")]
            [TestCase("baz.txt", null)]
            [TestCase("z/baz.txt", null)]
            [TestCase("c/../e/foo.txt", null)]
            [TestCase("c/e/../foo.txt", "/a/b")]
            public async Task ShouldReturnContainingPathForRelativeFilePath(string path, string expected)
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("b");
                fileSystem.InputPaths.Add("x");

                // When
                DirectoryPath inputPath = await fileSystem.GetContainingInputPathAsync(new FilePath(path));

                // Then
                Assert.AreEqual(expected, inputPath?.FullPath);
            }

            [TestCase("c", "/a/b")]
            [TestCase("z", "/a/y")]
            [TestCase("r", null)]
            [TestCase("c/e", null)]
            public async Task ShouldReturnContainingPathForRelativeDirectoryPath(string path, string expected)
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("b");
                fileSystem.InputPaths.Add("y");

                // When
                DirectoryPath inputPath = await fileSystem.GetContainingInputPathAsync(new DirectoryPath(path));

                // Then
                Assert.AreEqual(expected, inputPath?.FullPath);
            }

            [Test]
            public async Task ShouldReturnContainingPathWhenOtherInputPathStartsTheSame()
            {
                // Given
                TestFileProvider fileProvider = GetFileProvider();
                fileProvider.AddDirectory("yz");
                fileProvider.AddDirectory("y");
                fileProvider.AddFile("/a/yz/baz.txt");
                TestFileSystem fileSystem = new TestFileSystem(fileProvider);
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("yz");
                fileSystem.InputPaths.Add("y");

                // When
                DirectoryPath inputPath = await fileSystem.GetContainingInputPathAsync(new FilePath("baz.txt"));

                // Then
                Assert.AreEqual("/a/yz", inputPath?.FullPath);
            }
        }

        public class GetFilesTests : IReadOnlyFileSystemExtensionsFixture
        {
            [Test]
            public async Task ShouldThrowForNullDirectory()
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem(GetFileProvider());

                // When, Then
                await Should.ThrowAsync<ArgumentNullException>(async () => await fileSystem.GetFilesAsync((IDirectory)null, "/"));
            }

            [Test]
            public async Task ShouldThrowForNullPatterns()
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                IDirectory dir = await fileSystem.GetDirectoryAsync("/");

                // When, Then
                await Should.ThrowAsync<ArgumentNullException>(async () => await fileSystem.GetFilesAsync(dir, null));
            }

            [Test]
            public async Task ShouldNotThrowForNullPattern()
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                IDirectory dir = await fileSystem.GetDirectoryAsync("/");

                // When
                IEnumerable<IFile> results = await fileSystem.GetFilesAsync(dir, null, "**/foo.txt");

                // Then
                CollectionAssert.AreEquivalent(new[] { "/a/b/c/foo.txt" }, results.Select(x => x.Path.FullPath));
            }

            [TestCase("/", new[] { "/a/b/c/foo.txt" }, new[] { "/a/b/c/foo.txt" }, true)]
            [TestCase("/", new[] { "a/b/c/foo.txt" }, new[] { "/a/b/c/foo.txt" }, true)]
            [TestCase("/", new[] { "a/b/c/foo.txt", "a/b/c/foo.txt" }, new[] { "/a/b/c/foo.txt" }, true)]
            [TestCase("/a", new[] { "/a/b/c/foo.txt" }, new[] { "/a/b/c/foo.txt" }, true)]
            [TestCase("/a", new[] { "a/b/c/foo.txt" }, new string[] { }, true)]
            [TestCase("/", new[] { "!/a/b/c/foo.txt" }, new string[] { }, true)]
            [TestCase("/a", new[] { "a/b/c/foo.txt", "!/a/b/c/foo.txt" }, new string[] { }, true)]
            [TestCase("/a", new[] { "a/b/c/foo.txt", "a/b/c/foo.txt", "!/a/b/c/foo.txt" }, new string[] { }, true)]
            [TestCase("/a", new[] { "a/b/c/foo.txt", "!/a/b/c/foo.txt", "!/a/b/c/foo.txt" }, new string[] { }, true)]
            [TestCase("/", new[] { "**/foo.txt" }, new[] { "/a/b/c/foo.txt" }, true)]
            [TestCase("/", new[] { "**/foo.txt", "/a/x/bar.txt" }, new[] { "/a/b/c/foo.txt", "/a/x/bar.txt" }, true)]
            [TestCase("/", new[] { "**/foo.txt", "/a/x/baz.txt" }, new[] { "/a/b/c/foo.txt" }, true)]
            [TestCase("/", new[] { "**/foo.txt", "!/a/b/c/foo.txt" }, new string[] { }, true)]
            [TestCase("/", new[] { "**/foo.txt", "!/a/x/baz.txt" }, new[] { "/a/b/c/foo.txt" }, true)]
            [TestCase("/", new[] { "**/foo.txt", "!**/foo.txt" }, new string[] { }, true)]
            [TestCase("/", new[] { "**/foo.txt", "!**/bar.txt" }, new[] { "/a/b/c/foo.txt" }, true)]
            [TestCase("/", new[] { "/**/foo.txt" }, new[] { "/a/b/c/foo.txt" }, true)]
            [TestCase("/", new[] { "/a/b/c/d/../foo.txt" }, new[] { "/a/b/c/foo.txt" }, true)]
            [TestCase("/a", new[] { "a/b/c/foo.txt", "!/a/b/c/d/../foo.txt" }, new string[] { }, true)]
            [TestCase("/", new[] { "/**/*.txt" }, new[] { "/a/x/bar.txt", "/a/b/c/foo.txt", "/q/werty.txt" }, true)]
            [TestCase("/", new[] { "/**/*.txt" }, new[] { "/a/x/bar.txt", "/a/b/c/foo.txt", "/q/werty.txt" }, false)]
            [TestCase("/", new[] { "/q/werty.txt" }, new[] { "/q/werty.txt" }, true)]
            [TestCase("/", new[] { "/q/werty.txt" }, new[] { "/q/werty.txt" }, false)]
            public async Task ShouldReturnExistingFiles(string directory, string[] patterns, string[] expected, bool reverseSlashes)
            {
                // TestContext.Out.WriteLine($"Patterns: {string.Join(",", patterns)}");

                // Given
                TestFileProvider fileProvider = GetFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/q");
                fileProvider.AddFile("/q/werty.txt");
                TestFileSystem fileSystem = new TestFileSystem(fileProvider);
                IDirectory dir = await fileSystem.GetDirectoryAsync(directory);

                // When
                IEnumerable<IFile> results = await fileSystem.GetFilesAsync(dir, patterns);

                // Then
                CollectionAssert.AreEquivalent(expected, results.Select(x => x.Path.FullPath));

                if (reverseSlashes)
                {
                    // When
                    results = await fileSystem.GetFilesAsync(dir, patterns.Select(x => x.Replace("/", "\\")));

                    // Then
                    CollectionAssert.AreEquivalent(expected, results.Select(x => x.Path.FullPath));
                }
            }
        }

        private TestFileProvider GetFileProvider()
        {
            TestFileProvider fileProvider = new TestFileProvider();

            fileProvider.AddDirectory("/");
            fileProvider.AddDirectory("/a");
            fileProvider.AddDirectory("/a/b");
            fileProvider.AddDirectory("/a/b/c");
            fileProvider.AddDirectory("/a/b/d");
            fileProvider.AddDirectory("/a/x");
            fileProvider.AddDirectory("/a/y");
            fileProvider.AddDirectory("/a/y/z");

            fileProvider.AddFile("/a/b/c/foo.txt");
            fileProvider.AddFile("/a/x/bar.txt");

            return fileProvider;
        }
    }
}
