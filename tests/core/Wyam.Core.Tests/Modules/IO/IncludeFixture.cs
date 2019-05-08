using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Core.Modules.IO;
using Wyam.Testing;
using Wyam.Testing.Execution;
using Wyam.Testing.IO;

namespace Wyam.Core.Tests.Modules.IO
{
    [TestFixture]
    [NonParallelizable]
    public class IncludeFixture : BaseFixture
    {
        public class ExecuteTests : IncludeFixture
        {
            [Test]
            public async Task ThrowForNoSourceWhenIncludingRelativePath()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                IDocument[] documents =
                {
                    await context.GetDocumentAsync(content: "foo ^\"test-a.txt\" bar")
                };
                Include include = new Include();

                // When, Then
                await Should.ThrowAsync<Exception>(async () => await ExecuteAsync(documents, context, include));
            }

            [Test]
            public async Task DoesNotThrowForNoSourceWhenIncludingAbsolutePath()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                IDocument[] documents =
                {
                    await context.GetDocumentAsync(content: "foo ^\"/TestFiles/Input/test-a.txt\" bar")
                };
                Include include = new Include();

                // When, Then
                IReadOnlyList<IDocument> results = await ExecuteAsync(documents, context, include);

                // Then
                Assert.AreEqual("foo aaa bar", results.Single().Content);
            }

            [Test]
            public async Task IncludeOnFirstCharacter()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                IDocument[] documents =
                {
                    await context.GetDocumentAsync(source: new FilePath("/TestFiles/Input/test.txt"), content: "^\"test-a.txt\"foo")
                };
                Include include = new Include();

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(documents, context, include);

                // Then
                Assert.AreEqual("aaafoo", results.Single().Content);
            }

            [Test]
            public async Task DoesNotIncludeOnFirstCharacterIfEscaped()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                IDocument[] documents =
                {
                    await context.GetDocumentAsync(source: new FilePath("/TestFiles/Input/test.txt"), content: "\\^\"test-a.txt\"foo")
                };
                Include include = new Include();

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(documents, context, include);

                // Then
                Assert.AreEqual("^\"test-a.txt\"foo", results.Single().Content);
            }

            [Test]
            public async Task MultipleEscapeCharacters()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                IDocument[] documents =
                {
                    await context.GetDocumentAsync(source: new FilePath("/TestFiles/Input/test.txt"), content: "\\\\\\^\"test-a.txt\"foo")
                };
                Include include = new Include();

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(documents, context, include);

                // Then
                Assert.AreEqual("\\\\^\"test-a.txt\"foo", results.Single().Content);
            }

            [Test]
            public async Task MultipleIncludes()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                IDocument[] documents =
                {
                    await context.GetDocumentAsync(
                        source: new FilePath("/TestFiles/Input/test.txt"),
                        content: "x ^\"test-a.txt\" y ^\"test-b.txt\" z")
                };
                Include include = new Include();

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(documents, context, include);

                // Then
                Assert.AreEqual("x aaa y bbb z", results.Single().Content);
            }

            [Test]
            public async Task MultipleAdjacentIncludes()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                IDocument[] documents =
                {
                    await context.GetDocumentAsync(
                        source: new FilePath("/TestFiles/Input/test.txt"),
                        content: "x ^\"test-a.txt\"^\"test-b.txt\" z")
                };
                Include include = new Include();

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(documents, context, include);

                // Then
                Assert.AreEqual("x aaabbb z", results.Single().Content);
            }

            [Test]
            public async Task FileNotFoundRemovesIncludeStatement()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                ThrowOnTraceEventType(System.Diagnostics.TraceEventType.Error);
                IDocument[] documents =
                {
                    await context.GetDocumentAsync(
                        source: new FilePath("/TestFiles/Input/test.txt"),
                        content: "x ^\"test-c.txt\" y")
                };
                Include include = new Include();

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(documents, context, include);

                // Then
                Assert.AreEqual("x  y", results.Single().Content);
            }

            [Test]
            public async Task IncludingRelativePath()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                IDocument[] documents =
                {
                    await context.GetDocumentAsync(
                        source: new FilePath("/TestFiles/Input/test.txt"),
                        content: "x ^\"Subfolder/test-c.txt\" y")
                };
                Include include = new Include();

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(documents, context, include);

                // Then
                Assert.AreEqual("x ccc y", results.Single().Content);
            }

            [Test]
            public async Task IncludingRelativePathOutsideInput()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                IDocument[] documents =
                {
                    await context.GetDocumentAsync(
                        source: new FilePath("/TestFiles/Input/test.txt"),
                        content: "x ^\"../test-above-input.txt\" y")
                };
                Include include = new Include();

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(documents, context, include);

                // Then
                Assert.AreEqual("x test y", results.Single().Content);
            }

            [Test]
            public async Task IncludingAbsolutePath()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                IDocument[] documents =
                {
                    await context.GetDocumentAsync(
                        source: new FilePath("/TestFiles/Input/test.txt"),
                        content: "x ^\"/TestFiles/test-above-input.txt\" y")
                };
                Include include = new Include();

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(documents, context, include);

                // Then
                Assert.AreEqual("x test y", results.Single().Content);
            }

            [Test]
            public async Task NestedInclude()
            {
                // Given
                TestExecutionContext context = GetExecutionContext(out TestFileProvider fileProvider);
                fileProvider.AddFile(
                    "/TestFiles/Input/test-outer.txt",
                    "3 ^\"test-a.txt\" 4");
                IDocument[] documents =
                {
                    await context.GetDocumentAsync(
                        source: new FilePath("/TestFiles/Input/test.txt"),
                        content: "1 ^\"test-outer.txt\" 2")
                };
                Include include = new Include();

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(documents, context, include);

                // Then
                Assert.AreEqual("1 3 aaa 4 2", results.Single().Content);
            }

            [Test]
            public async Task NestedIncludeWithoutRecursion()
            {
                // Given
                TestExecutionContext context = GetExecutionContext(out TestFileProvider fileProvider);
                fileProvider.AddFile(
                    "/TestFiles/Input/test-outer.txt",
                    "3 ^\"test-a.txt\" 4");
                IDocument[] documents =
                {
                    await context.GetDocumentAsync(
                        source: new FilePath("/TestFiles/Input/test.txt"),
                        content: "1 ^\"test-outer.txt\" 2")
                };
                Include include = new Include().WithRecursion(false);

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(documents, context, include);

                // Then
                Assert.AreEqual("1 3 ^\"test-a.txt\" 4 2", results.Single().Content);
            }

            [Test]
            public async Task NestedIncludeWithInnerEscape()
            {
                // Given
                TestExecutionContext context = GetExecutionContext(out TestFileProvider fileProvider);
                fileProvider.AddFile(
                    "/TestFiles/Input/test-outer.txt",
                    "3 \\^\"test-a.txt\" 4");
                IDocument[] documents =
                {
                    await context.GetDocumentAsync(
                        source: new FilePath("/TestFiles/Input/test.txt"),
                        content: "1 ^\"test-outer.txt\" 2")
                };
                Include include = new Include().WithRecursion(false);

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(documents, context, include);

                // Then
                Assert.AreEqual("1 3 \\^\"test-a.txt\" 4 2", results.Single().Content);
            }

            [Test]
            public async Task MultipleNestedInclude()
            {
                // Given
                TestExecutionContext context = GetExecutionContext(out TestFileProvider fileProvider);
                fileProvider.AddFile(
                    "/TestFiles/Input/test-outer.txt",
                    "3 ^\"test-inner.txt\" 4");
                fileProvider.AddFile(
                    "/TestFiles/Input/test-inner.txt",
                    "5 ^\"test-a.txt\" 6");
                IDocument[] documents =
                {
                    await context.GetDocumentAsync(
                        source: new FilePath("/TestFiles/Input/test.txt"),
                        content: "1 ^\"test-outer.txt\" 2")
                };
                Include include = new Include();

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(documents, context, include);

                // Then
                Assert.AreEqual("1 3 5 aaa 6 4 2", results.Single().Content);
            }
        }

        protected static TestExecutionContext GetExecutionContext() => GetExecutionContext(out _);

        protected static TestExecutionContext GetExecutionContext(out TestFileProvider fileProvider)
        {
            fileProvider = new TestFileProvider();

            fileProvider.AddDirectory("/");
            fileProvider.AddDirectory("/TestFiles");
            fileProvider.AddDirectory("/TestFiles/Input");
            fileProvider.AddDirectory("/TestFiles/Input/Subfolder");

            fileProvider.AddFile("/TestFiles/test-above-input.txt", "test");
            fileProvider.AddFile("/TestFiles/Input/test-a.txt", "aaa");
            fileProvider.AddFile("/TestFiles/Input/test-b.txt", "bbb");
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
