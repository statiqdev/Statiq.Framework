using System;
using System.Linq;
using NUnit.Framework;
using Statiq.Testing;

namespace Statiq.Core.Tests.IO
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
    }
}
