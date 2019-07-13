using System;
using System.Collections.Generic;
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
    public class ReadFilesFixture : BaseFixture
    {
        public class ConstructorTests : ReadFilesFixture
        {
            [Test]
            public void ThrowsOnNullPathFunction()
            {
                // Given, When, Then
                Should.Throw<ArgumentNullException>(() => new ReadFiles((DocumentConfig<IEnumerable<string>>)null));
            }

            [Test]
            public void ThrowsOnNullPatterns()
            {
                // Given, When, Then
                Should.Throw<ArgumentNullException>(() => new ReadFiles(null));
            }
        }

        public class ExecuteTests : ReadFilesFixture
        {
            [TestCase("*.foo", 0)]
            [TestCase("**/*.foo", 0)]
            [TestCase("*.txt", 2)]
            [TestCase("**/*.txt", 3)]
            [TestCase("*.md", 1)]
            [TestCase("**/*.md", 2)]
            [TestCase("*.*", 4)]
            [TestCase("**/*.*", 6)]
            public async Task PatternFindsCorrectFiles(string pattern, int expectedCount)
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                ReadFiles readFiles = new ReadFiles(pattern);

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(context, readFiles);

                // Then
                results.Count.ShouldBe(expectedCount);
            }

            [Test]
            public async Task PatternWorksWithSubpath()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                ReadFiles readFiles = new ReadFiles("Subfolder/*.txt");

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(context, readFiles);

                // Then
                results.Count.ShouldBe(1);
            }

            [Test]
            public async Task PatternWorksWithSingleFile()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                ReadFiles readFiles = new ReadFiles("test-a.txt");

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(context, readFiles);

                // Then
                results.Count.ShouldBe(1);
            }

            [Test]
            public async Task GetsCorrectContent()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                ReadFiles readFiles = new ReadFiles("test-a.txt");

                // When
                TestDocument result = await ExecuteAsync(context, readFiles).SingleAsync();

                // Then
                result.Content.ShouldBe("aaa");
            }

            public async Task ShouldSetSource(string key, string expected)
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                ReadFiles readFiles = new ReadFiles("**/test-c.txt");

                // When
                TestDocument result = await ExecuteAsync(context, readFiles).SingleAsync();

                // Then
                result.Source.FullPath.ShouldBe("/TestFiles/Input/Subfolder/test-c.txt");
            }

            public async Task ShouldSetDestination(string key, string expected)
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                ReadFiles readFiles = new ReadFiles("**/test-c.txt");

                // When
                TestDocument result = await ExecuteAsync(context, readFiles).SingleAsync();

                // Then
                result.Destination.FullPath.ShouldBe("Subfolder/test-c.txt");
            }

            [Test]
            public async Task WorksWithMultipleExtensions()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                ReadFiles readFiles = new ReadFiles("**/*.{txt,md}");

                // When
                IReadOnlyList<TestDocument> documents = await ExecuteAsync(context, readFiles);

                // Then
                documents.Count.ShouldBe(5);
            }

            [Test]
            public async Task PredicateShouldReturnMatchingFiles()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                ReadFiles readFiles = new ReadFiles("**/*").Where(x => Task.FromResult(x.Path.FullPath.Contains("test")));

                // When
                IReadOnlyList<TestDocument> documents = await ExecuteAsync(context, readFiles);

                // Then
                documents.Count.ShouldBe(3);
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
            fileProvider.AddFile("/TestFiles/Input/.dotfile", "dotfile");

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
