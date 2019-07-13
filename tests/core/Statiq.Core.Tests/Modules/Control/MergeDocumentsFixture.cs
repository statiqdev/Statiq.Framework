using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Statiq.Testing;
using Shouldly;
using Statiq.Common;

namespace Statiq.Core.Tests.Modules.Control
{
    [TestFixture]
    public class MergeDocumentsFixture : BaseFixture
    {
        public class ExecuteTests : MergeDocumentsFixture
        {
            [Test]
            public async Task ReplacesContent()
            {
                // Given
                CountModule a = new CountModule("A")
                {
                    Value = 10,
                    EnsureInputDocument = true
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20,
                    EnsureInputDocument = true
                };

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(
                    a,
                    new MergeDocuments(b),
                    new AddMetadata("Content", Config.FromDocument(async doc => await doc.GetStringAsync())));

                // Then
                CollectionAssert.AreEqual(new[] { "21" }, results.Select(x => x["Content"]));
            }

            [Test]
            public async Task CombinesMetadata()
            {
                // Given
                CountModule a = new CountModule("A")
                {
                    Value = 10,
                    EnsureInputDocument = true
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20,
                    EnsureInputDocument = true
                };

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(a, new MergeDocuments(b));

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
                    Value = 10,
                    EnsureInputDocument = true
                };
                CountModule b = new CountModule("A")
                {
                    Value = 20,
                    EnsureInputDocument = true
                };

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(a, new MergeDocuments(b));

                // Then
                CollectionAssert.AreEqual(new[] { 21 }, results.Select(x => x["A"]));
            }

            [Test]
            public async Task SingleInputSingleResult()
            {
                // Given
                CountModule a = new CountModule("A")
                {
                    Value = 10,
                    EnsureInputDocument = true
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20,
                    EnsureInputDocument = true
                };

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(a, new MergeDocuments(b));

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
                    Value = 10,
                    EnsureInputDocument = true
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20,
                    AdditionalOutputs = 1,
                    EnsureInputDocument = true
                };

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(a, new MergeDocuments(b));

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
                    AdditionalOutputs = 1,
                    EnsureInputDocument = true
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20,
                    EnsureInputDocument = true
                };

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(a, new MergeDocuments(b));

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
                    AdditionalOutputs = 1,
                    EnsureInputDocument = true
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20,
                    AdditionalOutputs = 1,
                    EnsureInputDocument = true
                };

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(a, new MergeDocuments(b));

                // Then
                Assert.AreEqual(2, a.OutputCount);
                Assert.AreEqual(2, b.OutputCount);
                CollectionAssert.AreEqual(new[] { 11, 11, 12, 12 }, results.Select(x => x["A"]));
                CollectionAssert.AreEqual(new[] { 21, 22, 21, 22 }, results.Select(x => x["B"]));
            }

            [Test]
            public async Task ResultsInCorrectCountsWithInputDocuments()
            {
                // Given
                CountModule a = new CountModule("A")
                {
                    AdditionalOutputs = 1,
                    EnsureInputDocument = true
                };
                CountModule b = new CountModule("B")
                {
                    AdditionalOutputs = 2,
                    EnsureInputDocument = true
                };
                CountModule c = new CountModule("C")
                {
                    AdditionalOutputs = 3,
                    EnsureInputDocument = true
                };

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(a, new MergeDocuments(b).WithInputDocuments(), c);

                // Then
                Assert.AreEqual(1, a.ExecuteCount);
                Assert.AreEqual(1, b.ExecuteCount);
                Assert.AreEqual(1, c.ExecuteCount);
                Assert.AreEqual(1, a.InputCount);
                Assert.AreEqual(2, b.InputCount);
                Assert.AreEqual(12, c.InputCount);
                Assert.AreEqual(2, a.OutputCount);
                Assert.AreEqual(6, b.OutputCount);
                Assert.AreEqual(48, c.OutputCount);
                results.Count.ShouldBe(48);
            }
        }
    }
}
