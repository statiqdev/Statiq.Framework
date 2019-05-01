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
    }
}
