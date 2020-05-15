using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Control
{
    [TestFixture]
    public class ProcessSidecarFileFixture : BaseFixture
    {
        public class ExecuteTests : ProcessSidecarFileFixture
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
                ProcessSidecarFile sidecar = new ProcessSidecarFile(new ExecuteConfig(Config.FromDocument(async x =>
                {
                    lodedSidecarContent = await x.GetContentStringAsync();
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
                ProcessSidecarFile sidecar = new ProcessSidecarFile(".other", new ExecuteConfig(Config.FromDocument(async x =>
                {
                    lodedSidecarContent = await x.GetContentStringAsync();
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
                ProcessSidecarFile sidecar = new ProcessSidecarFile(".foo", new ExecuteConfig(Config.FromDocument(x =>
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
                    new NormalizedPath("/" + source),
                    new NormalizedPath(source),
                    content);

            private TestExecutionContext GetExecutionContext(Engine engine) =>
                new TestExecutionContext
                {
                    Namespaces = new TestNamespacesCollection(engine.Namespaces.ToArray()),
                    FileSystem = GetFileSystem()
                };

            private TestFileSystem GetFileSystem() =>
                new TestFileSystem
                {
                    InputPaths = new PathCollection(NormalizedPath.AbsoluteRoot),
                    FileProvider = GetFileProvider()
                };

            private TestFileProvider GetFileProvider() =>
                new TestFileProvider
                {
                    { "/a/1.md", "File a1" },
                    { "/a/1.md.meta", "data: a1" },
                    { "/a/1.md.other", "data: other" },
                    { "/a/2.md", "File a2" },
                    { "/a/2.md.meta", "data: a2" },
                    { "/b/1.md", "File b1" },
                    { "/b/1.md.meta", "data: b1" }
                };
        }
    }
}
