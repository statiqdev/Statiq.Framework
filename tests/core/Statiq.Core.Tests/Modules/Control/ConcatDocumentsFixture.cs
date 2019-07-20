using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Statiq.Testing;
using Shouldly;
using Statiq.Common;

namespace Statiq.Core.Tests.Modules.Control
{
    [TestFixture]
    public class ConcatDocumentsFixture : BaseFixture
    {
        public class ExecuteTests : ConcatDocumentsFixture
        {
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
                IReadOnlyList<IDocument> results = await ExecuteAsync(a, new ConcatDocuments(b), c);

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
