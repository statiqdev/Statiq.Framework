using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Execution;
using Wyam.Core.Execution;
using Wyam.Core.Modules.Extensibility;
using Wyam.Testing;
using Wyam.Testing.Execution;

namespace Wyam.Core.Tests.Modules.Control
{
    [TestFixture]
    [NonParallelizable]
    public class DocumentsFixture : BaseFixture
    {
        public class ExecuteTests : DocumentsFixture
        {
            [Test]
            public async Task CountReturnsCorrectDocuments()
            {
                // Given
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                Core.Modules.Control.Documents documents = new Core.Modules.Control.Documents(5);
                engine.Pipelines.Add(documents);

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                Assert.AreEqual(5, engine.Documents.Count());
            }

            [Test]
            public async Task ContentReturnsCorrectDocuments()
            {
                // Given
                List<string> content = new List<string>();
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                Core.Modules.Control.Documents documents = new Core.Modules.Control.Documents("A", "B", "C", "D");
                Execute gatherData = new Execute(
                    (d, c) =>
                {
                    content.Add(d.Content);
                    return null;
                }, false);
                engine.Pipelines.Add(documents, gatherData);

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                Assert.AreEqual(4, content.Count);
                CollectionAssert.AreEqual(new[] { "A", "B", "C", "D" }, content);
            }

            [Test]
            public async Task MetadataReturnsCorrectDocuments()
            {
                // Given
                List<object> values = new List<object>();
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                Core.Modules.Control.Documents documents = new Core.Modules.Control.Documents(
                    new Dictionary<string, object> { { "Foo", "a" } },
                    new Dictionary<string, object> { { "Foo", "b" } },
                    new Dictionary<string, object> { { "Foo", "c" } });
                Execute gatherData = new Execute(
                    (d, c) =>
                {
                    values.Add(d["Foo"]);
                    return null;
                }, false);
                engine.Pipelines.Add(documents, gatherData);

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                Assert.AreEqual(3, values.Count);
                CollectionAssert.AreEqual(new[] { "a", "b", "c" }, values);
            }

            [Test]
            public async Task ContentAndMetadataReturnsCorrectDocuments()
            {
                // Given
                List<string> content = new List<string>();
                List<object> values = new List<object>();
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                Core.Modules.Control.Documents documents = new Core.Modules.Control.Documents(
                    Tuple.Create("A", new Dictionary<string, object> { { "Foo", "a" } }.AsEnumerable()),
                    Tuple.Create("B", new Dictionary<string, object> { { "Foo", "b" } }.AsEnumerable()),
                    Tuple.Create("C", new Dictionary<string, object> { { "Foo", "c" } }.AsEnumerable()));
                Execute gatherData = new Execute(
                    (d, c) =>
                {
                    content.Add(d.Content);
                    values.Add(d["Foo"]);
                    return null;
                }, false);
                engine.Pipelines.Add(documents, gatherData);

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                Assert.AreEqual(3, content.Count);
                Assert.AreEqual(3, values.Count);
                CollectionAssert.AreEqual(new[] { "A", "B", "C" }, content);
                CollectionAssert.AreEqual(new[] { "a", "b", "c" }, values);
            }

            [Test]
            public async Task PipelineReturnsCorrectDocuments()
            {
                // Given
                List<string> content = new List<string>();
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                Execute gatherData = new Execute(
                    (d, c) =>
                    {
                        content.Add(d.Content);
                        return null;
                    }, false);
                engine.Pipelines.Add("Foo", new Core.Modules.Control.Documents("A", "B", "C", "D"));
                engine.Pipelines.Add("Bar", new Core.Modules.Control.Documents("E", "F"));
                engine.Pipelines.Add(new Core.Modules.Control.Documents("Foo"), gatherData);

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                Assert.AreEqual(4, content.Count);
                CollectionAssert.AreEqual(new[] { "A", "B", "C", "D" }, content);
            }

            [Test]
            public async Task EmptyConstructorReturnsAllDocuments()
            {
                // Given
                List<string> content = new List<string>();
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                Execute gatherData = new Execute(
                    (d, c) =>
                    {
                        content.Add(d.Content);
                        return null;
                    }, false);
                engine.Pipelines.Add("Foo", new Core.Modules.Control.Documents("A", "B", "C", "D"));
                engine.Pipelines.Add("Bar", new Core.Modules.Control.Documents("E", "F"));
                engine.Pipelines.Add(new Core.Modules.Control.Documents(), gatherData);

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                Assert.AreEqual(6, content.Count);
                CollectionAssert.AreEqual(new[] { "A", "B", "C", "D", "E", "F" }, content);
            }

            [Test]
            public async Task EmptyConstructorWithSpecifiedPipelinesReturnsCorrectDocuments()
            {
                // Given
                List<string> content = new List<string>();
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                Execute gatherData = new Execute(
                    (d, c) =>
                    {
                        content.Add(d.Content);
                        return null;
                    }, false);
                engine.Pipelines.Add("Foo", new Core.Modules.Control.Documents("A", "B", "C", "D"));
                engine.Pipelines.Add("Bar", new Core.Modules.Control.Documents("E", "F"));
                engine.Pipelines.Add("Baz", new Core.Modules.Control.Documents("G", "H"));
                engine.Pipelines.Add(new Core.Modules.Control.Documents().FromPipelines("Foo", "Baz"), gatherData);

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                Assert.AreEqual(6, content.Count);
                CollectionAssert.AreEqual(new[] { "A", "B", "C", "D", "G", "H" }, content);
            }

            [Test]
            public async Task SpecifiedPipelinesReturnsCorrectDocuments()
            {
                // Given
                List<string> content = new List<string>();
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                Execute gatherData = new Execute(
                    (d, c) =>
                    {
                        content.Add(d.Content);
                        return null;
                    }, false);
                engine.Pipelines.Add("Foo", new Core.Modules.Control.Documents("A", "B", "C", "D"));
                engine.Pipelines.Add("Bar", new Core.Modules.Control.Documents("E", "F"));
                engine.Pipelines.Add("Baz", new Core.Modules.Control.Documents("G", "H"));
                engine.Pipelines.Add(new Core.Modules.Control.Documents("Foo").FromPipelines("Baz"), gatherData);

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                Assert.AreEqual(6, content.Count);
                CollectionAssert.AreEqual(new[] { "A", "B", "C", "D", "G", "H" }, content);
            }

            [Test]
            public async Task SpecifiedPipelineDocumentsAreReturnedInCorrectOrder()
            {
                // Given
                List<string> content = new List<string>();
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                Execute gatherData = new Execute(
                    (d, c) =>
                    {
                        content.Add(d.Content);
                        return null;
                    }, false);
                engine.Pipelines.Add("Foo", new Core.Modules.Control.Documents("A", "B", "C", "D"));
                engine.Pipelines.Add("Bar", new Core.Modules.Control.Documents("E", "F"));
                engine.Pipelines.Add("Baz", new Core.Modules.Control.Documents("G", "H"));
                engine.Pipelines.Add(new Core.Modules.Control.Documents("Baz").FromPipelines("Foo"), gatherData);

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                Assert.AreEqual(6, content.Count);
                CollectionAssert.AreEqual(new[] { "G", "H", "A", "B", "C", "D" }, content);
            }
        }
    }
}
