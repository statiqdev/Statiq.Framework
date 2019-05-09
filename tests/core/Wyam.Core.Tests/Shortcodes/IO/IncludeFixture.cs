﻿using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.IO;
using Wyam.Core.Shortcodes.IO;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;
using Wyam.Testing.IO;

namespace Wyam.Core.Tests.Shortcodes.IO
{
    [TestFixture]
    public class IncludeFixture : BaseFixture
    {
        public class ExecuteTests : IncludeFixture
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
                Include shortcode = new Include();

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
                Include shortcode = new Include();

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
                TestDocument document = new TestDocument()
                {
                    Source = new FilePath("/A/D/x.txt")
                };
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, "../B/c.txt")
                };
                Include shortcode = new Include();

                // When
                TestDocument result = (TestDocument)await shortcode.ExecuteAsync(args, string.Empty, document, context);

                // Then
                result.Content.ShouldBe("foo");
            }
        }
    }
}
