using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Util;
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
                IExecutionContext context = GetExecutionContext(engine);
                IDocument[] inputs =
                {
                    GetDocument("a/1.md", "File a1")
                };
                string lodedSidecarContent = null;
                Sidecar sidecar = new Sidecar(new ExecuteDocument(Config.FromDocument(x =>
                {
                    lodedSidecarContent = x.Content;
                    return (object)new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await sidecar.ExecuteAsync(inputs, context).ToArrayAsync();

                // Then
                Assert.AreEqual("data: a1", lodedSidecarContent);
                Assert.AreEqual("File a1", documents.Single().Content);
            }

            [Test]
            public async Task LoadsCustomSidecarFile()
            {
                // Given
                Engine engine = new Engine();
                IExecutionContext context = GetExecutionContext(engine);
                IDocument[] inputs =
                {
                    GetDocument("a/1.md", "File a1")
                };
                string lodedSidecarContent = null;
                Sidecar sidecar = new Sidecar(".other", new ExecuteDocument(Config.FromDocument(x =>
                {
                    lodedSidecarContent = x.Content;
                    return (object)new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await sidecar.ExecuteAsync(inputs, context).ToArrayAsync();

                // Then
                Assert.AreEqual("data: other", lodedSidecarContent);
                Assert.AreEqual("File a1", documents.Single().Content);
            }

            [Test]
            public async Task ReturnsOriginalDocumentForMissingSidecarFile()
            {
                // Given
                Engine engine = new Engine();
                IExecutionContext context = GetExecutionContext(engine);
                IDocument[] inputs =
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
                IEnumerable<IDocument> documents = await sidecar.ExecuteAsync(inputs, context).ToArrayAsync();

                // Then
                Assert.IsFalse(executedSidecarModules);
                Assert.AreEqual(inputs.First(), documents.First());
            }

            private IDocument GetDocument(string source, string content)
            {
                IDocument document = new TestDocument(
                    content,
                    new Dictionary<string, object>
                    {
                        { Keys.RelativeFilePath, source },
                        { Keys.SourceFilePath, new FilePath("/" + source) },
                        { Keys.SourceFileName, new FilePath(source).FileName }
                    })
                    {
                        Source = new FilePath("/" + source)
                    };
                return document;
            }

            private IExecutionContext GetExecutionContext(Engine engine)
            {
                TestExecutionContext context = new TestExecutionContext
                {
                    Namespaces = engine.Namespaces,
                    FileSystem = GetFileSystem()
                };
                return context;
            }

            private IReadOnlyFileSystem GetFileSystem()
            {
                TestFileProvider fileProvider = GetFileProvider();
                TestFileSystem fileSystem = new TestFileSystem
                {
                    InputPaths = new PathCollection<DirectoryPath>(new[]
                    {
                        new DirectoryPath("/")
                    }),
                    FileProvider = fileProvider
                };
                return fileSystem;
            }

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
