using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Util;
using Wyam.Core.Documents;
using Wyam.Core.Execution;
using Wyam.Core.Modules.IO;
using Wyam.Testing;
using Wyam.Testing.Execution;
using Wyam.Testing.IO;
using ExecutionContext = Wyam.Core.Execution.ExecutionContext;

namespace Wyam.Core.Tests.Modules.IO
{
    [TestFixture]
    [NonParallelizable]
    public class CopyFilesFixture : BaseFixture
    {
        private Engine Engine { get; set; }
        private ExecutionPipeline Pipeline { get; set; }
        private IExecutionContext Context { get; set; }
        private IDocument[] Inputs { get; set; }

        [SetUp]
        public void SetUp()
        {
            IServiceProvider serviceProvider = new TestServiceProvider();
            Engine = new Engine();
            Engine.FileSystem.FileProviders.Add(NormalizedPath.DefaultFileProvider.Scheme, GetFileProvider());
            Engine.FileSystem.RootPath = "/";
            Engine.FileSystem.InputPaths.Clear();
            Engine.FileSystem.InputPaths.Add("/TestFiles/Input");
            Pipeline = new ExecutionPipeline("Pipeline", (IModuleList)null);
            Context = new ExecutionContext(Engine, Guid.Empty, Pipeline, serviceProvider);
            Inputs = new[] { Context.GetDocument() };
        }

        private IFileProvider GetFileProvider()
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

            return fileProvider;
        }

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
                CopyFiles copyFiles = new CopyFiles("**/*.txt");

                // When
                await copyFiles.ExecuteAsync(Inputs, Context).ToListAsync();

                // Then
                Assert.IsTrue(await (await Engine.FileSystem.GetOutputFileAsync("test-a.txt")).GetExistsAsync());
                Assert.IsTrue(await (await Engine.FileSystem.GetOutputFileAsync("test-b.txt")).GetExistsAsync());
                Assert.IsTrue(await (await Engine.FileSystem.GetOutputFileAsync("Subfolder/test-c.txt")).GetExistsAsync());
                Assert.IsTrue(await (await Engine.FileSystem.GetOutputDirectoryAsync("Subfolder")).GetExistsAsync());
                Assert.IsFalse(await (await Engine.FileSystem.GetOutputFileAsync("markdown-x.md")).GetExistsAsync());
                Assert.IsFalse(await (await Engine.FileSystem.GetOutputFileAsync("Subfolder/markdown-y.md")).GetExistsAsync());
            }

            [Test]
            public async Task CopyFilesInTopDirectoryOnly()
            {
                // Given
                CopyFiles copyFiles = new CopyFiles("*.txt");

                // When
                await copyFiles.ExecuteAsync(Inputs, Context).ToListAsync();

                // Then
                Assert.IsTrue(await (await Engine.FileSystem.GetOutputFileAsync("test-a.txt")).GetExistsAsync());
                Assert.IsTrue(await (await Engine.FileSystem.GetOutputFileAsync("test-b.txt")).GetExistsAsync());
                Assert.IsFalse(await (await Engine.FileSystem.GetOutputFileAsync("Subfolder/test-c.txt")).GetExistsAsync());
                Assert.IsFalse(await (await Engine.FileSystem.GetOutputDirectoryAsync("Subfolder")).GetExistsAsync());
                Assert.IsFalse(await (await Engine.FileSystem.GetOutputFileAsync("markdown-x.md")).GetExistsAsync());
                Assert.IsFalse(await (await Engine.FileSystem.GetOutputFileAsync("Subfolder/markdown-y.md")).GetExistsAsync());
            }

            [Test]
            public async Task CopyFilesInSubfolderOnly()
            {
                // Given
                CopyFiles copyFiles = new CopyFiles("Subfolder/*.txt");

                // When
                await copyFiles.ExecuteAsync(Inputs, Context).ToListAsync();

                // Then
                Assert.IsFalse(await (await Engine.FileSystem.GetOutputFileAsync("test-a.txt")).GetExistsAsync());
                Assert.IsFalse(await (await Engine.FileSystem.GetOutputFileAsync("test-b.txt")).GetExistsAsync());
                Assert.IsTrue(await (await Engine.FileSystem.GetOutputFileAsync("Subfolder/test-c.txt")).GetExistsAsync());
                Assert.IsTrue(await (await Engine.FileSystem.GetOutputDirectoryAsync("Subfolder")).GetExistsAsync());
                Assert.IsFalse(await (await Engine.FileSystem.GetOutputFileAsync("markdown-x.md")).GetExistsAsync());
                Assert.IsFalse(await (await Engine.FileSystem.GetOutputFileAsync("Subfolder/markdown-y.md")).GetExistsAsync());
            }

            [Test]
            public async Task CopyFilesAboveInputPath()
            {
                // Given
                CopyFiles copyFiles = new CopyFiles("../*.txt");

                // When
                await copyFiles.ExecuteAsync(Inputs, Context).ToListAsync();

                // Then
                Assert.IsFalse(await (await Engine.FileSystem.GetOutputFileAsync("test-a.txt")).GetExistsAsync());
                Assert.IsFalse(await (await Engine.FileSystem.GetOutputFileAsync("test-b.txt")).GetExistsAsync());
                Assert.IsFalse(await (await Engine.FileSystem.GetOutputFileAsync("Subfolder/test-c.txt")).GetExistsAsync());
                Assert.IsFalse(await (await Engine.FileSystem.GetOutputDirectoryAsync("Subfolder")).GetExistsAsync());
                Assert.IsFalse(await (await Engine.FileSystem.GetOutputFileAsync("markdown-x.md")).GetExistsAsync());
                Assert.IsFalse(await (await Engine.FileSystem.GetOutputFileAsync("Subfolder/markdown-y.md")).GetExistsAsync());
                Assert.IsTrue(await (await Engine.FileSystem.GetOutputFileAsync("test-above-input.txt")).GetExistsAsync());
            }

            [Test]
            public async Task CopyFilesAboveInputPathWithOthers()
            {
                // Given
                CopyFiles copyFiles = new CopyFiles("../**/*.txt");

                // When
                await copyFiles.ExecuteAsync(Inputs, Context).ToListAsync();

                // Then
                Assert.IsTrue(await (await Engine.FileSystem.GetOutputFileAsync("test-a.txt")).GetExistsAsync());
                Assert.IsTrue(await (await Engine.FileSystem.GetOutputFileAsync("test-b.txt")).GetExistsAsync());
                Assert.IsTrue(await (await Engine.FileSystem.GetOutputFileAsync("Subfolder/test-c.txt")).GetExistsAsync());
                Assert.IsTrue(await (await Engine.FileSystem.GetOutputDirectoryAsync("Subfolder")).GetExistsAsync());
                Assert.IsFalse(await (await Engine.FileSystem.GetOutputFileAsync("markdown-x.md")).GetExistsAsync());
                Assert.IsFalse(await (await Engine.FileSystem.GetOutputFileAsync("Subfolder/markdown-y.md")).GetExistsAsync());
                Assert.IsTrue(await (await Engine.FileSystem.GetOutputFileAsync("test-above-input.txt")).GetExistsAsync());
            }

            [Test]
            public async Task CopyFolderFromAbsolutePath()
            {
                // Given
                CopyFiles copyFiles = new CopyFiles("/TestFiles/Input/**/*.txt");

                // When
                await copyFiles.ExecuteAsync(Inputs, Context).ToListAsync();

                // Then
                Assert.IsTrue(await (await Engine.FileSystem.GetOutputFileAsync("test-a.txt")).GetExistsAsync());
                Assert.IsTrue(await (await Engine.FileSystem.GetOutputFileAsync("test-b.txt")).GetExistsAsync());
                Assert.IsTrue(await (await Engine.FileSystem.GetOutputFileAsync("Subfolder/test-c.txt")).GetExistsAsync());
                Assert.IsTrue(await (await Engine.FileSystem.GetOutputDirectoryAsync("Subfolder")).GetExistsAsync());
                Assert.IsFalse(await (await Engine.FileSystem.GetOutputFileAsync("markdown-x.md")).GetExistsAsync());
                Assert.IsFalse(await (await Engine.FileSystem.GetOutputFileAsync("Subfolder/markdown-y.md")).GetExistsAsync());
            }

            [Test]
            public async Task CopyNonExistingFolder()
            {
                // Given
                CopyFiles copyFiles = new CopyFiles("NonExisting/**/*.txt");

                // When
                await copyFiles.ExecuteAsync(Inputs, Context).ToListAsync();

                // Then
                Assert.IsFalse(await (await Engine.FileSystem.GetOutputFileAsync("test-a.txt")).GetExistsAsync());
                Assert.IsFalse(await (await Engine.FileSystem.GetOutputFileAsync("test-b.txt")).GetExistsAsync());
                Assert.IsFalse(await (await Engine.FileSystem.GetOutputFileAsync("Subfolder/test-c.txt")).GetExistsAsync());
                Assert.IsFalse(await (await Engine.FileSystem.GetOutputDirectoryAsync("Subfolder")).GetExistsAsync());
                Assert.IsFalse(await (await Engine.FileSystem.GetOutputFileAsync("markdown-x.md")).GetExistsAsync());
                Assert.IsFalse(await (await Engine.FileSystem.GetOutputFileAsync("Subfolder/markdown-y.md")).GetExistsAsync());
            }

            [Test]
            public async Task ShouldSetMetadata()
            {
                // Given
                CopyFiles copyFiles = new CopyFiles("**/test-a.txt");

                // When
                await copyFiles.ExecuteAsync(Inputs, Context).ToListAsync();
            }

            [TestCase(Keys.SourceFilePath, "/TestFiles/Input/test-a.txt")]
            [TestCase(Keys.DestinationFilePath, "/output/test-a.txt")]
            public async Task ShouldSetFilePathMetadata(string key, string expected)
            {
                // Given
                CopyFiles copyFiles = new CopyFiles("**/test-a.txt");

                // When
                IDocument output = (await copyFiles.ExecuteAsync(Inputs, Context)).First();

                // Then
                object result = output[key];
                Assert.IsInstanceOf<FilePath>(result);
                Assert.AreEqual(expected, ((FilePath)result).FullPath);
            }
        }
    }
}
