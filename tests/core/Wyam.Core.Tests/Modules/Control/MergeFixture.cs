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
using Wyam.Common.Documents;

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
                CountModule a = new CountModule("A")
                {
                    Value = 10
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20
                };

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(
                    a,
                    new Merge(b),
                    new Core.Modules.Metadata.Meta("Content", Config.FromDocument(doc => doc.Content)));

                // Then
                CollectionAssert.AreEqual(new[] { "21" }, results.Select(x => x["Content"]));
            }

            [Test]
            public async Task CombinesMetadata()
            {
                // Given
                CountModule a = new CountModule("A")
                {
                    Value = 10
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20
                };

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(a, new Merge(b));

                // Then
                CollectionAssert.AreEqual(new[] { 11 }, results.Select(x => x["A"]));
                CollectionAssert.AreEqual(new[] { 21 }, results.Select(x => x["B"]));
            }

            [Test]
            public async Task CombinesAndOverwritesMetadata()
            {
                // Given
                CountModule a = new CountModule("A")
                {
                    Value = 10
                };
                CountModule b = new CountModule("A")
                {
                    Value = 20
                };

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(a, new Merge(b));

                // Then
                CollectionAssert.AreEqual(new[] { 21 }, results.Select(x => x["A"]));
            }

            [Test]
            public async Task SingleInputSingleResult()
            {
                // Given
                CountModule a = new CountModule("A")
                {
                    Value = 10
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20
                };

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(a, new Merge(b));

                // Then
                Assert.AreEqual(1, a.OutputCount);
                Assert.AreEqual(1, b.OutputCount);
                CollectionAssert.AreEqual(new[] { 11 }, results.Select(x => x["A"]));
                CollectionAssert.AreEqual(new[] { 21 }, results.Select(x => x["B"]));
            }

            [Test]
            public async Task SingleInputMultipleResults()
            {
                // Given
                CountModule a = new CountModule("A")
                {
                    Value = 10
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20,
                    AdditionalOutputs = 1
                };

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(a, new Merge(b));

                // Then
                Assert.AreEqual(1, a.OutputCount);
                Assert.AreEqual(2, b.OutputCount);
                CollectionAssert.AreEqual(new[] { 11, 11 }, results.Select(x => x["A"]));
                CollectionAssert.AreEqual(new[] { 21, 22 }, results.Select(x => x["B"]));
            }

            [Test]
            public async Task MultipleInputsSingleResult()
            {
                // Given
                CountModule a = new CountModule("A")
                {
                    Value = 10,
                    AdditionalOutputs = 1
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20
                };

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(a, new Merge(b));

                // Then
                Assert.AreEqual(2, a.OutputCount);
                Assert.AreEqual(1, b.OutputCount);
                CollectionAssert.AreEqual(new[] { 11, 12 }, results.Select(x => x["A"]));
                CollectionAssert.AreEqual(new[] { 21, 21 }, results.Select(x => x["B"]));
            }

            [Test]
            public async Task MultipleInputsMultipleResults()
            {
                // Given
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

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(a, new Merge(b));

                // Then
                Assert.AreEqual(2, a.OutputCount);
                Assert.AreEqual(2, b.OutputCount);
                CollectionAssert.AreEqual(new[] { 11, 11, 12, 12 }, results.Select(x => x["A"]));
                CollectionAssert.AreEqual(new[] { 21, 22, 21, 22 }, results.Select(x => x["B"]));
            }

            [Test]
            public async Task SingleInputSingleResultForEachDocument()
            {
                // Given
                CountModule a = new CountModule("A")
                {
                    Value = 10
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20
                };

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(
                    a,
                    new Merge(b).ForEachDocument(),
                    new Core.Modules.Metadata.Meta("Content", Config.FromDocument(doc => doc.Content)));

                // Then
                Assert.AreEqual(1, a.OutputCount);
                Assert.AreEqual(1, b.OutputCount);
                CollectionAssert.AreEqual(new[] { "1121" }, results.Select(x => x["Content"]));
            }

            [Test]
            public async Task SingleInputMultipleResultsForEachDocument()
            {
                // Given
                CountModule a = new CountModule("A")
                {
                    Value = 10
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20,
                    AdditionalOutputs = 1
                };

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(
                    a,
                    new Merge(b).ForEachDocument(),
                    new Core.Modules.Metadata.Meta("Content", Config.FromDocument(doc => doc.Content)));

                // Then
                Assert.AreEqual(1, a.OutputCount);
                Assert.AreEqual(2, b.OutputCount);
                CollectionAssert.AreEqual(new[] { "1121", "1122" }, results.Select(x => x["Content"]));
            }

            [Test]
            public async Task MultipleInputsSingleResultForEachDocument()
            {
                // Given
                CountModule a = new CountModule("A")
                {
                    Value = 10,
                    AdditionalOutputs = 1
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20
                };

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(
                    a,
                    new Merge(b).ForEachDocument(),
                    new Core.Modules.Metadata.Meta("Content", Config.FromDocument(doc => doc.Content)));

                // Then
                Assert.AreEqual(2, a.OutputCount);
                Assert.AreEqual(2, b.OutputCount);
                CollectionAssert.AreEqual(new[] { "1121", "1222" }, results.Select(x => x["Content"]));
            }

            [Test]
            public async Task MultipleInputsMultipleResultsForEachDocument()
            {
                // Given
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

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(
                    a,
                    new Merge(b).ForEachDocument(),
                    new Core.Modules.Metadata.Meta("Content", Config.FromDocument(doc => doc.Content)));

                // Then
                Assert.AreEqual(2, a.OutputCount);
                Assert.AreEqual(4, b.OutputCount);
                CollectionAssert.AreEqual(new[] { "1121", "1122", "1223", "1224" }, results.Select(x => x["Content"]));
            }
        }
    }
}
