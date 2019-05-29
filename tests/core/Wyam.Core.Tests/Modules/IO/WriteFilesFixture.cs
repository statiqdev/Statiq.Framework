using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Core.Modules.IO;
using Wyam.Testing;
using Wyam.Testing.Documents;
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
            /*
            [Test]
            public async Task ExtensionWithoutDotWritesFiles()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                TestDocument input = new TestDocument(
                    new Dictionary<string, object>
                    {
                        { Keys.RelativeFilePath, new FilePath("Subfolder/write-test.abc") }
                    },
                    "Test");
                WriteFiles writeFiles = new WriteFiles("txt");

                // When
                await ExecuteAsync(input, context, writeFiles);

                // Then
                IFile outputFile = await context.FileSystem.GetOutputFileAsync("Subfolder/write-test.txt");
                (await outputFile.GetExistsAsync()).ShouldBeTrue();
                (await outputFile.ReadAllTextAsync()).ShouldBe("Test");
            }

            [Test]
            public async Task ShouldWriteDotFile()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                TestDocument input = new TestDocument("Test");
                WriteFiles writeFiles = new WriteFiles((FilePath)".dotfile");

                // When
                await ExecuteAsync(input, context, writeFiles);

                // Then
                IFile outputFile = await context.FileSystem.GetOutputFileAsync(".dotfile");
                (await outputFile.GetExistsAsync()).ShouldBeTrue();
                (await outputFile.ReadAllTextAsync()).ShouldBe("Test");
            }

            [Test]
            public async Task ShouldTruncateOldFileOnWrite()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                const string fileName = "test.txt";
                const string oldContent = "TestTest";
                const string newContent = "Test";

                IFile file = await context.FileSystem.GetOutputFileAsync(fileName);
                await file.WriteAllTextAsync(oldContent);

                WriteFiles writeFiles = new WriteFiles((FilePath)fileName);
                TestDocument input = new TestDocument(newContent);

                // When
                await ExecuteAsync(input, context, writeFiles);

                // Then
                IFile outputFile = await context.FileSystem.GetOutputFileAsync(fileName);
                (await outputFile.ReadAllTextAsync()).ShouldBe(newContent);
            }

            [Test]
            public async Task ShouldReturnNullBasePathsForDotFiles()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                TestDocument input = new TestDocument("Test");
                WriteFiles writeFiles = new WriteFiles((FilePath)".dotfile");

                // When
                TestDocument result = await ExecuteAsync(input, context, writeFiles).SingleAsync();

                // Then
                result[Keys.DestinationFileBase].ShouldBeNull();
                result[Keys.DestinationFilePathBase].ShouldBeNull();
                result[Keys.RelativeFilePathBase].ShouldBeNull();
            }

            [Test]
            public async Task OutputDocumentContainsSameContent()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                TestDocument input = new TestDocument("Test");
                WriteFiles writeFiles = new WriteFiles((FilePath)null);

                // When
                TestDocument result = await ExecuteAsync(input, context, writeFiles).SingleAsync();

                // Then
                result.Content.ShouldBe("Test");
            }

            [Test]
            public async Task ShouldReturnOriginalDocumentForFailedPredicate()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                TestDocument input = new TestDocument("Test");
                WriteFiles writeFiles = new WriteFiles((FilePath)null).Where(false);

                // When
                TestDocument result = await ExecuteAsync(input, context, writeFiles).SingleAsync();

                // Then
                result.ShouldBe(input);
            }

            [Test]
            public async Task InputDocumentsAreEvaluatedInOrderWhenOverwritting()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                ThrowOnTraceEventType(TraceEventType.Error);
                TestDocument[] inputs = GetDocuments("A", "B", "C", "D", "E");
                WriteFiles writeFiles = new WriteFiles((FilePath)"output.txt");

                // When
                await ExecuteAsync(inputs, context, writeFiles);

                // Then
                IFile outputFile = await context.FileSystem.GetOutputFileAsync("output.txt");
                (await outputFile.GetExistsAsync()).ShouldBeTrue();
                (await outputFile.ReadAllTextAsync()).ShouldBe("E");
            }

            [Test]
            public async Task DocumentsWithSameOutputGeneratesWarning()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                TestDocument[] inputs = new[]
                {
                    new TestDocument(new FilePath("/a.txt"), null, null, await context.GetContentProviderAsync("A")),
                    new TestDocument(new FilePath("/b.txt"), null, null, await context.GetContentProviderAsync("B")),
                    new TestDocument(new FilePath("/c.txt"), null, null, await context.GetContentProviderAsync("C")),
                    new TestDocument(new FilePath("/d.txt"), null, null, await context.GetContentProviderAsync("D")),
                    new TestDocument(new FilePath("/e.txt"), null, null, await context.GetContentProviderAsync("E")),
                };
                WriteFiles writeFiles = new WriteFiles((FilePath)"output.txt");

                // When, Then
                await Should.ThrowAsync<Exception>(async () => await ExecuteAsync(inputs, context, writeFiles));
            }

            [Test]
            public async Task InputDocumentsAreEvaluatedInOrderWhenAppending()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                TestDocument[] inputs = GetDocuments("A", "B", "C", "D", "E");
                WriteFiles writeFiles = new WriteFiles((FilePath)"output.txt").Append();

                // When
                await ExecuteAsync(inputs, context, writeFiles);

                // Then
                IFile outputFile = await context.FileSystem.GetOutputFileAsync("output.txt");
                (await outputFile.GetExistsAsync()).ShouldBeTrue();
                (await outputFile.ReadAllTextAsync()).ShouldBe("ABCDE");
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
                TestDocument input = new TestDocument(
                    new Dictionary<string, object>
                    {
                        { Keys.RelativeFilePath, new FilePath("Subfolder/write-test.abc") }
                    },
                    "Test");
                WriteFiles writeFiles = new WriteFiles(".txt");

                // When
                TestDocument result = await ExecuteAsync(input, context, writeFiles).SingleAsync();

                // Then
                object value = result[key];
                value.ShouldBeOfType<FilePath>();
                ((FilePath)value).FullPath.ShouldBe(expected);
            }

            [TestCase(Keys.DestinationFileDir, "/output/Subfolder")]
            [TestCase(Keys.RelativeFileDir, "Subfolder")]
            public async Task ShouldSetDirectoryPathMetadata(string key, string expected)
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                TestDocument input = new TestDocument(
                    new Dictionary<string, object>
                    {
                        { Keys.RelativeFilePath, new FilePath("Subfolder/write-test.abc") }
                    },
                    "Test");
                WriteFiles writeFiles = new WriteFiles(".txt");

                // When
                TestDocument result = await ExecuteAsync(input, context, writeFiles).SingleAsync();

                // Then
                object value = result[key];
                value.ShouldBeOfType<DirectoryPath>();
                ((DirectoryPath)value).FullPath.ShouldBe(expected);
            }

            [TestCase(Keys.DestinationFileExt, ".txt")]
            public async Task ShouldSetStringMetadata(string key, string expected)
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                TestDocument input = new TestDocument(
                    new Dictionary<string, object>
                    {
                        { Keys.RelativeFilePath, new FilePath("Subfolder/write-test.abc") }
                    },
                    "Test");
                WriteFiles writeFiles = new WriteFiles(".txt");

                // When
                TestDocument result = await ExecuteAsync(input, context, writeFiles).SingleAsync();

                // Then
                object value = result[key];
                value.ShouldBeOfType<string>();
                value.ShouldBe(expected);
            }

            [Test]
            public async Task IgnoresEmptyDocuments()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                MemoryStream emptyStream = new MemoryStream(new byte[] { });
                TestDocument[] inputs =
                {
                    new TestDocument(
                        new MetadataItems
                        {
                            new MetadataItem(Keys.RelativeFilePath, new FilePath("Subfolder/write-test"))
                        },
                        "Test"),
                    new TestDocument(
                        new MetadataItems
                        {
                            new MetadataItem(Keys.RelativeFilePath, new FilePath("Subfolder/empty-test")),
                        },
                        string.Empty),
                    new TestDocument(
                        new MetadataItems
                        {
                            new MetadataItem(Keys.RelativeFilePath, new FilePath("Subfolder/null-test"))
                        },
                        (Stream)null),
                    new TestDocument(
                        new MetadataItems
                        {
                            new MetadataItem(Keys.RelativeFilePath, new FilePath("Subfolder/stream-test"))
                        },
                        emptyStream)
                };
                WriteFiles writeFiles = new WriteFiles();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(inputs, context, writeFiles);

                // Then
                results.Count.ShouldBe(4);
                (await (await context.FileSystem.GetOutputFileAsync("Subfolder/write-test")).GetExistsAsync()).ShouldBeTrue();
                (await (await context.FileSystem.GetOutputFileAsync("output/Subfolder/empty-test")).GetExistsAsync()).ShouldBeFalse();
                (await (await context.FileSystem.GetOutputFileAsync("output/Subfolder/null-test")).GetExistsAsync()).ShouldBeFalse();
                (await (await context.FileSystem.GetOutputFileAsync("output/Subfolder/stream-test")).GetExistsAsync()).ShouldBeFalse();
            }
            */
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
