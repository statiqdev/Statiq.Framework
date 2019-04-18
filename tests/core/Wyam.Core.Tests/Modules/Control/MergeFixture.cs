using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Core.Execution;
using Wyam.Core.Modules.Control;
using Wyam.Testing;
using Wyam.Testing.Modules;
using Wyam.Common.Execution;
using Wyam.Testing.Execution;
using Wyam.Common.Configuration;

namespace Wyam.Core.Tests.Modules.Control
{
    [TestFixture]
    [NonParallelizable]
    public class MergeFixture : BaseFixture
    {
        public class ExecuteTests : MergeFixture
        {
            [Test]
            public async Task ReplacesContent()
            {
                // Given
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                CountModule a = new CountModule("A")
                {
                    Value = 10
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20
                };
                engine.Pipelines.Add(
                    "Test",
                    a,
                    new Merge(b),
                    new Core.Modules.Metadata.Meta("Content", Config.FromDocument(doc => doc.Content)));

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                CollectionAssert.AreEqual(new[] { "21" }, engine.Documents["Test"].Select(x => x["Content"]));
            }

            [Test]
            public async Task CombinesMetadata()
            {
                // Given
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                CountModule a = new CountModule("A")
                {
                    Value = 10
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20
                };
                engine.Pipelines.Add("Test", a, new Merge(b));

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                CollectionAssert.AreEqual(new[] { 11 }, engine.Documents["Test"].Select(x => x["A"]));
                CollectionAssert.AreEqual(new[] { 21 }, engine.Documents["Test"].Select(x => x["B"]));
            }

            [Test]
            public async Task CombinesAndOverwritesMetadata()
            {
                // Given
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                CountModule a = new CountModule("A")
                {
                    Value = 10
                };
                CountModule b = new CountModule("A")
                {
                    Value = 20
                };
                engine.Pipelines.Add("Test", a, new Merge(b));

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                CollectionAssert.AreEqual(new[] { 21 }, engine.Documents["Test"].Select(x => x["A"]));
            }

            [Test]
            public async Task SingleInputSingleResult()
            {
                // Given
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                CountModule a = new CountModule("A")
                {
                    Value = 10
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20
                };
                engine.Pipelines.Add("Test", a, new Merge(b));

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                Assert.AreEqual(1, a.OutputCount);
                Assert.AreEqual(1, b.OutputCount);
                CollectionAssert.AreEqual(new[] { 11 }, engine.Documents["Test"].Select(x => x["A"]));
                CollectionAssert.AreEqual(new[] { 21 }, engine.Documents["Test"].Select(x => x["B"]));
            }

            [Test]
            public async Task SingleInputMultipleResults()
            {
                // Given
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                CountModule a = new CountModule("A")
                {
                    Value = 10
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20,
                    AdditionalOutputs = 1
                };
                engine.Pipelines.Add("Test", a, new Merge(b));

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                Assert.AreEqual(1, a.OutputCount);
                Assert.AreEqual(2, b.OutputCount);
                CollectionAssert.AreEqual(new[] { 11, 11 }, engine.Documents["Test"].Select(x => x["A"]));
                CollectionAssert.AreEqual(new[] { 21, 22 }, engine.Documents["Test"].Select(x => x["B"]));
            }

            [Test]
            public async Task MultipleInputsSingleResult()
            {
                // Given
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                CountModule a = new CountModule("A")
                {
                    Value = 10,
                    AdditionalOutputs = 1
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20
                };
                engine.Pipelines.Add("Test", a, new Merge(b));

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                Assert.AreEqual(2, a.OutputCount);
                Assert.AreEqual(1, b.OutputCount);
                CollectionAssert.AreEqual(new[] { 11, 12 }, engine.Documents["Test"].Select(x => x["A"]));
                CollectionAssert.AreEqual(new[] { 21, 21 }, engine.Documents["Test"].Select(x => x["B"]));
            }

            [Test]
            public async Task MultipleInputsMultipleResults()
            {
                // Given
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                CountModule a = new CountModule("A")
                {
                    Value = 10,
                    AdditionalOutputs = 1
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20,
                    AdditionalOutputs = 1
                };
                engine.Pipelines.Add("Test", a, new Merge(b));

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                Assert.AreEqual(2, a.OutputCount);
                Assert.AreEqual(2, b.OutputCount);
                CollectionAssert.AreEqual(new[] { 11, 11, 12, 12 }, engine.Documents["Test"].Select(x => x["A"]));
                CollectionAssert.AreEqual(new[] { 21, 22, 21, 22 }, engine.Documents["Test"].Select(x => x["B"]));
            }

            [Test]
            public async Task SingleInputSingleResultForEachDocument()
            {
                // Given
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                CountModule a = new CountModule("A")
                {
                    Value = 10
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20
                };
                engine.Pipelines.Add(
                    "Test",
                    a,
                    new Merge(b).ForEachDocument(),
                    new Core.Modules.Metadata.Meta("Content", Config.FromDocument(doc => doc.Content)));

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                Assert.AreEqual(1, a.OutputCount);
                Assert.AreEqual(1, b.OutputCount);
                CollectionAssert.AreEqual(new[] { "1121" }, engine.Documents["Test"].Select(x => x["Content"]));
            }

            [Test]
            public async Task SingleInputMultipleResultsForEachDocument()
            {
                // Given
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                CountModule a = new CountModule("A")
                {
                    Value = 10
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20,
                    AdditionalOutputs = 1
                };
                engine.Pipelines.Add(
                    "Test",
                    a,
                    new Merge(b).ForEachDocument(),
                    new Core.Modules.Metadata.Meta("Content", Config.FromDocument(doc => doc.Content)));

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                Assert.AreEqual(1, a.OutputCount);
                Assert.AreEqual(2, b.OutputCount);
                CollectionAssert.AreEqual(new[] { "1121", "1122" }, engine.Documents["Test"].Select(x => x["Content"]));
            }

            [Test]
            public async Task MultipleInputsSingleResultForEachDocument()
            {
                // Given
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                CountModule a = new CountModule("A")
                {
                    Value = 10,
                    AdditionalOutputs = 1
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20
                };
                engine.Pipelines.Add(
                    "Test",
                    a,
                    new Merge(b).ForEachDocument(),
                    new Core.Modules.Metadata.Meta("Content", Config.FromDocument(doc => doc.Content)));

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                Assert.AreEqual(2, a.OutputCount);
                Assert.AreEqual(2, b.OutputCount);
                CollectionAssert.AreEqual(new[] { "1121", "1222" }, engine.Documents["Test"].Select(x => x["Content"]));
            }

            [Test]
            public async Task MultipleInputsMultipleResultsForEachDocument()
            {
                // Given
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                CountModule a = new CountModule("A")
                {
                    Value = 10,
                    AdditionalOutputs = 1
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20,
                    AdditionalOutputs = 1
                };
                engine.Pipelines.Add(
                    "Test",
                    a,
                    new Merge(b).ForEachDocument(),
                    new Core.Modules.Metadata.Meta("Content", Config.FromDocument(doc => doc.Content)));

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                Assert.AreEqual(2, a.OutputCount);
                Assert.AreEqual(4, b.OutputCount);
                CollectionAssert.AreEqual(new[] { "1121", "1122", "1223", "1224" }, engine.Documents["Test"].Select(x => x["Content"]));
            }
        }
    }
}
