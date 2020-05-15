using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.IO
{
    [TestFixture]
    public class IReadOnlyFileSystemFixture : BaseFixture
    {
        public class GetInputFileTests : IReadOnlyFileSystemFixture
        {
            [TestCase("foo.txt", "/a/b/c/foo.txt")]
            [TestCase("bar.txt", "/a/x/bar.txt")]
            [TestCase("baz.txt", "/a/y/baz.txt")]
            [TestCase("/a/b/c/foo.txt", "/a/b/c/foo.txt")]
            [TestCase("/z/baz.txt", "/z/baz.txt")]
            public void ReturnsInputFile(string input, string expected)
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("b/c");
                fileSystem.InputPaths.Add("b/d");
                fileSystem.InputPaths.Add("x");
                fileSystem.InputPaths.Add("y");

                // When
                IFile result = fileSystem.GetInputFile(input);

                // Then
                Assert.AreEqual(expected, result.Path.FullPath);
            }

            [Test]
            public void ReturnsInputFileAboveInputDirectory()
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("x/t");

                // When
                IFile result = fileSystem.GetInputFile("../bar.txt");

                // Then
                Assert.AreEqual("/a/x/bar.txt", result.Path.FullPath);
            }

            [Test]
            public void ReturnsInputFileWhenInputDirectoryAboveRoot()
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a/b";
                fileSystem.InputPaths.Add("../x");

                // When
                IFile result = fileSystem.GetInputFile("bar.txt");

                // Then
                Assert.AreEqual("/a/x/bar.txt", result.Path.FullPath);
            }

            [Test]
            public void ReturnsInputFileWhenInputDirectoryAndFileAscend()
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a/b";
                fileSystem.InputPaths.Add("../x/y");

                // When
                IFile result = fileSystem.GetInputFile("../bar.txt");

                // Then
                Assert.AreEqual("/a/x/bar.txt", result.Path.FullPath);
            }
        }

        public class GetInputDirectoryTests : IReadOnlyFileSystemFixture
        {
            [Test]
            public void ReturnsVirtualInputDirectoryForRelativePath()
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem();

                // When
                IDirectory result = fileSystem.GetInputDirectory("A/B/C");

                // Then
                Assert.IsInstanceOf<VirtualInputDirectory>(result);
                Assert.AreEqual("A/B/C", result.Path.FullPath);
            }

            [Test]
            public void ReturnsVirtualInputDirectoryForAscendingPath()
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem();

                // When
                IDirectory result = fileSystem.GetInputDirectory("../A/B/C");

                // Then
                Assert.IsInstanceOf<VirtualInputDirectory>(result);
                Assert.AreEqual("../A/B/C", result.Path.FullPath);
            }

            [Test]
            public void ReturnsVirtualInputDirectoryForNullPath()
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem();

                // When
                IDirectory result = fileSystem.GetInputDirectory();

                // Then
                Assert.IsInstanceOf<VirtualInputDirectory>(result);
                Assert.AreEqual(".", result.Path.FullPath);
            }

            [Test]
            public void ReturnsDirectoryForAbsolutePath()
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem();

                // When
                IDirectory result = fileSystem.GetInputDirectory("/A/B/C");

                // Then
                Assert.AreEqual("/A/B/C", result.Path.FullPath);
            }
        }

        public class GetInputDirectoriesTests : IReadOnlyFileSystemFixture
        {
            [Test]
            public void ReturnsCombinedInputDirectories()
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("b/c");
                fileSystem.InputPaths.Add("b/d");
                fileSystem.InputPaths.Add("x");
                fileSystem.InputPaths.Add("y");
                fileSystem.InputPaths.Add("../z");

                // When
                IEnumerable<IDirectory> result = fileSystem.GetInputDirectories();

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

        public class GetContainingInputPathForAbsolutePathTests : IReadOnlyFileSystemFixture
        {
            [Test]
            public void ThrowsForNullPath()
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem();

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
                IFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("b");
                fileSystem.InputPaths.Add("x");

                // When
                NormalizedPath inputPath = fileSystem.GetContainingInputPathForAbsolutePath(new NormalizedPath(path));

                // Then
                inputPath.FullPath.ShouldBe(expected);
            }

            [TestCase("/a/b/c/foo.txt", "/a/b")]
            [TestCase("/a/x/bar.txt", "/a/x")]
            [TestCase("/a/x/baz.txt", "/a/x")]
            [TestCase("/z/baz.txt", null)]
            [TestCase("/a/b/c/../e/foo.txt", "/a/b")]
            public void ShouldReturnContainingPathForInputPathAboveRootPath(string path, string expected)
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a/y";
                fileSystem.InputPaths.Add("../b");
                fileSystem.InputPaths.Add("../x");

                // When
                NormalizedPath inputPath = fileSystem.GetContainingInputPathForAbsolutePath(new NormalizedPath(path));

                // Then
                inputPath.FullPath.ShouldBe(expected);
            }
        }

        public class GetContainingInputPathTests : IReadOnlyFileSystemFixture
        {
            [Test]
            public void ThrowsForNullPath()
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem();

                // When, Then
                Should.Throw<ArgumentNullException>(() => fileSystem.GetContainingInputPath(null));
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
                IFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("b");
                fileSystem.InputPaths.Add("x");

                // When
                NormalizedPath inputPath = fileSystem.GetContainingInputPath(new NormalizedPath(path));

                // Then
                inputPath.FullPath.ShouldBe(expected);
            }

            [TestCase("/a/b/c/foo.txt", "/a/b")]
            [TestCase("/a/x/bar.txt", "/a/x")]
            [TestCase("/a/x/baz.txt", "/a/x")]
            [TestCase("/z/baz.txt", null)]
            [TestCase("/a/b/c/../e/foo.txt", "/a/b")]
            public void ShouldReturnContainingPathForInputPathAboveRootPath(string path, string expected)
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a/y";
                fileSystem.InputPaths.Add("../b");
                fileSystem.InputPaths.Add("../x");

                // When
                NormalizedPath inputPath = fileSystem.GetContainingInputPath(new NormalizedPath(path));

                // Then
                inputPath.FullPath.ShouldBe(expected);
            }

            [TestCase("c/foo.txt", "/a/b")]
            [TestCase("bar.txt", "/a/x")]
            [TestCase("baz.txt", null)]
            [TestCase("z/baz.txt", null)]
            [TestCase("c/../e/foo.txt", null)]
            [TestCase("c/e/../foo.txt", "/a/b")]
            public void ShouldReturnContainingPathForRelativeFilePath(string path, string expected)
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("b");
                fileSystem.InputPaths.Add("x");

                // When
                NormalizedPath inputPath = fileSystem.GetContainingInputPath(new NormalizedPath(path));

                // Then
                inputPath.FullPath.ShouldBe(expected);
            }

            [TestCase("c", "/a/b")]
            [TestCase("z", "/a/y")]
            [TestCase("r", null)]
            [TestCase("c/e", null)]
            public void ShouldReturnContainingPathForRelativeDirectoryPath(string path, string expected)
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("b");
                fileSystem.InputPaths.Add("y");

                // When
                NormalizedPath inputPath = fileSystem.GetContainingInputPath(new NormalizedPath(path));

                // Then
                inputPath.FullPath.ShouldBe(expected);
            }

            [Test]
            public void ShouldReturnContainingPathWhenOtherInputPathStartsTheSame()
            {
                // Given
                TestFileProvider fileProvider = GetFileProvider();
                fileProvider.AddDirectory("yz");
                fileProvider.AddDirectory("y");
                fileProvider.AddFile("/a/yz/baz.txt");
                IFileSystem fileSystem = new TestFileSystem(fileProvider);
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("yz");
                fileSystem.InputPaths.Add("y");

                // When
                NormalizedPath inputPath = fileSystem.GetContainingInputPath(new NormalizedPath("baz.txt"));

                // Then
                inputPath.FullPath.ShouldBe("/a/yz");
            }
        }

        public class GetFilesTests : IReadOnlyFileSystemFixture
        {
            [Test]
            public void ShouldThrowForNullDirectory()
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem(GetFileProvider());

                // When, Then
                Should.Throw<ArgumentNullException>(() => fileSystem.GetFiles((IDirectory)null, "/"));
            }

            [Test]
            public void ShouldThrowForNullPatterns()
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                IDirectory dir = fileSystem.GetDirectory("/");

                // When, Then
                Should.Throw<ArgumentNullException>(() => fileSystem.GetFiles(dir, null));
            }

            [Test]
            public void ShouldNotThrowForNullPattern()
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                IDirectory dir = fileSystem.GetDirectory("/");

                // When
                IEnumerable<IFile> results = fileSystem.GetFiles(dir, null, "**/foo.txt");

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
            public void ShouldReturnExistingFiles(string directory, string[] patterns, string[] expected, bool reverseSlashes)
            {
                // TestContext.Out.WriteLine($"Patterns: {string.Join(",", patterns)}");

                // Given
                TestFileProvider fileProvider = GetFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/q");
                fileProvider.AddFile("/q/werty.txt");
                IFileSystem fileSystem = new TestFileSystem(fileProvider);
                IDirectory dir = fileSystem.GetDirectory(directory);

                // When
                IEnumerable<IFile> results = fileSystem.GetFiles(dir, patterns);

                // Then
                CollectionAssert.AreEquivalent(expected, results.Select(x => x.Path.FullPath));

                if (reverseSlashes)
                {
                    // When
                    results = fileSystem.GetFiles(dir, patterns.Select(x => x.Replace("/", "\\")));

                    // Then
                    CollectionAssert.AreEquivalent(expected, results.Select(x => x.Path.FullPath));
                }
            }
        }

        private TestFileProvider GetFileProvider()
        {
            TestFileProvider fileProvider = new TestFileProvider
            {
                { "/a/b/c/foo.txt" },
                { "/a/x/bar.txt" }
            };

            // Directories that don't exist as files
            fileProvider.AddDirectory("/a/b/d");
            fileProvider.AddDirectory("/a/y/z");

            return fileProvider;
        }
    }
}
