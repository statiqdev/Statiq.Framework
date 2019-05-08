using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Util;
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
        public class ExecuteTests : WriteFilesFixture
        {
            [Test]
            public async Task ExtensionWithDotWritesFiles()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                IDocument[] inputs = new[]
                {
                    context.GetDocument(
                        new Dictionary<string, object>
                        {
                            { Keys.RelativeFilePath, new FilePath("Subfolder/write-test.abc") }
                        },
                        await context.GetContentProviderAsync("Test"))
                };
                WriteFiles writeFiles = new WriteFiles(".txt");

                // When
                await writeFiles.ExecuteAsync(inputs, context).ToListAsync();

                // Then
                IFile outputFile = await context.FileSystem.GetOutputFileAsync("Subfolder/write-test.txt");
                Assert.IsTrue(await outputFile.GetExistsAsync());
                Assert.AreEqual("Test", await outputFile.ReadAllTextAsync());
            }

            [Test]
            public async Task ExtensionWithoutDotWritesFiles()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                IDocument[] inputs = new[]
                {
                    context.GetDocument(
                        metadata: new Dictionary<string, object>
                        {
                            { Keys.RelativeFilePath, new FilePath("Subfolder/write-test.abc") }
                        },
                        await context.GetContentProviderAsync("Test"))
                };
                WriteFiles writeFiles = new WriteFiles("txt");

                // When
                await writeFiles.ExecuteAsync(inputs, context).ToListAsync();

                // Then
                IFile outputFile = await context.FileSystem.GetOutputFileAsync("Subfolder/write-test.txt");
                Assert.IsTrue(await outputFile.GetExistsAsync());
                Assert.AreEqual("Test", await outputFile.ReadAllTextAsync());
            }

            [Test]
            public async Task ShouldWriteDotFile()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                IDocument[] inputs = new[] { context.GetDocument(await context.GetContentProviderAsync("Test")) };
                WriteFiles writeFiles = new WriteFiles((FilePath)".dotfile");

                // When
                await writeFiles.ExecuteAsync(inputs, context).ToListAsync();

                // Then
                IFile outputFile = await context.FileSystem.GetOutputFileAsync(".dotfile");
                Assert.IsTrue(await outputFile.GetExistsAsync());
                Assert.AreEqual("Test", await outputFile.ReadAllTextAsync());
            }

            [Test]
            public async Task ShouldTruncateOldFileOnWrite()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                const string fileName = "test.txt";
                const string oldContent = "TestTest";
                const string newContent = "Test";

                IFile fileMock = await context.FileSystem.GetOutputFileAsync(fileName);
                await fileMock.WriteAllTextAsync(oldContent);

                WriteFiles writeFiles = new WriteFiles((FilePath)fileName);
                IDocument[] inputs = { context.GetDocument(await context.GetContentProviderAsync(newContent)) };

                // When
                await writeFiles.ExecuteAsync(inputs, context).ToListAsync();

                // Then
                IFile outputFile = await context.FileSystem.GetOutputFileAsync(fileName);
                Assert.AreEqual(newContent, await outputFile.ReadAllTextAsync());
            }

            [Test]
            public async Task ShouldReturnNullBasePathsForDotFiles()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                IDocument[] inputs = new[] { context.GetDocument(await context.GetContentProviderAsync("Test")) };
                WriteFiles writeFiles = new WriteFiles((FilePath)".dotfile");

                // When
                IDocument document = (await writeFiles.ExecuteAsync(inputs, context)).First();

                // Then
                Assert.IsNull(document[Keys.DestinationFileBase]);
                Assert.IsNull(document[Keys.DestinationFilePathBase]);
                Assert.IsNull(document[Keys.RelativeFilePathBase]);
            }

            [Test]
            public async Task OutputDocumentContainsSameContent()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                IDocument[] inputs = new[] { context.GetDocument(await context.GetContentProviderAsync("Test")) };
                WriteFiles writeFiles = new WriteFiles((FilePath)null);

                // When
                IDocument output = (await writeFiles.ExecuteAsync(inputs, context)).First();

                // Then
                Assert.AreEqual("Test", await output.GetStringAsync());
            }

            [Test]
            public async Task ShouldReturnOriginalDocumentForFailedPredicate()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                IDocument[] inputs = new[] { context.GetDocument(await context.GetContentProviderAsync("Test")) };
                WriteFiles writeFiles = new WriteFiles((FilePath)null).Where(false);

                // When
                IDocument output = (await writeFiles.ExecuteAsync(inputs, context)).First();

                // Then
                Assert.AreEqual("Test", await output.GetStringAsync());
            }

            [Test]
            public async Task InputDocumentsAreEvaluatedInOrderWhenOverwritting()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                ThrowOnTraceEventType(TraceEventType.Error);
                IDocument[] inputs = new[]
                {
                    context.GetDocument(await context.GetContentProviderAsync("A")),
                    context.GetDocument(await context.GetContentProviderAsync("B")),
                    context.GetDocument(await context.GetContentProviderAsync("C")),
                    context.GetDocument(await context.GetContentProviderAsync("D")),
                    context.GetDocument(await context.GetContentProviderAsync("E")),
                };
                WriteFiles writeFiles = new WriteFiles((FilePath)"output.txt");

                // When
                await writeFiles.ExecuteAsync(inputs, context).ToListAsync();

                // Then
                IFile outputFile = await context.FileSystem.GetOutputFileAsync("output.txt");
                Assert.IsTrue(await outputFile.GetExistsAsync());
                Assert.AreEqual("E", await outputFile.ReadAllTextAsync());
            }

            [Test]
            public async Task DocumentsWithSameOutputGeneratesWarning()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                IDocument[] inputs = new[]
                {
                    context.GetDocument(new FilePath("/a.txt"), null, await context.GetContentProviderAsync("A")),
                    context.GetDocument(new FilePath("/b.txt"), null, await context.GetContentProviderAsync("B")),
                    context.GetDocument(new FilePath("/c.txt"), null, await context.GetContentProviderAsync("C")),
                    context.GetDocument(new FilePath("/d.txt"), null, await context.GetContentProviderAsync("D")),
                    context.GetDocument(new FilePath("/e.txt"), null, await context.GetContentProviderAsync("E")),
                };
                WriteFiles writeFiles = new WriteFiles((FilePath)"output.txt");

                // When, Then
                await Should.ThrowAsync<Exception>(async () => await writeFiles.ExecuteAsync(inputs, context).ToListAsync());
            }

            [Test]
            public async Task InputDocumentsAreEvaluatedInOrderWhenAppending()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                IDocument[] inputs = new[]
                {
                    context.GetDocument(await context.GetContentProviderAsync("A")),
                    context.GetDocument(await context.GetContentProviderAsync("B")),
                    context.GetDocument(await context.GetContentProviderAsync("C")),
                    context.GetDocument(await context.GetContentProviderAsync("D")),
                    context.GetDocument(await context.GetContentProviderAsync("E")),
                };
                WriteFiles writeFiles = new WriteFiles((FilePath)"output.txt").Append();

                // When
                await writeFiles.ExecuteAsync(inputs, context).ToListAsync();

                // Then
                IFile outputFile = await context.FileSystem.GetOutputFileAsync("output.txt");
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
                TestExecutionContext context = GetExecutionContext();
                IDocument[] inputs = new[]
                {
                    context.GetDocument(
                        metadata: new Dictionary<string, object>
                        {
                            { Keys.RelativeFilePath, new FilePath("Subfolder/write-test.abc") }
                        },
                        await context.GetContentProviderAsync("Test"))
                };
                WriteFiles writeFiles = new WriteFiles(".txt");

                // When
                IDocument output = (await writeFiles.ExecuteAsync(inputs, context)).First();

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
                TestExecutionContext context = GetExecutionContext();
                IDocument[] inputs = new[]
                {
                    context.GetDocument(
                        metadata: new Dictionary<string, object>
                        {
                            { Keys.RelativeFilePath, new FilePath("Subfolder/write-test.abc") }
                        },
                        await context.GetContentProviderAsync("Test"))
                };
                WriteFiles writeFiles = new WriteFiles(".txt");

                // When
                IDocument output = (await writeFiles.ExecuteAsync(inputs, context)).First();

                // Then
                object result = output[key];
                Assert.IsInstanceOf<DirectoryPath>(result);
                Assert.AreEqual(expected, ((DirectoryPath)result).FullPath);
            }

            [TestCase(Keys.DestinationFileExt, ".txt")]
            public async Task ShouldSetStringMetadata(string key, string expected)
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                IDocument[] inputs = new[]
                {
                    context.GetDocument(
                        metadata: new Dictionary<string, object>
                        {
                            { Keys.RelativeFilePath, new FilePath("Subfolder/write-test.abc") }
                        },
                        await context.GetContentProviderAsync("Test"))
                };
                WriteFiles writeFiles = new WriteFiles(".txt");

                // When
                IDocument output = (await writeFiles.ExecuteAsync(inputs, context)).First();

                // Then
                object result = output[key];
                Assert.IsInstanceOf<string>(result);
                Assert.AreEqual(expected, result);
            }

            [Test]
            public async Task IgnoresEmptyDocuments()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                MemoryStream emptyStream = new MemoryStream(new byte[] { });
                IDocument[] inputs =
                {
                    context.GetDocument(
                        new MetadataItems
                        {
                            new MetadataItem(Keys.RelativeFilePath, new FilePath("Subfolder/write-test"))
                        },
                        await context.GetContentProviderAsync("Test")),
                    context.GetDocument(
                        new MetadataItems
                        {
                            new MetadataItem(Keys.RelativeFilePath, new FilePath("Subfolder/empty-test")),
                        },
                        await context.GetContentProviderAsync(string.Empty)),
                    context.GetDocument(
                        new MetadataItems
                        {
                            new MetadataItem(Keys.RelativeFilePath, new FilePath("Subfolder/null-test"))
                        },
                        await context.GetContentProviderAsync((Stream)null)),
                    context.GetDocument(
                        new MetadataItems
                        {
                            new MetadataItem(Keys.RelativeFilePath, new FilePath("Subfolder/stream-test"))
                        },
                        await context.GetContentProviderAsync(emptyStream))
                };
                WriteFiles writeFiles = new WriteFiles();

                // When
                IEnumerable<IDocument> outputs = await writeFiles.ExecuteAsync(inputs, context).ToListAsync();

                // Then
                Assert.AreEqual(4, outputs.Count());
                Assert.IsTrue(await (await context.FileSystem.GetOutputFileAsync("Subfolder/write-test")).GetExistsAsync());
                Assert.IsFalse(await (await context.FileSystem.GetOutputFileAsync("output/Subfolder/empty-test")).GetExistsAsync());
                Assert.IsFalse(await (await context.FileSystem.GetOutputFileAsync("output/Subfolder/null-test")).GetExistsAsync());
                Assert.IsFalse(await (await context.FileSystem.GetOutputFileAsync("output/Subfolder/stream-test")).GetExistsAsync());
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
