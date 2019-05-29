using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Core.Execution;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;
using Wyam.Testing.IO;

namespace Wyam.Core.Tests.Modules.Control
{
    [TestFixture]
    [NonParallelizable]
    public class SidecarFixture : BaseFixture
    {
        public class ExecuteTests : SidecarFixture
        {
            [Test]
            public async Task LoadsSidecarFile()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument[] inputs =
                {
                    GetDocument("a/1.md", "File a1")
                };
                string lodedSidecarContent = null;
                Sidecar sidecar = new Sidecar(new ExecuteDocument(Config.FromDocument(async x =>
                {
                    lodedSidecarContent = await x.GetStringAsync();
                    return new[] { x };
                })));

                // When
                IReadOnlyList<TestDocument> documents = await ExecuteAsync(inputs, context, sidecar);

                // Then
                Assert.AreEqual("data: a1", lodedSidecarContent);
                Assert.AreEqual("File a1", documents.Single().Content);
            }

            [Test]
            public async Task LoadsCustomSidecarFile()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument[] inputs =
                {
                    GetDocument("a/1.md", "File a1")
                };
                string lodedSidecarContent = null;
                Sidecar sidecar = new Sidecar(".other", new ExecuteDocument(Config.FromDocument(async x =>
                {
                    lodedSidecarContent = await x.GetStringAsync();
                    return new[] { x };
                })));

                // When
                IReadOnlyList<TestDocument> documents = await ExecuteAsync(inputs, context, sidecar);

                // Then
                Assert.AreEqual("data: other", lodedSidecarContent);
                Assert.AreEqual("File a1", documents.Single().Content);
            }

            [Test]
            public async Task ReturnsOriginalDocumentForMissingSidecarFile()
            {
                // Given
                Engine engine = new Engine();
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument[] inputs =
                {
                    GetDocument("a/1.md", "File a1")
                };
                bool executedSidecarModules = false;
                Sidecar sidecar = new Sidecar(".foo", new ExecuteDocument(Config.FromDocument(x =>
                {
                    executedSidecarModules = true;
                    return (object)new[] { x };
                })));

                // When
                IReadOnlyList<TestDocument> documents = await ExecuteAsync(inputs, context, sidecar);

                // Then
                Assert.IsFalse(executedSidecarModules);
                Assert.AreEqual(inputs.First(), documents.First());
            }

            private TestDocument GetDocument(string source, string content) =>
                new TestDocument(
                    new FilePath("/" + source),
                    new FilePath(source),
                    content);

            private TestExecutionContext GetExecutionContext(Engine engine) =>
                new TestExecutionContext
                {
                    Namespaces = engine.Namespaces,
                    FileSystem = GetFileSystem()
                };

            private TestFileSystem GetFileSystem() =>
                new TestFileSystem
                {
                    InputPaths = new PathCollection<DirectoryPath>(new[]
                    {
                        new DirectoryPath("/")
                    }),
                    FileProvider = GetFileProvider()
                };

            private TestFileProvider GetFileProvider()
            {
                TestFileProvider fileProvider = new TestFileProvider();

                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/a");
                fileProvider.AddDirectory("/b");

                fileProvider.AddFile("/a/1.md", "File a1");
                fileProvider.AddFile("/a/1.md.meta", "data: a1");
                fileProvider.AddFile("/a/1.md.other", "data: other");
                fileProvider.AddFile("/a/2.md", "File a2");
                fileProvider.AddFile("/a/2.md.meta", "data: a2");

                fileProvider.AddFile("/b/1.md", "File b1");
                fileProvider.AddFile("/b/1.md.meta", "data: b1");

                return fileProvider;
            }
        }
    }
}
