using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Statiq.Core.Modules.Control;
using Statiq.Testing;
using Statiq.Testing.Modules;
using Statiq.Common.Execution;
using Statiq.Common.Configuration;
using Statiq.Common.Documents;
using Shouldly;

namespace Statiq.Core.Tests.Modules.Control
{
    [TestFixture]
    public class ExecuteModulesFixture : BaseFixture
    {
        public class ExecuteTests : ExecuteModulesFixture
        {
            [Test]
            public async Task ReplacesContentOnMerge()
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
                    new ExecuteModules(b).WithResults(ExecuteModuleResults.Merge),
                    new Core.Modules.Metadata.Meta("Content", Config.FromDocument(async doc => await doc.GetStringAsync())));

                // Then
                CollectionAssert.AreEqual(new[] { "21" }, results.Select(x => x["Content"]));
            }

            [Test]
            public async Task CombinesMetadataOnMerge()
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
                IReadOnlyList<IDocument> results = await ExecuteAsync(a, new ExecuteModules(b).WithResults(ExecuteModuleResults.Merge));

                // Then
                CollectionAssert.AreEqual(new[] { 11 }, results.Select(x => x["A"]));
                CollectionAssert.AreEqual(new[] { 21 }, results.Select(x => x["B"]));
            }

            [Test]
            public async Task CombinesAndOverwritesMetadataOnMerge()
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
                IReadOnlyList<IDocument> results = await ExecuteAsync(a, new ExecuteModules(b).WithResults(ExecuteModuleResults.Merge));

                // Then
                CollectionAssert.AreEqual(new[] { 21 }, results.Select(x => x["A"]));
            }

            [Test]
            public async Task SingleInputSingleResultOnMerge()
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
                IReadOnlyList<IDocument> results = await ExecuteAsync(a, new ExecuteModules(b).WithResults(ExecuteModuleResults.Merge));

                // Then
                Assert.AreEqual(1, a.OutputCount);
                Assert.AreEqual(1, b.OutputCount);
                CollectionAssert.AreEqual(new[] { 11 }, results.Select(x => x["A"]));
                CollectionAssert.AreEqual(new[] { 21 }, results.Select(x => x["B"]));
            }

            [Test]
            public async Task SingleInputMultipleResultsOnMerge()
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
                IReadOnlyList<IDocument> results = await ExecuteAsync(a, new ExecuteModules(b).WithResults(ExecuteModuleResults.Merge));

                // Then
                Assert.AreEqual(1, a.OutputCount);
                Assert.AreEqual(2, b.OutputCount);
                CollectionAssert.AreEqual(new[] { 11, 11 }, results.Select(x => x["A"]));
                CollectionAssert.AreEqual(new[] { 21, 22 }, results.Select(x => x["B"]));
            }

            [Test]
            public async Task MultipleInputsSingleResultOnMerge()
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
                IReadOnlyList<IDocument> results = await ExecuteAsync(a, new ExecuteModules(b).WithResults(ExecuteModuleResults.Merge));

                // Then
                Assert.AreEqual(2, a.OutputCount);
                Assert.AreEqual(1, b.OutputCount);
                CollectionAssert.AreEqual(new[] { 11, 12 }, results.Select(x => x["A"]));
                CollectionAssert.AreEqual(new[] { 21, 21 }, results.Select(x => x["B"]));
            }

            [Test]
            public async Task MultipleInputsMultipleResultsOnMerge()
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
                IReadOnlyList<IDocument> results = await ExecuteAsync(a, new ExecuteModules(b).WithResults(ExecuteModuleResults.Merge));

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
                IReadOnlyList<IDocument> results = await ExecuteAsync(a, new ExecuteModules(b).WithInputDocuments(), c);

                // Then
                Assert.AreEqual(1, a.ExecuteCount);
                Assert.AreEqual(1, b.ExecuteCount);
                Assert.AreEqual(1, c.ExecuteCount);
                Assert.AreEqual(1, a.InputCount);
                Assert.AreEqual(2, b.InputCount);
                Assert.AreEqual(6, c.InputCount);
                Assert.AreEqual(2, a.OutputCount);
                Assert.AreEqual(6, b.OutputCount);
                Assert.AreEqual(24, c.OutputCount);
                results.Count.ShouldBe(24);
            }

            [Test]
            public async Task ResultsInCorrectCountsWithInputDocumentsOnConcat()
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
                IReadOnlyList<IDocument> results = await ExecuteAsync(a, new ExecuteModules(b).WithInputDocuments().WithResults(ExecuteModuleResults.Concat), c);

                // Then
                Assert.AreEqual(1, a.ExecuteCount);
                Assert.AreEqual(1, b.ExecuteCount);
                Assert.AreEqual(1, c.ExecuteCount);
                Assert.AreEqual(1, a.InputCount);
                Assert.AreEqual(2, b.InputCount);
                Assert.AreEqual(8, c.InputCount);
                Assert.AreEqual(2, a.OutputCount);
                Assert.AreEqual(6, b.OutputCount);
                Assert.AreEqual(32, c.OutputCount);
                results.Count.ShouldBe(32);
            }
        }
    }
}
