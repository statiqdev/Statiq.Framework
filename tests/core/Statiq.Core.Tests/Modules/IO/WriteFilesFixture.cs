using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;
using Statiq.Testing.Documents;
using Statiq.Testing.Execution;
using Statiq.Testing.IO;

namespace Statiq.Core.Tests.Modules.IO
{
    [TestFixture]
    public class WriteFilesFixture : BaseFixture
    {
        public class ExecuteTests : WriteFilesFixture
        {
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

                TestDocument input = new TestDocument(
                    new FilePath(fileName),
                    newContent);
                WriteFiles writeFiles = new WriteFiles();

                // When
                await ExecuteAsync(input, context, writeFiles);

                // Then
                IFile outputFile = await context.FileSystem.GetOutputFileAsync(fileName);
                (await outputFile.ReadAllTextAsync()).ShouldBe(newContent);
            }

            [Test]
            public async Task OutputDocumentContainsSameContent()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                TestDocument input = new TestDocument("Test");
                WriteFiles writeFiles = new WriteFiles();

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
                WriteFiles writeFiles = new WriteFiles().Where(false);

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
                TestDocument[] inputs = new[] { "A", "B", "C", "D", "E" }
                    .Select(x => new TestDocument(new FilePath("output.txt"), x))
                    .ToArray();
                WriteFiles writeFiles = new WriteFiles();

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
                TestDocument[] inputs = new[] { "A", "B", "C", "D", "E" }
                    .Select(x => new TestDocument(new FilePath("output.txt"), x))
                    .ToArray();
                WriteFiles writeFiles = new WriteFiles();

                // When, Then
                await Should.ThrowAsync<Exception>(async () => await ExecuteAsync(inputs, context, writeFiles));
            }

            [Test]
            public async Task InputDocumentsAreEvaluatedInOrderWhenAppending()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                TestDocument[] inputs = new[] { "A", "B", "C", "D", "E" }
                    .Select(x => new TestDocument(new FilePath("output.txt"), x))
                    .ToArray();
                WriteFiles writeFiles = new WriteFiles().Append();

                // When
                await ExecuteAsync(inputs, context, writeFiles);

                // Then
                IFile outputFile = await context.FileSystem.GetOutputFileAsync("output.txt");
                (await outputFile.GetExistsAsync()).ShouldBeTrue();
                (await outputFile.ReadAllTextAsync()).ShouldBe("ABCDE");
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
                        new FilePath("Subfolder/write-test"),
                        "Test"),
                    new TestDocument(
                        new FilePath("Subfolder/empty-test"),
                        string.Empty),
                    new TestDocument(
                        new FilePath("Subfolder/stream-test"),
                        emptyStream)
                };
                WriteFiles writeFiles = new WriteFiles();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(inputs, context, writeFiles);

                // Then
                results.Count.ShouldBe(3);
                (await (await context.FileSystem.GetOutputFileAsync("Subfolder/write-test")).GetExistsAsync()).ShouldBeTrue();
                (await (await context.FileSystem.GetOutputFileAsync("output/Subfolder/empty-test")).GetExistsAsync()).ShouldBeFalse();
                (await (await context.FileSystem.GetOutputFileAsync("output/Subfolder/stream-test")).GetExistsAsync()).ShouldBeFalse();
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
