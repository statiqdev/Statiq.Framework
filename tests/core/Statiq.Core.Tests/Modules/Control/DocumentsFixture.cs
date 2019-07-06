using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Statiq.Common.Configuration;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Core.Execution;
using Statiq.Core.Modules.Extensibility;
using Statiq.Testing;
using Statiq.Testing.Execution;

namespace Statiq.Core.Tests.Modules.Control
{
    [TestFixture]
    public class DocumentsFixture : BaseFixture
    {
        public class ExecuteTests : DocumentsFixture
        {
            [Test]
            public async Task CountReturnsCorrectDocuments()
            {
                // Given
                Core.Modules.Control.Documents documents = new Core.Modules.Control.Documents(5);

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(documents);

                // Then
                Assert.AreEqual(5, results.Count);
            }

            [Test]
            public async Task ContentReturnsCorrectDocuments()
            {
                // Given
                List<string> content = new List<string>();
                Core.Modules.Control.Documents documents = new Core.Modules.Control.Documents("A", "B", "C", "D");
                Execute gatherData = new ExecuteDocument(
                    Config.FromDocument(async d =>
                {
                    content.Add(await d.GetStringAsync());
                    return (object)null;
                }), false);

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(documents, gatherData);

                // Then
                Assert.AreEqual(4, content.Count);
                CollectionAssert.AreEqual(new[] { "A", "B", "C", "D" }, content);
            }

            [Test]
            public async Task MetadataReturnsCorrectDocuments()
            {
                // Given
                List<object> values = new List<object>();
                Core.Modules.Control.Documents documents = new Core.Modules.Control.Documents(
                    new Dictionary<string, object> { { "Foo", "a" } },
                    new Dictionary<string, object> { { "Foo", "b" } },
                    new Dictionary<string, object> { { "Foo", "c" } });
                Execute gatherData = new ExecuteDocument(
                    Config.FromDocument(d =>
                {
                    values.Add(d["Foo"]);
                    return (object)null;
                }), false);

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(documents, gatherData);

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
                Core.Modules.Control.Documents documents = new Core.Modules.Control.Documents(
                    Tuple.Create("A", new Dictionary<string, object> { { "Foo", "a" } }.AsEnumerable()),
                    Tuple.Create("B", new Dictionary<string, object> { { "Foo", "b" } }.AsEnumerable()),
                    Tuple.Create("C", new Dictionary<string, object> { { "Foo", "c" } }.AsEnumerable()));
                Execute gatherData = new ExecuteDocument(
                    Config.FromDocument(async d =>
                {
                    content.Add(await d.GetStringAsync());
                    values.Add(d["Foo"]);
                    return (object)null;
                }), false);

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(documents, gatherData);

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
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                Execute gatherData = new ExecuteDocument(
                    Config.FromDocument(async d =>
                    {
                        content.Add(await d.GetStringAsync());
                        return (object)null;
                    }), false);
                engine.Pipelines.Add("Foo", new TestPipeline(new Core.Modules.Control.Documents("A", "B", "C", "D")));
                engine.Pipelines.Add("Bar", new TestPipeline(new Core.Modules.Control.Documents("E", "F")));
                engine.Pipelines.Add(new TestPipeline(new Core.Modules.Control.Documents("Foo"), gatherData).WithDependencies("Foo", "Bar"));

                // When
                await engine.ExecuteAsync(serviceProvider, cancellationTokenSource);

                // Then
                Assert.AreEqual(4, content.Count);
                CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "D" }, content);
            }

            [Test]
            public async Task EmptyConstructorReturnsAllDocuments()
            {
                // Given
                List<string> content = new List<string>();
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                Execute gatherData = new ExecuteDocument(
                    Config.FromDocument(async d =>
                    {
                        content.Add(await d.GetStringAsync());
                        return (object)null;
                    }), false);
                engine.Pipelines.Add("Foo", new TestPipeline(new Core.Modules.Control.Documents("A", "B", "C", "D")));
                engine.Pipelines.Add("Bar", new TestPipeline(new Core.Modules.Control.Documents("E", "F")));
                engine.Pipelines.Add(new TestPipeline(new Core.Modules.Control.Documents(), gatherData).WithDependencies("Foo", "Bar"));

                // When
                await engine.ExecuteAsync(serviceProvider, cancellationTokenSource);

                // Then
                Assert.AreEqual(6, content.Count);
                CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "D", "E", "F" }, content);
            }

            [Test]
            public async Task EmptyConstructorWithSpecifiedPipelinesReturnsCorrectDocuments()
            {
                // Given
                List<string> content = new List<string>();
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                Execute gatherData = new ExecuteDocument(
                    Config.FromDocument(async d =>
                    {
                        content.Add(await d.GetStringAsync());
                        return (object)null;
                    }), false);
                engine.Pipelines.Add("Foo", new TestPipeline(new Core.Modules.Control.Documents("A", "B", "C", "D")));
                engine.Pipelines.Add("Bar", new TestPipeline(new Core.Modules.Control.Documents("E", "F")));
                engine.Pipelines.Add("Baz", new TestPipeline(new Core.Modules.Control.Documents("G", "H")));
                engine.Pipelines.Add(
                    new TestPipeline(new Core.Modules.Control.Documents().FromPipelines("Foo", "Baz"), gatherData)
                        .WithDependencies("Foo", "Bar", "Baz"));

                // When
                await engine.ExecuteAsync(serviceProvider, cancellationTokenSource);

                // Then
                Assert.AreEqual(6, content.Count);
                CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "D", "G", "H" }, content);
            }

            [Test]
            public async Task SpecifiedPipelinesReturnsCorrectDocuments()
            {
                // Given
                List<string> content = new List<string>();
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                Execute gatherData = new ExecuteDocument(
                    Config.FromDocument(async d =>
                    {
                        content.Add(await d.GetStringAsync());
                        return (object)null;
                    }), false);
                engine.Pipelines.Add("Foo", new TestPipeline(new Core.Modules.Control.Documents("A", "B", "C", "D")));
                engine.Pipelines.Add("Bar", new TestPipeline(new Core.Modules.Control.Documents("E", "F")));
                engine.Pipelines.Add("Baz", new TestPipeline(new Core.Modules.Control.Documents("G", "H")));
                engine.Pipelines.Add(
                    new TestPipeline(new Core.Modules.Control.Documents("Foo").FromPipelines("Baz"), gatherData)
                        .WithDependencies("Foo", "Bar", "Baz"));

                // When
                await engine.ExecuteAsync(serviceProvider, cancellationTokenSource);

                // Then
                Assert.AreEqual(6, content.Count);
                CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "D", "G", "H" }, content);
            }

            [Test]
            public async Task SpecifiedPipelineDocumentsAreReturnedInCorrectOrder()
            {
                // Given
                List<string> content = new List<string>();
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                Execute gatherData = new ExecuteDocument(
                    Config.FromDocument(async d =>
                    {
                        content.Add(await d.GetStringAsync());
                        return (object)null;
                    }), false);
                engine.Pipelines.Add("Foo", new TestPipeline(new Core.Modules.Control.Documents("A", "B", "C", "D")));
                engine.Pipelines.Add("Bar", new TestPipeline(new Core.Modules.Control.Documents("E", "F")));
                engine.Pipelines.Add("Baz", new TestPipeline(new Core.Modules.Control.Documents("G", "H")));
                engine.Pipelines.Add(
                    new TestPipeline(new Core.Modules.Control.Documents("Baz").FromPipelines("Foo"), gatherData)
                        .WithDependencies("Foo", "Bar", "Baz"));

                // When
                await engine.ExecuteAsync(serviceProvider, cancellationTokenSource);

                // Then
                Assert.AreEqual(6, content.Count);
                CollectionAssert.AreEquivalent(new[] { "G", "H", "A", "B", "C", "D" }, content);
            }
        }
    }
}
