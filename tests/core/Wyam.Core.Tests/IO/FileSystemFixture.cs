using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.IO;
using Wyam.Core.IO;
using Wyam.Testing;
using Wyam.Testing.IO;

namespace Wyam.Core.Tests.IO
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class FileSystemFixture : BaseFixture
    {
        public class ConstructorTests : FileSystemFixture
        {
            [Test]
            public void AddsDefaultInputPath()
            {
                // Given, When
                FileSystem fileSystem = new FileSystem();

                // Then
                CollectionAssert.AreEquivalent(new[] { "theme", "input" }, fileSystem.InputPaths.Select(x => x.FullPath));
            }
        }

        public class RootPathTests : FileSystemFixture
        {
            [Test]
            public void SetThrowsForNullValue()
            {
                // Given
                FileSystem fileSystem = new FileSystem();

                // When, Then
                Assert.Throws<ArgumentNullException>(() => fileSystem.RootPath = null);
            }

            [Test]
            public void SetThrowsForRelativePath()
            {
                // Given
                FileSystem fileSystem = new FileSystem();

                // When, Then
                Assert.Throws<ArgumentException>(() => fileSystem.RootPath = "foo");
            }

            [Test]
            public void CanSet()
            {
                // Given
                FileSystem fileSystem = new FileSystem();

                // When
                fileSystem.RootPath = "/foo/bar";

                // Then
                Assert.AreEqual("/foo/bar", fileSystem.RootPath.FullPath);
            }
        }

        public class OutputPathTests : FileSystemFixture
        {
            [Test]
            public void SetThrowsForNullValue()
            {
                // Given
                FileSystem fileSystem = new FileSystem();

                // When, Then
                Assert.Throws<ArgumentNullException>(() => fileSystem.OutputPath = null);
            }

            [Test]
            public void CanSet()
            {
                // Given
                FileSystem fileSystem = new FileSystem();

                // When
                fileSystem.OutputPath = "/foo/bar";

                // Then
                Assert.AreEqual("/foo/bar", fileSystem.OutputPath.FullPath);
            }
        }

        public class GetInputFileTests : FileSystemFixture
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
                FileSystem fileSystem = new FileSystem();
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
                FileSystem fileSystem = new FileSystem();
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
                FileSystem fileSystem = new FileSystem();
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
                FileSystem fileSystem = new FileSystem();
                fileSystem.RootPath = "/a/b";
                fileSystem.InputPaths.Add("../x/y");
                fileSystem.FileProviders.Add(NormalizedPath.DefaultFileProvider.Scheme, GetFileProvider());

                // When
                IFile result = await fileSystem.GetInputFileAsync("../bar.txt");

                // Then
                Assert.AreEqual("/a/x/bar.txt", result.Path.FullPath);
            }
        }

        public class GetInputDirectoryTests : FileSystemFixture
        {
            [Test]
            public async Task ReturnsVirtualInputDirectoryForRelativePath()
            {
                // Given
                FileSystem fileSystem = new FileSystem();

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
                FileSystem fileSystem = new FileSystem();

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
                FileSystem fileSystem = new FileSystem();

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
                FileSystem fileSystem = new FileSystem();

                // When
                IDirectory result = await fileSystem.GetInputDirectoryAsync("/A/B/C");

                // Then
                Assert.AreEqual("/A/B/C", result.Path.FullPath);
            }
        }

        public class GetInputDirectoriesTests : FileSystemFixture
        {
            [Test]
            public async Task ReturnsCombinedInputDirectories()
            {
                // Given
                FileSystem fileSystem = new FileSystem();
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

        public class GetContainingInputPathTests : FileSystemFixture
        {
            [Test]
            public async Task ThrowsForNullPath()
            {
                // Given
                FileSystem fileSystem = new FileSystem();

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
                FileSystem fileSystem = new FileSystem();
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
                FileSystem fileSystem = new FileSystem();
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
                FileSystem fileSystem = new FileSystem();
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
                FileSystem fileSystem = new FileSystem();
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
                FileSystem fileSystem = new FileSystem();
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

        public class GetFileProviderTests : FileSystemFixture
        {
            [Test]
            public void ThrowsForNullPath()
            {
                // Given
                FileSystem fileSystem = new FileSystem();

                // When, Then
                Assert.Throws<ArgumentNullException>(() => fileSystem.GetFileProvider(null));
            }

            [Test]
            public void ThrowsForRelativeDirectoryPath()
            {
                // Given
                FileSystem fileSystem = new FileSystem();
                DirectoryPath relativePath = new DirectoryPath("A/B/C");

                // When, Then
                Assert.Throws<ArgumentException>(() => fileSystem.GetFileProvider(relativePath));
            }

            [Test]
            public void ThrowsForRelativeFilePath()
            {
                // Given
                FileSystem fileSystem = new FileSystem();
                FilePath relativePath = new FilePath("A/B/C.txt");

                // When, Then
                Assert.Throws<ArgumentException>(() => fileSystem.GetFileProvider(relativePath));
            }

            [Test]
            public void ReturnsDefaultProviderForDirectoryPath()
            {
                // Given
                FileSystem fileSystem = new FileSystem();
                IFileProvider defaultProvider = new TestFileProvider();
                IFileProvider fooProvider = new TestFileProvider();
                fileSystem.FileProviders.Add(NormalizedPath.DefaultFileProvider.Scheme, defaultProvider);
                fileSystem.FileProviders.Add("foo", fooProvider);
                DirectoryPath path = new DirectoryPath("/a/b/c");

                // When
                IFileProvider result = fileSystem.GetFileProvider(path);

                // Then
                Assert.AreEqual(defaultProvider, result);
            }

            [Test]
            public void ReturnsDefaultProviderForFilePath()
            {
                // Given
                FileSystem fileSystem = new FileSystem();
                IFileProvider defaultProvider = new TestFileProvider();
                IFileProvider fooProvider = new TestFileProvider();
                fileSystem.FileProviders.Add(NormalizedPath.DefaultFileProvider.Scheme, defaultProvider);
                fileSystem.FileProviders.Add("foo", fooProvider);
                FilePath path = new FilePath("/a/b/c.txt");

                // When
                IFileProvider result = fileSystem.GetFileProvider(path);

                // Then
                Assert.AreEqual(defaultProvider, result);
            }

            [Test]
            public void ReturnsOtherProviderForDirectoryPath()
            {
                // Given
                FileSystem fileSystem = new FileSystem();
                IFileProvider defaultProvider = new TestFileProvider();
                IFileProvider fooProvider = new TestFileProvider();
                fileSystem.FileProviders.Add(NormalizedPath.DefaultFileProvider.Scheme, defaultProvider);
                fileSystem.FileProviders.Add("foo", fooProvider);
                DirectoryPath path = new DirectoryPath("foo", "/a/b/c");

                // When
                IFileProvider result = fileSystem.GetFileProvider(path);

                // Then
                Assert.AreEqual(fooProvider, result);
            }

            [Test]
            public void ReturnsOtherProviderForFilePath()
            {
                // Given
                FileSystem fileSystem = new FileSystem();
                IFileProvider defaultProvider = new TestFileProvider();
                IFileProvider fooProvider = new TestFileProvider();
                fileSystem.FileProviders.Add(NormalizedPath.DefaultFileProvider.Scheme, defaultProvider);
                fileSystem.FileProviders.Add("foo", fooProvider);
                FilePath path = new FilePath("foo", "/a/b/c.txt");

                // When
                IFileProvider result = fileSystem.GetFileProvider(path);

                // Then
                Assert.AreEqual(fooProvider, result);
            }

            [Test]
            public void ThrowsIfProviderNotFoundForDirectoryPath()
            {
                // Given
                FileSystem fileSystem = new FileSystem();
                DirectoryPath path = new DirectoryPath("foo", "/a/b/c");

                // When, Then
                Assert.Throws<KeyNotFoundException>(() => fileSystem.GetFileProvider(path));
            }

            [Test]
            public void ThrowsIfProviderNotFoundForFilePath()
            {
                // Given
                FileSystem fileSystem = new FileSystem();
                FilePath path = new FilePath("foo", "/a/b/c.txt");

                // When, Then
                Assert.Throws<KeyNotFoundException>(() => fileSystem.GetFileProvider(path));
            }
        }

        public class GetFilesTests : FileSystemFixture
        {
            [Test]
            public async Task ShouldThrowForNullDirectory()
            {
                // Given
                FileSystem fileSystem = new FileSystem();
                fileSystem.FileProviders.Add(NormalizedPath.DefaultFileProvider.Scheme, GetFileProvider());

                // When, Then
                await Should.ThrowAsync<ArgumentNullException>(async () => await fileSystem.GetFilesAsync((IDirectory)null, "/"));
            }

            [Test]
            public async Task ShouldThrowForNullPatterns()
            {
                // Given
                FileSystem fileSystem = new FileSystem();
                fileSystem.FileProviders.Add(NormalizedPath.DefaultFileProvider.Scheme, GetFileProvider());
                IDirectory dir = await fileSystem.GetDirectoryAsync("/");

                // When, Then
                await Should.ThrowAsync<ArgumentNullException>(async () => await fileSystem.GetFilesAsync(dir, null));
            }

            [Test]
            public async Task ShouldNotThrowForNullPattern()
            {
                // Given
                FileSystem fileSystem = new FileSystem();
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
                FileSystem fileSystem = new FileSystem();
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
