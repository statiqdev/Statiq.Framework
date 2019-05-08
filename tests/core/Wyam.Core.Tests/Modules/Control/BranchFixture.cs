using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Core.Modules.Control;
using Wyam.Testing;
using Wyam.Testing.Modules;

namespace Wyam.Core.Tests.Modules.Control
{
    [TestFixture]
    [NonParallelizable]
    public class BranchFixture : BaseFixture
    {
        public class ExecuteTests : BranchFixture
        {
            [Test]
            public async Task ResultsInCorrectCounts()
            {
                // Given
                CountModule a = new CountModule("A")
                {
                    AdditionalOutputs = 1,
                    EnsureInputDocument = true
                };
                CountModule b = new CountModule("B")
                {
                    AdditionalOutputs = 2
                };
                CountModule c = new CountModule("C")
                {
                    AdditionalOutputs = 3
                };

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(a, new Branch(b), c);

                // Then
                Assert.AreEqual(1, a.ExecuteCount);
                Assert.AreEqual(1, b.ExecuteCount);
                Assert.AreEqual(1, c.ExecuteCount);
                Assert.AreEqual(1, a.InputCount);
                Assert.AreEqual(2, b.InputCount);
                Assert.AreEqual(2, c.InputCount);
                Assert.AreEqual(2, a.OutputCount);
                Assert.AreEqual(6, b.OutputCount);
                Assert.AreEqual(8, c.OutputCount);
            }

            [Test]
            public async Task ResultsInCorrectCountsWithPredicate()
            {
                // Given
                CountModule a = new CountModule("A")
                {
                    AdditionalOutputs = 1,
                    EnsureInputDocument = true
                };
                CountModule b = new CountModule("B")
                {
                    AdditionalOutputs = 2
                };
                CountModule c = new CountModule("C")
                {
                    AdditionalOutputs = 3
                };

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(a, new Branch(b).Where(Config.FromDocument(x => x.Content == "1")), c);

                // Then
                Assert.AreEqual(1, a.ExecuteCount);
                Assert.AreEqual(1, b.ExecuteCount);
                Assert.AreEqual(1, c.ExecuteCount);
                Assert.AreEqual(1, a.InputCount);
                Assert.AreEqual(1, b.InputCount);
                Assert.AreEqual(2, c.InputCount);
                Assert.AreEqual(2, a.OutputCount);
                Assert.AreEqual(3, b.OutputCount);
                Assert.AreEqual(8, c.OutputCount);
            }
        }
    }
}
