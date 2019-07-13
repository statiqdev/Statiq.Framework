using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.IO
{
    [TestFixture]
    public class ProcessIncludesFixture : BaseFixture
    {
        public class ExecuteTests : ProcessIncludesFixture
        {
            [Test]
            public async Task ThrowForNoSourceWhenIncludingRelativePath()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                TestDocument document = new TestDocument("foo ^\"test-a.txt\" bar");
                ProcessIncludes include = new ProcessIncludes();

                // When, Then
                await Should.ThrowAsync<Exception>(async () => await ExecuteAsync(document, context, include));
            }

            [Test]
            public async Task DoesNotThrowForNoSourceWhenIncludingAbsolutePath()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                TestDocument document = new TestDocument("foo ^\"/TestFiles/Input/test-a.txt\" bar");
                ProcessIncludes include = new ProcessIncludes();

                // When, Then
                TestDocument result = await ExecuteAsync(document, context, include).SingleAsync();

                // Then
                result.Content.ShouldBe("foo aaa bar");
            }

            [Test]
            public async Task IncludeOnFirstCharacter()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                TestDocument document = new TestDocument(new FilePath("/TestFiles/Input/test.txt"), "^\"test-a.txt\"foo");
                ProcessIncludes include = new ProcessIncludes();

                // When
                TestDocument result = await ExecuteAsync(document, context, include).SingleAsync();

                // Then
                result.Content.ShouldBe("aaafoo");
            }

            [Test]
            public async Task DoesNotIncludeOnFirstCharacterIfEscaped()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                TestDocument document = new TestDocument(new FilePath("/TestFiles/Input/test.txt"), "\\^\"test-a.txt\"foo");
                ProcessIncludes include = new ProcessIncludes();

                // When
                TestDocument result = await ExecuteAsync(document, context, include).SingleAsync();

                // Then
                result.Content.ShouldBe("^\"test-a.txt\"foo");
            }

            [Test]
            public async Task MultipleEscapeCharacters()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                TestDocument document = new TestDocument(new FilePath("/TestFiles/Input/test.txt"), "\\\\\\^\"test-a.txt\"foo");
                ProcessIncludes include = new ProcessIncludes();

                // When
                TestDocument result = await ExecuteAsync(document, context, include).SingleAsync();

                // Then
                result.Content.ShouldBe("\\\\^\"test-a.txt\"foo");
            }

            [Test]
            public async Task MultipleIncludes()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                TestDocument document = new TestDocument(new FilePath("/TestFiles/Input/test.txt"), "x ^\"test-a.txt\" y ^\"test-b.txt\" z");
                ProcessIncludes include = new ProcessIncludes();

                // When
                TestDocument result = await ExecuteAsync(document, context, include).SingleAsync();

                // Then
                result.Content.ShouldBe("x aaa y bbb z");
            }

            [Test]
            public async Task MultipleAdjacentIncludes()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                TestDocument document = new TestDocument(new FilePath("/TestFiles/Input/test.txt"), "x ^\"test-a.txt\"^\"test-b.txt\" z");
                ProcessIncludes include = new ProcessIncludes();

                // When
                TestDocument result = await ExecuteAsync(document, context, include).SingleAsync();

                // Then
                result.Content.ShouldBe("x aaabbb z");
            }

            [Test]
            public async Task FileNotFoundRemovesIncludeStatement()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                ThrowOnTraceEventType(System.Diagnostics.TraceEventType.Error);
                TestDocument document = new TestDocument(new FilePath("/TestFiles/Input/test.txt"), "x ^\"test-c.txt\" y");
                ProcessIncludes include = new ProcessIncludes();

                // When
                TestDocument result = await ExecuteAsync(document, context, include).SingleAsync();

                // Then
                result.Content.ShouldBe("x  y");
            }

            [Test]
            public async Task IncludingRelativePath()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                TestDocument document = new TestDocument(new FilePath("/TestFiles/Input/test.txt"), "x ^\"Subfolder/test-c.txt\" y");
                ProcessIncludes include = new ProcessIncludes();

                // When
                TestDocument result = await ExecuteAsync(document, context, include).SingleAsync();

                // Then
                result.Content.ShouldBe("x ccc y");
            }

            [Test]
            public async Task IncludingRelativePathOutsideInput()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                TestDocument document = new TestDocument(new FilePath("/TestFiles/Input/test.txt"), "x ^\"../test-above-input.txt\" y");
                ProcessIncludes include = new ProcessIncludes();

                // When
                TestDocument result = await ExecuteAsync(document, context, include).SingleAsync();

                // Then
                result.Content.ShouldBe("x test y");
            }

            [Test]
            public async Task IncludingAbsolutePath()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                TestDocument document = new TestDocument(new FilePath("/TestFiles/Input/test.txt"), "x ^\"/TestFiles/test-above-input.txt\" y");
                ProcessIncludes include = new ProcessIncludes();

                // When
                TestDocument result = await ExecuteAsync(document, context, include).SingleAsync();

                // Then
                result.Content.ShouldBe("x test y");
            }

            [Test]
            public async Task NestedInclude()
            {
                // Given
                TestExecutionContext context = GetExecutionContext(out TestFileProvider fileProvider);
                fileProvider.AddFile(
                    "/TestFiles/Input/test-outer.txt",
                    "3 ^\"test-a.txt\" 4");
                TestDocument document = new TestDocument(new FilePath("/TestFiles/Input/test.txt"), "1 ^\"test-outer.txt\" 2");
                ProcessIncludes include = new ProcessIncludes();

                // When
                TestDocument result = await ExecuteAsync(document, context, include).SingleAsync();

                // Then
                result.Content.ShouldBe("1 3 aaa 4 2");
            }

            [Test]
            public async Task NestedIncludeWithoutRecursion()
            {
                // Given
                TestExecutionContext context = GetExecutionContext(out TestFileProvider fileProvider);
                fileProvider.AddFile(
                    "/TestFiles/Input/test-outer.txt",
                    "3 ^\"test-a.txt\" 4");
                TestDocument document = new TestDocument(new FilePath("/TestFiles/Input/test.txt"), "1 ^\"test-outer.txt\" 2");
                ProcessIncludes include = new ProcessIncludes().WithRecursion(false);

                // When
                TestDocument result = await ExecuteAsync(document, context, include).SingleAsync();

                // Then
                result.Content.ShouldBe("1 3 ^\"test-a.txt\" 4 2");
            }

            [Test]
            public async Task NestedIncludeWithInnerEscape()
            {
                // Given
                TestExecutionContext context = GetExecutionContext(out TestFileProvider fileProvider);
                fileProvider.AddFile(
                    "/TestFiles/Input/test-outer.txt",
                    "3 \\^\"test-a.txt\" 4");
                TestDocument document = new TestDocument(new FilePath("/TestFiles/Input/test.txt"), "1 ^\"test-outer.txt\" 2");
                ProcessIncludes include = new ProcessIncludes().WithRecursion(false);

                // When
                TestDocument result = await ExecuteAsync(document, context, include).SingleAsync();

                // Then
                result.Content.ShouldBe("1 3 \\^\"test-a.txt\" 4 2");
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
                TestDocument document = new TestDocument(new FilePath("/TestFiles/Input/test.txt"), "1 ^\"test-outer.txt\" 2");
                ProcessIncludes include = new ProcessIncludes();

                // When
                TestDocument result = await ExecuteAsync(document, context, include).SingleAsync();

                // Then
                result.Content.ShouldBe("1 3 5 aaa 6 4 2");
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
