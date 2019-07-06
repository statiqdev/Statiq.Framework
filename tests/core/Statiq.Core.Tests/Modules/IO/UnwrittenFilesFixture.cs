using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Common.IO;
using Statiq.Core.Modules.IO;
using Statiq.Testing;
using Statiq.Testing.Documents;
using Statiq.Testing.Execution;
using Statiq.Testing.IO;

namespace Statiq.Core.Tests.Modules.IO
{
    [TestFixture]
    public class UnwrittenFilesFixture : BaseFixture
    {
        [Test]
        public async Task DoesNotOutputExistingFiles()
        {
            // Given
            TestExecutionContext context = GetContext();
            TestDocument input = new TestDocument(new FilePath("test.md"));
            UnwrittenFiles unwrittenFiles = new UnwrittenFiles();

            // When
            IReadOnlyList<TestDocument> results = await ExecuteAsync(input, context, unwrittenFiles);

            // Then
            results.ShouldBeEmpty();
        }

        [Test]
        public async Task DoesNotOutputExistingFilesWithDifferentExtension()
        {
            // Given
            TestExecutionContext context = GetContext();
            TestDocument input = new TestDocument(
                new FilePath("/input/test.txt"),
                new FilePath("test.md"));
            UnwrittenFiles unwrittenFiles = new UnwrittenFiles();

            // When
            IReadOnlyList<TestDocument> results = await ExecuteAsync(input, context, unwrittenFiles);

            // Then
            results.ShouldBeEmpty();
        }

        [Test]
        public async Task ShouldOutputNonExistingFiles()
        {
            // Given
            TestExecutionContext context = GetContext();
            TestDocument input = new TestDocument(
                new FilePath("foo.txt"),
                "Test");
            UnwrittenFiles unwrittenFiles = new UnwrittenFiles();

            // When
            TestDocument result = await ExecuteAsync(input, context, unwrittenFiles).SingleAsync();

            // Then
            result.Content.ShouldBe("Test");
        }

        [Test]
        public async Task ShouldOutputNonExistingFilesWithDifferentExtension()
        {
            // Given
            TestExecutionContext context = GetContext();
            TestDocument input = new TestDocument(
                new FilePath("/input/test.md"),
                new FilePath("test.txt"),
                "Test");
            UnwrittenFiles unwrittenFiles = new UnwrittenFiles();

            // When
            TestDocument result = await ExecuteAsync(input, context, unwrittenFiles).SingleAsync();

            // Then
            result.Content.ShouldBe("Test");
        }

        private TestExecutionContext GetContext() => new TestExecutionContext
        {
            FileSystem = new TestFileSystem
            {
                FileProvider = GetFileProvider(),
                RootPath = "/"
            }
        };

        private TestFileProvider GetFileProvider()
        {
            TestFileProvider fileProvider = new TestFileProvider();

            fileProvider.AddDirectory("/");
            fileProvider.AddDirectory("/output");

            fileProvider.AddFile("/output/test.md");

            return fileProvider;
        }
    }
}
