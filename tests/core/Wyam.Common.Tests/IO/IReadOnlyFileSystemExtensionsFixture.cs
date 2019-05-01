using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.IO;
using Wyam.Testing;
using Wyam.Testing.IO;

namespace Wyam.Common.Tests.IO
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
                TestFileSystem fileSystem = new TestFileSystem();
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("b/c");
                fileSystem.InputPaths.Add("b/d");
                fileSystem.InputPaths.Add("x");
                fileSystem.InputPaths.Add("y");
                fileSystem.FileProviders.Add(NormalizedPath.DefaultFileProvider.Scheme, GetFileProvider());

                // When
                IFile result = await fileSystem.GetInputFileAsync(input);

                // Then
                Assert.AreEqual(expected, result.Path.FullPath);
            }

            [Test]
            public async Task ReturnsInputFileAboveInputDirectory()
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem();
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("x/t");
                fileSystem.FileProviders.Add(NormalizedPath.DefaultFileProvider.Scheme, GetFileProvider());

                // When
                IFile result = await fileSystem.GetInputFileAsync("../bar.txt");

                // Then
                Assert.AreEqual("/a/x/bar.txt", result.Path.FullPath);
            }

            [Test]
            public async Task ReturnsInputFileWhenInputDirectoryAboveRoot()
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem();
                fileSystem.RootPath = "/a/b";
                fileSystem.InputPaths.Add("../x");
                fileSystem.FileProviders.Add(NormalizedPath.DefaultFileProvider.Scheme, GetFileProvider());

                // When
                IFile result = await fileSystem.GetInputFileAsync("bar.txt");

                // Then
                Assert.AreEqual("/a/x/bar.txt", result.Path.FullPath);
            }

            [Test]
            public async Task ReturnsInputFileWhenInputDirectoryAndFileAscend()
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem();
                fileSystem.RootPath = "/a/b";
                fileSystem.InputPaths.Add("../x/y");
                fileSystem.FileProviders.Add(NormalizedPath.DefaultFileProvider.Scheme, GetFileProvider());

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
                TestFileSystem fileSystem = new TestFileSystem();
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("b/c");
                fileSystem.InputPaths.Add("b/d");
                fileSystem.InputPaths.Add("x");
                fileSystem.InputPaths.Add("y");
                fileSystem.InputPaths.Add("../z");
                fileSystem.FileProviders.Add(NormalizedPath.DefaultFileProvider.Scheme, GetFileProvider());

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
                TestFileSystem fileSystem = new TestFileSystem();
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("b");
                fileSystem.InputPaths.Add("x");
                fileSystem.FileProviders.Add(string.Empty, GetFileProvider());

                // When
                DirectoryPath inputPathFromFilePath = await fileSystem.GetContainingInputPathAsync(new FilePath(path));
                DirectoryPath inputPathFromDirectoryPath = await fileSystem.GetContainingInputPathAsync(new DirectoryPath(path));

                // Then
                Assert.AreEqual(expected, inputPathFromFilePath?.FullPath);
                Assert.AreEqual(expected, inputPathFromDirectoryPath?.FullPath);
            }

            [TestCase("/a/b/c/foo.txt", "/a/y/../b")]
            [TestCase("/a/x/bar.txt", "/a/y/../x")]
            [TestCase("/a/x/baz.txt", "/a/y/../x")]
            [TestCase("/z/baz.txt", null)]
            [TestCase("/a/b/c/../e/foo.txt", "/a/y/../b")]
            public async Task ShouldReturnContainingPathForInputPathAboveRootPath(string path, string expected)
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem();
                fileSystem.RootPath = "/a/y";
                fileSystem.InputPaths.Add("../b");
                fileSystem.InputPaths.Add("../x");
                fileSystem.FileProviders.Add(string.Empty, GetFileProvider());

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
                TestFileSystem fileSystem = new TestFileSystem();
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("b");
                fileSystem.InputPaths.Add("x");
                fileSystem.FileProviders.Add(NormalizedPath.DefaultFileProvider.Scheme, GetFileProvider());

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
                TestFileSystem fileSystem = new TestFileSystem();
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("b");
                fileSystem.InputPaths.Add("y");
                fileSystem.FileProviders.Add(NormalizedPath.DefaultFileProvider.Scheme, GetFileProvider());

                // When
                DirectoryPath inputPath = await fileSystem.GetContainingInputPathAsync(new DirectoryPath(path));

                // Then
                Assert.AreEqual(expected, inputPath?.FullPath);
            }

            [Test]
            public async Task ShouldReturnContainingPathWhenOtherInputPathStartsTheSame()
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem();
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("yz");
                fileSystem.InputPaths.Add("y");
                TestFileProvider fileProvider = (TestFileProvider)GetFileProvider();
                fileProvider.AddDirectory("yz");
                fileProvider.AddDirectory("y");
                fileProvider.AddFile("/a/yz/baz.txt");
                fileSystem.FileProviders.Add(NormalizedPath.DefaultFileProvider.Scheme, fileProvider);

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
                TestFileSystem fileSystem = new TestFileSystem();
                fileSystem.FileProviders.Add(NormalizedPath.DefaultFileProvider.Scheme, GetFileProvider());

                // When, Then
                await Should.ThrowAsync<ArgumentNullException>(async () => await fileSystem.GetFilesAsync((IDirectory)null, "/"));
            }

            [Test]
            public async Task ShouldThrowForNullPatterns()
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem();
                fileSystem.FileProviders.Add(NormalizedPath.DefaultFileProvider.Scheme, GetFileProvider());
                IDirectory dir = await fileSystem.GetDirectoryAsync("/");

                // When, Then
                await Should.ThrowAsync<ArgumentNullException>(async () => await fileSystem.GetFilesAsync(dir, null));
            }

            [Test]
            public async Task ShouldNotThrowForNullPattern()
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem();
                fileSystem.FileProviders.Add(NormalizedPath.DefaultFileProvider.Scheme, GetFileProvider());
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
            [TestCase("/", new[] { "qwerty:///**/*.txt" }, new[] { "/q/werty.txt" }, false)]
            [TestCase("/", new[] { "qwerty|/**/*.txt" }, new[] { "/q/werty.txt" }, true)]
            [TestCase("/", new[] { "qwerty:///|/**/*.txt" }, new[] { "/q/werty.txt" }, false)]
            [TestCase("/", new[] { "/q/werty.txt" }, new string[] { }, true)]
            [TestCase("qwerty|/", new[] { "/q/werty.txt" }, new string[] { }, true)]
            [TestCase("qwerty:///|/", new[] { "/q/werty.txt" }, new string[] { }, true)]
            [TestCase("qwerty:///", new[] { "/q/werty.txt" }, new string[] { }, true)]
            [TestCase("/", new[] { "qwerty|/q/werty.txt" }, new[] { "/q/werty.txt" }, true)]
            [TestCase("/", new[] { "qwerty:///|/q/werty.txt" }, new[] { "/q/werty.txt" }, false)]
            [TestCase("/", new[] { "qwerty:///q/werty.txt" }, new[] { "/q/werty.txt" }, false)]
            public async Task ShouldReturnExistingFiles(string directory, string[] patterns, string[] expected, bool reverseSlashes)
            {
                // TestContext.Out.WriteLine($"Patterns: {string.Join(",", patterns)}");

                // Given
                TestFileSystem fileSystem = new TestFileSystem();
                fileSystem.FileProviders.Add(NormalizedPath.DefaultFileProvider.Scheme, GetFileProvider());
                TestFileProvider alternateProvider = new TestFileProvider();
                alternateProvider.AddDirectory("/");
                alternateProvider.AddDirectory("/q");
                alternateProvider.AddFile("/q/werty.txt");
                fileSystem.FileProviders.Add("qwerty", alternateProvider);
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

        private IFileProvider GetFileProvider()
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
