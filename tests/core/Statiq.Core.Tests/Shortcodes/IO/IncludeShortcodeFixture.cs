using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;
using Statiq.Testing.Documents;
using Statiq.Testing.Execution;
using Statiq.Testing.IO;

namespace Statiq.Core.Tests.Shortcodes.IO
{
    [TestFixture]
    public class IncludeShortcodeFixture : BaseFixture
    {
        public class ExecuteTests : IncludeShortcodeFixture
        {
            [Test]
            public async Task IncludesFile()
            {
                // Given
                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/A");
                fileProvider.AddDirectory("/A/B");
                fileProvider.AddFile("/A/B/c.txt", "foo");
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                fileSystem.InputPaths.Clear();
                fileSystem.InputPaths.Add("/A");

                TestExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                TestDocument document = new TestDocument();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, "B/c.txt")
                };
                IncludeShortcode shortcode = new IncludeShortcode();

                // When
                TestDocument result = (TestDocument)await shortcode.ExecuteAsync(args, string.Empty, document, context);

                // Then
                result.Content.ShouldBe("foo");
            }

            [Test]
            public async Task EmptyResultIfFileDoesNotExist()
            {
                // Given
                ThrowOnTraceEventType(System.Diagnostics.TraceEventType.Error);
                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/A");
                fileProvider.AddDirectory("/A/B");
                fileProvider.AddFile("/A/B/c.txt", "foo");
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                fileSystem.InputPaths.Clear();
                fileSystem.InputPaths.Add("/A");

                TestExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                TestDocument document = new TestDocument();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, "B/d.txt")
                };
                IncludeShortcode shortcode = new IncludeShortcode();

                // When
                TestDocument result = (TestDocument)await shortcode.ExecuteAsync(args, string.Empty, document, context);

                // Then
                result.Content.ShouldBeEmpty();
            }

            [Test]
            public async Task IncludesFileRelativeToSource()
            {
                // Given
                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/A");
                fileProvider.AddDirectory("/A/B");
                fileProvider.AddDirectory("/A/D");
                fileProvider.AddFile("/A/B/c.txt", "foo");
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                fileSystem.InputPaths.Clear();
                fileSystem.InputPaths.Add("/A");

                TestExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                TestDocument document = new TestDocument(new FilePath("/A/D/x.txt"), (FilePath)null);
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, "../B/c.txt")
                };
                IncludeShortcode shortcode = new IncludeShortcode();

                // When
                TestDocument result = (TestDocument)await shortcode.ExecuteAsync(args, string.Empty, document, context);

                // Then
                result.Content.ShouldBe("foo");
            }
        }
    }
}
