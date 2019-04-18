using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Util;
using Wyam.Core.Documents;
using Wyam.Core.Execution;
using Wyam.Core.IO;
using Wyam.Core.Modules.IO;
using Wyam.Testing;
using Wyam.Testing.Execution;
using Wyam.Testing.IO;

namespace Wyam.Core.Tests.Modules.IO
{
    [TestFixture]
    [NonParallelizable]
    public class ReadFilesFixture : BaseFixture
    {
        private Engine Engine { get; set; }
        private ExecutionPipeline Pipeline { get; set; }
        private IExecutionContext Context { get; set; }
        private IDocument[] Inputs { get; set; }

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
            Inputs = new[] { Context.GetDocument() };
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
            fileProvider.AddFile("/TestFiles/Input/.dotfile", "dotfile");

            return fileProvider;
        }

        public class ConstructorTests : ReadFilesFixture
        {
            [Test]
            public void ThrowsOnNullPathFunction()
            {
                // Given, When, Then
                Assert.Throws<ArgumentNullException>(() => new ReadFiles((DocumentConfig<IEnumerable<string>>)null));
            }

            [Test]
            public void ThrowsOnNullPatterns()
            {
                // Given, When, Then
                Assert.Throws<ArgumentNullException>(() => new ReadFiles((string[])null));
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
                ReadFiles readFiles = new ReadFiles(pattern);

                // When
                IEnumerable<IDocument> documents = await readFiles.ExecuteAsync(Inputs, Context).ToListAsync();

                // Then
                Assert.AreEqual(expectedCount, documents.Count());
            }

            [Test]
            public async Task PatternWorksWithSubpath()
            {
                // Given
                ReadFiles readFiles = new ReadFiles("Subfolder/*.txt");

                // When
                IEnumerable<IDocument> documents = await readFiles.ExecuteAsync(Inputs, Context).ToListAsync();

                // Then
                Assert.AreEqual(1, documents.Count());
            }

            [Test]
            public async Task PatternWorksWithSingleFile()
            {
                // Given
                ReadFiles readFiles = new ReadFiles("test-a.txt");

                // When
                IEnumerable<IDocument> documents = await readFiles.ExecuteAsync(Inputs, Context).ToListAsync();

                // Then
                Assert.AreEqual(1, documents.Count());
            }

            [Test]
            public async Task ShouldReturnNullBasePathsForDotFiles()
            {
                // Given
                ReadFiles readFiles = new ReadFiles(".dotfile");

                // When
                IDocument document = (await readFiles.ExecuteAsync(Inputs, Context)).First();

                // Then
                Assert.IsNull(document[Keys.SourceFileBase]);
                Assert.IsNull(document[Keys.SourceFilePathBase]);
                Assert.IsNull(document[Keys.RelativeFilePathBase]);
            }

            [Test]
            public async Task GetsCorrectContent()
            {
                // Given
                ReadFiles readFiles = new ReadFiles("test-a.txt");

                // When
                IDocument document = (await readFiles.ExecuteAsync(Inputs, Context)).First();

                // Then
                Assert.AreEqual("aaa", document.Content);
            }

            [TestCase(Keys.SourceFileBase, "test-c")]
            [TestCase(Keys.SourceFileName, "test-c.txt")]
            [TestCase(Keys.SourceFilePath, "/TestFiles/Input/Subfolder/test-c.txt")]
            [TestCase(Keys.SourceFilePathBase, "/TestFiles/Input/Subfolder/test-c")]
            [TestCase(Keys.RelativeFilePath, "Subfolder/test-c.txt")]
            [TestCase(Keys.RelativeFilePathBase, "Subfolder/test-c")]
            public async Task ShouldSetFilePathMetadata(string key, string expected)
            {
                // Given
                ReadFiles readFiles = new ReadFiles("**/test-c.txt");

                // When
                IDocument output = (await readFiles.ExecuteAsync(Inputs, Context)).First();

                // Then
                object result = output[key];
                Assert.IsInstanceOf<FilePath>(result);
                Assert.AreEqual(expected, ((FilePath)result).FullPath);
            }

            [TestCase(Keys.SourceFileRoot, "/TestFiles/Input")]
            [TestCase(Keys.SourceFileDir, "/TestFiles/Input/Subfolder")]
            [TestCase(Keys.RelativeFileDir, "Subfolder")]
            public async Task ShouldSetDirectoryPathMetadata(string key, string expected)
            {
                // Given
                ReadFiles readFiles = new ReadFiles("**/test-c.txt");

                // When
                IDocument output = (await readFiles.ExecuteAsync(Inputs, Context)).First();

                // Then
                object result = output[key];
                Assert.IsInstanceOf<DirectoryPath>(result);
                Assert.AreEqual(expected, ((DirectoryPath)result).FullPath);
            }

            [TestCase(Keys.SourceFileExt, ".txt")]
            public async Task ShouldSetStringMetadata(string key, string expected)
            {
                // Given
                ReadFiles readFiles = new ReadFiles("**/test-c.txt");

                // When
                IDocument output = (await readFiles.ExecuteAsync(Inputs, Context)).First();

                // Then
                object result = output[key];
                Assert.IsInstanceOf<string>(result);
                Assert.AreEqual(expected, result);
            }

            [Test]
            public async Task WorksWithMultipleExtensions()
            {
                // Given
                ReadFiles readFiles = new ReadFiles("**/*.{txt,md}");

                // When
                IEnumerable<IDocument> documents = await readFiles.ExecuteAsync(Inputs, Context).ToListAsync();

                // Then
                Assert.AreEqual(5, documents.Count());
            }

            [Test]
            public async Task PredicateShouldReturnMatchingFiles()
            {
                // Given
                ReadFiles readFiles = new ReadFiles("**/*").Where(x => Task.FromResult(x.Path.FullPath.Contains("test")));

                // When
                IEnumerable<IDocument> documents = await readFiles.ExecuteAsync(Inputs, Context).ToListAsync();

                // Then
                Assert.AreEqual(3, documents.Count());
            }
        }
    }
}
