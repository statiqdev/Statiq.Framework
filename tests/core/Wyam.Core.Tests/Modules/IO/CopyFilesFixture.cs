using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Core.Modules.IO;
using Wyam.Testing;
using Wyam.Testing.Execution;
using Wyam.Testing.IO;

namespace Wyam.Core.Tests.Modules.IO
{
    [TestFixture]
    [NonParallelizable]
    public class CopyFilesFixture : BaseFixture
    {
        public class ConstructorTests : CopyFilesFixture
        {
            [Test]
            public void ThrowsOnNullPathFunction()
            {
                // Given, When, Then
                Assert.Throws<ArgumentNullException>(() => new CopyFiles((DocumentConfig<IEnumerable<string>>)null));
            }

            [Test]
            public void ThrowsOnNullPatterns()
            {
                // Given, When, Then
                Assert.Throws<ArgumentNullException>(() => new CopyFiles((string[])null));
            }
        }

        public class ExecuteTests : CopyFilesFixture
        {
            [Test]
            public async Task RecursivePatternCopiesFiles()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                CopyFiles copyFiles = new CopyFiles("**/*.txt");

                // When
                await ExecuteAsync(context, copyFiles);

                // Then
                Assert.IsTrue(await (await context.FileSystem.GetOutputFileAsync("test-a.txt")).GetExistsAsync());
                Assert.IsTrue(await (await context.FileSystem.GetOutputFileAsync("test-b.txt")).GetExistsAsync());
                Assert.IsTrue(await (await context.FileSystem.GetOutputFileAsync("Subfolder/test-c.txt")).GetExistsAsync());
                Assert.IsTrue(await (await context.FileSystem.GetOutputDirectoryAsync("Subfolder")).GetExistsAsync());
                Assert.IsFalse(await (await context.FileSystem.GetOutputFileAsync("markdown-x.md")).GetExistsAsync());
                Assert.IsFalse(await (await context.FileSystem.GetOutputFileAsync("Subfolder/markdown-y.md")).GetExistsAsync());
            }

            [Test]
            public async Task CopyFilesInTopDirectoryOnly()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                CopyFiles copyFiles = new CopyFiles("*.txt");

                // When
                await ExecuteAsync(context, copyFiles);

                // Then
                Assert.IsTrue(await (await context.FileSystem.GetOutputFileAsync("test-a.txt")).GetExistsAsync());
                Assert.IsTrue(await (await context.FileSystem.GetOutputFileAsync("test-b.txt")).GetExistsAsync());
                Assert.IsFalse(await (await context.FileSystem.GetOutputFileAsync("Subfolder/test-c.txt")).GetExistsAsync());
                Assert.IsFalse(await (await context.FileSystem.GetOutputDirectoryAsync("Subfolder")).GetExistsAsync());
                Assert.IsFalse(await (await context.FileSystem.GetOutputFileAsync("markdown-x.md")).GetExistsAsync());
                Assert.IsFalse(await (await context.FileSystem.GetOutputFileAsync("Subfolder/markdown-y.md")).GetExistsAsync());
            }

            [Test]
            public async Task CopyFilesInSubfolderOnly()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                CopyFiles copyFiles = new CopyFiles("Subfolder/*.txt");

                // When
                await ExecuteAsync(context, copyFiles);

                // Then
                Assert.IsFalse(await (await context.FileSystem.GetOutputFileAsync("test-a.txt")).GetExistsAsync());
                Assert.IsFalse(await (await context.FileSystem.GetOutputFileAsync("test-b.txt")).GetExistsAsync());
                Assert.IsTrue(await (await context.FileSystem.GetOutputFileAsync("Subfolder/test-c.txt")).GetExistsAsync());
                Assert.IsTrue(await (await context.FileSystem.GetOutputDirectoryAsync("Subfolder")).GetExistsAsync());
                Assert.IsFalse(await (await context.FileSystem.GetOutputFileAsync("markdown-x.md")).GetExistsAsync());
                Assert.IsFalse(await (await context.FileSystem.GetOutputFileAsync("Subfolder/markdown-y.md")).GetExistsAsync());
            }

            [Test]
            public async Task CopyFilesAboveInputPath()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                CopyFiles copyFiles = new CopyFiles("../*.txt");

                // When
                await ExecuteAsync(context, copyFiles);

                // Then
                Assert.IsFalse(await (await context.FileSystem.GetOutputFileAsync("test-a.txt")).GetExistsAsync());
                Assert.IsFalse(await (await context.FileSystem.GetOutputFileAsync("test-b.txt")).GetExistsAsync());
                Assert.IsFalse(await (await context.FileSystem.GetOutputFileAsync("Subfolder/test-c.txt")).GetExistsAsync());
                Assert.IsFalse(await (await context.FileSystem.GetOutputDirectoryAsync("Subfolder")).GetExistsAsync());
                Assert.IsFalse(await (await context.FileSystem.GetOutputFileAsync("markdown-x.md")).GetExistsAsync());
                Assert.IsFalse(await (await context.FileSystem.GetOutputFileAsync("Subfolder/markdown-y.md")).GetExistsAsync());
                Assert.IsTrue(await (await context.FileSystem.GetOutputFileAsync("test-above-input.txt")).GetExistsAsync());
            }

            [Test]
            public async Task CopyFilesAboveInputPathWithOthers()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                CopyFiles copyFiles = new CopyFiles("../**/*.txt");

                // When
                await ExecuteAsync(context, copyFiles);

                // Then
                Assert.IsTrue(await (await context.FileSystem.GetOutputFileAsync("test-a.txt")).GetExistsAsync());
                Assert.IsTrue(await (await context.FileSystem.GetOutputFileAsync("test-b.txt")).GetExistsAsync());
                Assert.IsTrue(await (await context.FileSystem.GetOutputFileAsync("Subfolder/test-c.txt")).GetExistsAsync());
                Assert.IsTrue(await (await context.FileSystem.GetOutputDirectoryAsync("Subfolder")).GetExistsAsync());
                Assert.IsFalse(await (await context.FileSystem.GetOutputFileAsync("markdown-x.md")).GetExistsAsync());
                Assert.IsFalse(await (await context.FileSystem.GetOutputFileAsync("Subfolder/markdown-y.md")).GetExistsAsync());
                Assert.IsTrue(await (await context.FileSystem.GetOutputFileAsync("test-above-input.txt")).GetExistsAsync());
            }

            [Test]
            public async Task CopyFolderFromAbsolutePath()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                CopyFiles copyFiles = new CopyFiles("/TestFiles/Input/**/*.txt");

                // When
                await ExecuteAsync(context, copyFiles);

                // Then
                Assert.IsTrue(await (await context.FileSystem.GetOutputFileAsync("test-a.txt")).GetExistsAsync());
                Assert.IsTrue(await (await context.FileSystem.GetOutputFileAsync("test-b.txt")).GetExistsAsync());
                Assert.IsTrue(await (await context.FileSystem.GetOutputFileAsync("Subfolder/test-c.txt")).GetExistsAsync());
                Assert.IsTrue(await (await context.FileSystem.GetOutputDirectoryAsync("Subfolder")).GetExistsAsync());
                Assert.IsFalse(await (await context.FileSystem.GetOutputFileAsync("markdown-x.md")).GetExistsAsync());
                Assert.IsFalse(await (await context.FileSystem.GetOutputFileAsync("Subfolder/markdown-y.md")).GetExistsAsync());
            }

            [Test]
            public async Task CopyNonExistingFolder()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                CopyFiles copyFiles = new CopyFiles("NonExisting/**/*.txt");

                // When
                await ExecuteAsync(context, copyFiles);

                // Then
                Assert.IsFalse(await (await context.FileSystem.GetOutputFileAsync("test-a.txt")).GetExistsAsync());
                Assert.IsFalse(await (await context.FileSystem.GetOutputFileAsync("test-b.txt")).GetExistsAsync());
                Assert.IsFalse(await (await context.FileSystem.GetOutputFileAsync("Subfolder/test-c.txt")).GetExistsAsync());
                Assert.IsFalse(await (await context.FileSystem.GetOutputDirectoryAsync("Subfolder")).GetExistsAsync());
                Assert.IsFalse(await (await context.FileSystem.GetOutputFileAsync("markdown-x.md")).GetExistsAsync());
                Assert.IsFalse(await (await context.FileSystem.GetOutputFileAsync("Subfolder/markdown-y.md")).GetExistsAsync());
            }

            public async Task ShouldSetSourceAndDestination()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                CopyFiles copyFiles = new CopyFiles("**/test-a.txt");

                // When
                IDocument output = (await ExecuteAsync(context, copyFiles))[0];

                // Then
                output.Source.FullPath.ShouldBe("/TestFiles/Input/test-a.txt");
                output.Destination.FullPath.ShouldBe("test-a.txt");
            }
        }

        protected static TestExecutionContext GetExecutionContext()
        {
            TestFileProvider fileProvider = new TestFileProvider();

            fileProvider.AddDirectory("/");
            fileProvider.AddDirectory("/TestFiles");
            fileProvider.AddDirectory("/TestFiles/Input");
            fileProvider.AddDirectory("/TestFiles/Input/Subfolder");

            fileProvider.AddFile("/TestFiles/test-above-input.txt", "test");
            fileProvider.AddFile("/TestFiles/Input/markdown-x.md", "xxx");
            fileProvider.AddFile("/TestFiles/Input/test-a.txt", "aaa");
            fileProvider.AddFile("/TestFiles/Input/test-b.txt", "bbb");
            fileProvider.AddFile("/TestFiles/Input/Subfolder/markdown-y.md", "yyy");
            fileProvider.AddFile("/TestFiles/Input/Subfolder/test-c.txt", "ccc");

            TestFileSystem fileSystem = new TestFileSystem
            {
                FileProvider = fileProvider,
                RootPath = "/"
            };
            fileSystem.InputPaths.Clear();
            fileSystem.InputPaths.Add("/TestFiles/Input");

            return new TestExecutionContext
            {
                FileSystem = fileSystem
            };
        }
    }
}
