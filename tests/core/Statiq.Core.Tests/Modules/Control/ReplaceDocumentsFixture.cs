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
    public class ReplaceDocumentsFixture : BaseFixture
    {
        public class ExecuteTests : ReplaceDocumentsFixture
        {
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
                engine.Pipelines.Add("Foo", new TestPipeline(new Core.Modules.Control.CreateDocuments("A", "B", "C", "D")));
                engine.Pipelines.Add("Bar", new TestPipeline(new Core.Modules.Control.CreateDocuments("E", "F")));
                engine.Pipelines.Add(new TestPipeline(new Core.Modules.Control.ReplaceDocuments("Foo"), gatherData).WithDependencies("Foo", "Bar"));

                // When
                await engine.ExecuteAsync(serviceProvider, cancellationTokenSource);

                // Then
                Assert.AreEqual(4, content.Count);
                CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "D" }, content);
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
                engine.Pipelines.Add("Foo", new TestPipeline(new Core.Modules.Control.CreateDocuments("A", "B", "C", "D")));
                engine.Pipelines.Add("Bar", new TestPipeline(new Core.Modules.Control.CreateDocuments("E", "F")));
                engine.Pipelines.Add("Baz", new TestPipeline(new Core.Modules.Control.CreateDocuments("G", "H")));
                engine.Pipelines.Add(
                    new TestPipeline(new Core.Modules.Control.ReplaceDocuments("Foo", "Baz"), gatherData)
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
                engine.Pipelines.Add("Foo", new TestPipeline(new Core.Modules.Control.CreateDocuments("A", "B", "C", "D")));
                engine.Pipelines.Add("Bar", new TestPipeline(new Core.Modules.Control.CreateDocuments("E", "F")));
                engine.Pipelines.Add("Baz", new TestPipeline(new Core.Modules.Control.CreateDocuments("G", "H")));
                engine.Pipelines.Add(
                    new TestPipeline(new Core.Modules.Control.ReplaceDocuments("Baz", "Foo"), gatherData)
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
