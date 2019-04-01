using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Core.Execution;
using Wyam.Core.Modules.IO;
using Wyam.Testing;
using Wyam.Testing.Execution;
using Wyam.Testing.IO;

namespace Wyam.Core.Tests.Modules.IO
{
    [TestFixture]
    [NonParallelizable]
    public class WriteFilesFixture : BaseFixture
    {
        private Engine Engine { get; set; }
        private ExecutionPipeline Pipeline { get; set; }
        private IExecutionContext Context { get; set; }

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

        public class ExecuteTests : WriteFilesFixture
        {
            [Test]
            public async Task ExtensionWithDotWritesFiles()
            {
                // Given
                Engine.Settings[Keys.RelativeFilePath] = new FilePath("Subfolder/write-test.abc");
                IDocument[] inputs = new[] { await Context.GetDocumentAsync("Test") };
                WriteFiles writeFiles = new WriteFiles(".txt");

                // When
                writeFiles.Execute(inputs, Context).ToList();

                // Then
                IFile outputFile = await Engine.FileSystem.GetOutputFileAsync("Subfolder/write-test.txt");
                Assert.IsTrue(await outputFile.GetExistsAsync());
                Assert.AreEqual("Test", await outputFile.ReadAllTextAsync());
            }

            [Test]
            public async Task ExtensionWithoutDotWritesFiles()
            {
                // Given
                Engine.Settings[Keys.RelativeFilePath] = new FilePath("Subfolder/write-test.abc");
                IDocument[] inputs = new[] { await Context.GetDocumentAsync("Test") };
                WriteFiles writeFiles = new WriteFiles("txt");

                // When
                writeFiles.Execute(inputs, Context).ToList();

                // Then
                IFile outputFile = await Engine.FileSystem.GetOutputFileAsync("Subfolder/write-test.txt");
                Assert.IsTrue(await outputFile.GetExistsAsync());
                Assert.AreEqual("Test", await outputFile.ReadAllTextAsync());
            }

            [Test]
            public async Task ShouldWriteDotFile()
            {
                // Given
                IDocument[] inputs = new[] { await Context.GetDocumentAsync("Test") };
                WriteFiles writeFiles = new WriteFiles((x, y) => ".dotfile");

                // When
                writeFiles.Execute(inputs, Context).ToList();

                // Then
                IFile outputFile = await Engine.FileSystem.GetOutputFileAsync(".dotfile");
                Assert.IsTrue(await outputFile.GetExistsAsync());
                Assert.AreEqual("Test", await outputFile.ReadAllTextAsync());
            }

            [Test]
            public async Task ShouldTruncateOldFileOnWrite()
            {
                // Given
                const string fileName = "test.txt";
                const string oldContent = "TestTest";
                const string newContent = "Test";

                IFile fileMock = await Engine.FileSystem.GetOutputFileAsync(fileName);
                await fileMock.WriteAllTextAsync(oldContent);

                WriteFiles writeFiles = new WriteFiles((x, y) => fileName);
                IDocument[] inputs = { await Context.GetDocumentAsync(newContent) };

                // When
                writeFiles.Execute(inputs, Context).ToList();

                // Then
                IFile outputFile = await Engine.FileSystem.GetOutputFileAsync(fileName);
                Assert.AreEqual(newContent, await outputFile.ReadAllTextAsync());
            }

            [Test]
            public async Task ShouldReturnNullBasePathsForDotFiles()
            {
                // Given
                IDocument[] inputs = new[] { await Context.GetDocumentAsync("Test") };
                WriteFiles writeFiles = new WriteFiles((x, y) => ".dotfile");

                // When
                IDocument document = writeFiles.Execute(inputs, Context).ToList().First();

                // Then
                Assert.IsNull(document[Keys.DestinationFileBase]);
                Assert.IsNull(document[Keys.DestinationFilePathBase]);
                Assert.IsNull(document[Keys.RelativeFilePathBase]);
            }

            [Test]
            public async Task OutputDocumentContainsSameContent()
            {
                // Given
                IDocument[] inputs = new[] { await Context.GetDocumentAsync("Test") };
                WriteFiles writeFiles = new WriteFiles((x, y) => null);

                // When
                IDocument output = writeFiles.Execute(inputs, Context).ToList().First();

                // Then
                Assert.AreEqual("Test", output.Content);
            }

            [Test]
            public async Task ShouldReturnOriginalDocumentForFailedPredicate()
            {
                // Given
                IDocument[] inputs = new[] { await Context.GetDocumentAsync("Test") };
                WriteFiles writeFiles = new WriteFiles((x, y) => null).Where((x, y) => false);

                // When
                IDocument output = writeFiles.Execute(inputs, Context).ToList().First();

                // Then
                Assert.AreEqual("Test", output.Content);
            }

            [Test]
            public async Task InputDocumentsAreEvaluatedInOrderWhenOverwritting()
            {
                // Given
                ThrowOnTraceEventType(TraceEventType.Error);
                IDocument[] inputs = new[]
                {
                    await Context.GetDocumentAsync("A"),
                    await Context.GetDocumentAsync("B"),
                    await Context.GetDocumentAsync("C"),
                    await Context.GetDocumentAsync("D"),
                    await Context.GetDocumentAsync("E"),
                };
                WriteFiles writeFiles = new WriteFiles((x, y) => "output.txt");

                // When
                writeFiles.Execute(inputs, Context).ToList();

                // Then
                IFile outputFile = await Engine.FileSystem.GetOutputFileAsync("output.txt");
                Assert.IsTrue(await outputFile.GetExistsAsync());
                Assert.AreEqual("E", await outputFile.ReadAllTextAsync());
            }

            [Test]
            public async Task DocumentsWithSameOutputGeneratesWarning()
            {
                // Given
                IDocument[] inputs = new[]
                {
                    await Context.GetDocumentAsync(new FilePath("/a.txt"), "A"),
                    await Context.GetDocumentAsync(new FilePath("/b.txt"), "B"),
                    await Context.GetDocumentAsync(new FilePath("/c.txt"), "C"),
                    await Context.GetDocumentAsync(new FilePath("/d.txt"), "D"),
                    await Context.GetDocumentAsync(new FilePath("/e.txt"), "E"),
                };
                WriteFiles writeFiles = new WriteFiles((x, y) => "output.txt");

                // When, Then
                Assert.Throws<Exception>(() => writeFiles.Execute(inputs, Context).ToList(), @"Multiple documents output to output.txt (this probably wasn't intended):
  /a.txt
  /b.txt
  /c.txt
  /d.txt
  /e.txt");
            }

            [Test]
            public async Task InputDocumentsAreEvaluatedInOrderWhenAppending()
            {
                // Given
                IDocument[] inputs = new[]
                {
                    await Context.GetDocumentAsync("A"),
                    await Context.GetDocumentAsync("B"),
                    await Context.GetDocumentAsync("C"),
                    await Context.GetDocumentAsync("D"),
                    await Context.GetDocumentAsync("E"),
                };
                WriteFiles writeFiles = new WriteFiles((x, y) => "output.txt").Append();

                // When
                writeFiles.Execute(inputs, Context).ToList();

                // Then
                IFile outputFile = await Engine.FileSystem.GetOutputFileAsync("output.txt");
                Assert.IsTrue(await outputFile.GetExistsAsync());
                Assert.AreEqual("ABCDE", await outputFile.ReadAllTextAsync());
            }

            [TestCase(Keys.DestinationFileBase, "write-test")]
            [TestCase(Keys.DestinationFileName, "write-test.txt")]
            [TestCase(Keys.DestinationFilePath, "/output/Subfolder/write-test.txt")]
            [TestCase(Keys.DestinationFilePathBase, "/output/Subfolder/write-test")]
            [TestCase(Keys.RelativeFilePath, "Subfolder/write-test.txt")]
            [TestCase(Keys.RelativeFilePathBase, "Subfolder/write-test")]
            public async Task ShouldSetFilePathMetadata(string key, string expected)
            {
                // Given
                Engine.Settings[Keys.RelativeFilePath] = new FilePath("Subfolder/write-test.abc");
                IDocument[] inputs = new[] { await Context.GetDocumentAsync("Test") };
                WriteFiles writeFiles = new WriteFiles(".txt");

                // When
                IDocument output = writeFiles.Execute(inputs, Context).ToList().First();

                // Then
                object result = output[key];
                Assert.IsInstanceOf<FilePath>(result);
                Assert.AreEqual(expected, ((FilePath)result).FullPath);
            }

            [TestCase(Keys.DestinationFileDir, "/output/Subfolder")]
            [TestCase(Keys.RelativeFileDir, "Subfolder")]
            public async Task ShouldSetDirectoryPathMetadata(string key, string expected)
            {
                // Given
                Engine.Settings[Keys.RelativeFilePath] = new FilePath("Subfolder/write-test.abc");
                IDocument[] inputs = new[] { await Context.GetDocumentAsync("Test") };
                WriteFiles writeFiles = new WriteFiles(".txt");

                // When
                IDocument output = writeFiles.Execute(inputs, Context).ToList().First();

                // Then
                object result = output[key];
                Assert.IsInstanceOf<DirectoryPath>(result);
                Assert.AreEqual(expected, ((DirectoryPath)result).FullPath);
            }

            [TestCase(Keys.DestinationFileExt, ".txt")]
            public async Task ShouldSetStringMetadata(string key, string expected)
            {
                // Given
                Engine.Settings[Keys.RelativeFilePath] = new FilePath("Subfolder/write-test.abc");
                IDocument[] inputs = new[] { await Context.GetDocumentAsync("Test") };
                WriteFiles writeFiles = new WriteFiles(".txt");

                // When
                IDocument output = writeFiles.Execute(inputs, Context).ToList().First();

                // Then
                object result = output[key];
                Assert.IsInstanceOf<string>(result);
                Assert.AreEqual(expected, result);
            }

            [Test]
            public async Task IgnoresEmptyDocuments()
            {
                // Given
                MemoryStream emptyStream = new MemoryStream(new byte[] { });
                IDocument[] inputs =
                {
                    await Context.GetDocumentAsync(
                        "Test",
                        new MetadataItems
                        {
                            new MetadataItem(Keys.RelativeFilePath, new FilePath("Subfolder/write-test"))
                        }),
                    await Context.GetDocumentAsync(
                        string.Empty,
                        new MetadataItems
                        {
                            new MetadataItem(Keys.RelativeFilePath, new FilePath("Subfolder/empty-test")),
                        }),
                    Context.GetDocument(
                        (Stream)null,
                        new MetadataItems
                        {
                            new MetadataItem(Keys.RelativeFilePath, new FilePath("Subfolder/null-test"))
                        }),
                    Context.GetDocument(
                        emptyStream,
                        new MetadataItems
                        {
                            new MetadataItem(Keys.RelativeFilePath, new FilePath("Subfolder/stream-test"))
                        })
                };
                WriteFiles writeFiles = new WriteFiles();

                // When
                IEnumerable<IDocument> outputs = writeFiles.Execute(inputs, Context).ToList();

                // Then
                Assert.AreEqual(4, outputs.Count());
                Assert.IsTrue(await (await Context.FileSystem.GetOutputFileAsync("Subfolder/write-test")).GetExistsAsync());
                Assert.IsFalse(await (await Context.FileSystem.GetOutputFileAsync("output/Subfolder/empty-test")).GetExistsAsync());
                Assert.IsFalse(await (await Context.FileSystem.GetOutputFileAsync("output/Subfolder/null-test")).GetExistsAsync());
                Assert.IsFalse(await (await Context.FileSystem.GetOutputFileAsync("output/Subfolder/stream-test")).GetExistsAsync());
            }
        }
    }
}
