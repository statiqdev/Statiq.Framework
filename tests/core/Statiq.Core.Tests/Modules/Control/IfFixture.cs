using System.Threading.Tasks;
using NUnit.Framework;
using Statiq.Common.Configuration;
using Statiq.Core.Modules.Control;
using Statiq.Testing;
using Statiq.Testing.Modules;

namespace Statiq.Core.Tests.Modules.Control
{
    [TestFixture]
    public class IfFixture : BaseFixture
    {
        public class ExecuteTests : IfFixture
        {
            [Test]
            public async Task IfResultsInCorrectCounts()
            {
                // Given
                CountModule a = new CountModule("A")
                {
                    AdditionalOutputs = 2,
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
                await ExecuteAsync(a, new If(Config.FromDocument(async doc => await doc.GetStringAsync() == "1"), b), c);

                // Then
                Assert.AreEqual(1, a.ExecuteCount);
                Assert.AreEqual(1, b.ExecuteCount);
                Assert.AreEqual(1, c.ExecuteCount);
                Assert.AreEqual(1, a.InputCount);
                Assert.AreEqual(1, b.InputCount);
                Assert.AreEqual(5, c.InputCount);
                Assert.AreEqual(3, a.OutputCount);
                Assert.AreEqual(3, b.OutputCount);
                Assert.AreEqual(20, c.OutputCount);
            }

            [Test]
            public async Task ElseIfResultsInCorrectCounts()
            {
                // Given
                CountModule a = new CountModule("A")
                {
                    AdditionalOutputs = 2,
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
                CountModule d = new CountModule("B")
                {
                    AdditionalOutputs = 2
                };

                // When
                await ExecuteAsync(
                    a,
                    new If(Config.FromDocument(async doc => await doc.GetStringAsync() == "1"), b)
                        .ElseIf(Config.FromDocument(async doc => await doc.GetStringAsync() == "2"), c),
                    d);

                // Then
                Assert.AreEqual(1, a.ExecuteCount);
                Assert.AreEqual(1, b.ExecuteCount);
                Assert.AreEqual(1, c.ExecuteCount);
                Assert.AreEqual(1, d.ExecuteCount);
                Assert.AreEqual(1, a.InputCount);
                Assert.AreEqual(1, b.InputCount);
                Assert.AreEqual(1, c.InputCount);
                Assert.AreEqual(8, d.InputCount);
                Assert.AreEqual(3, a.OutputCount);
                Assert.AreEqual(3, b.OutputCount);
                Assert.AreEqual(4, c.OutputCount);
                Assert.AreEqual(24, d.OutputCount);
            }

            [Test]
            public async Task ElseResultsInCorrectCounts()
            {
                // Given
                CountModule a = new CountModule("A")
                {
                    AdditionalOutputs = 2,
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
                CountModule d = new CountModule("B")
                {
                    AdditionalOutputs = 2
                };

                // When
                await ExecuteAsync(
                    a,
                    new If(Config.FromDocument(async doc => await doc.GetStringAsync() == "1"), b)
                        .Else(c),
                    d);

                // Then
                Assert.AreEqual(1, a.ExecuteCount);
                Assert.AreEqual(1, b.ExecuteCount);
                Assert.AreEqual(1, c.ExecuteCount);
                Assert.AreEqual(1, d.ExecuteCount);
                Assert.AreEqual(1, a.InputCount);
                Assert.AreEqual(1, b.InputCount);
                Assert.AreEqual(2, c.InputCount);
                Assert.AreEqual(11, d.InputCount);
                Assert.AreEqual(3, a.OutputCount);
                Assert.AreEqual(3, b.OutputCount);
                Assert.AreEqual(8, c.OutputCount);
                Assert.AreEqual(33, d.OutputCount);
            }

            [Test]
            public async Task IfElseAndElseResultsInCorrectCounts()
            {
                // Given
                CountModule a = new CountModule("A")
                {
                    AdditionalOutputs = 3,
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
                CountModule d = new CountModule("B")
                {
                    AdditionalOutputs = 2
                };
                CountModule e = new CountModule("B")
                {
                    AdditionalOutputs = 3
                };

                // When
                await ExecuteAsync(
                    a,
                    new If(Config.FromDocument(async doc => await doc.GetStringAsync() == "1"), b)
                        .ElseIf(Config.FromDocument(async doc => await doc.GetStringAsync() == "3"), c)
                        .Else(d),
                    e);

                // Then
                Assert.AreEqual(1, a.ExecuteCount);
                Assert.AreEqual(1, b.ExecuteCount);
                Assert.AreEqual(1, c.ExecuteCount);
                Assert.AreEqual(1, d.ExecuteCount);
                Assert.AreEqual(1, e.ExecuteCount);
                Assert.AreEqual(1, a.InputCount);
                Assert.AreEqual(1, b.InputCount);
                Assert.AreEqual(1, c.InputCount);
                Assert.AreEqual(2, d.InputCount);
                Assert.AreEqual(13, e.InputCount);
                Assert.AreEqual(4, a.OutputCount);
                Assert.AreEqual(3, b.OutputCount);
                Assert.AreEqual(4, c.OutputCount);
                Assert.AreEqual(6, d.OutputCount);
                Assert.AreEqual(52, e.OutputCount);
            }

            [Test]
            public async Task IfWithContextResultsInCorrectCounts()
            {
                // Given
                CountModule a = new CountModule("A")
                {
                    AdditionalOutputs = 2,
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
                await ExecuteAsync(a, new If(Config.FromContext(x => true), b), c);

                // Then
                Assert.AreEqual(1, a.ExecuteCount);
                Assert.AreEqual(1, b.ExecuteCount);
                Assert.AreEqual(1, c.ExecuteCount);
                Assert.AreEqual(1, a.InputCount);
                Assert.AreEqual(3, b.InputCount);
                Assert.AreEqual(9, c.InputCount);
                Assert.AreEqual(3, a.OutputCount);
                Assert.AreEqual(9, b.OutputCount);
                Assert.AreEqual(36, c.OutputCount);
            }

            [Test]
            public async Task FalseIfWithContextResultsInCorrectCounts()
            {
                // Given
                CountModule a = new CountModule("A")
                {
                    AdditionalOutputs = 2,
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
                await ExecuteAsync(a, new If(Config.FromContext(x => false), b), c);

                // Then
                Assert.AreEqual(1, a.ExecuteCount);
                Assert.AreEqual(0, b.ExecuteCount);
                Assert.AreEqual(1, c.ExecuteCount);
                Assert.AreEqual(1, a.InputCount);
                Assert.AreEqual(0, b.InputCount);
                Assert.AreEqual(3, c.InputCount);
                Assert.AreEqual(3, a.OutputCount);
                Assert.AreEqual(0, b.OutputCount);
                Assert.AreEqual(12, c.OutputCount);
            }

            [Test]
            public async Task UnmatchedDocumentsAreAddedToResults()
            {
                // Given
                CountModule a = new CountModule("A")
                {
                    AdditionalOutputs = 2,
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
                await ExecuteAsync(a, new If(Config.FromDocument((doc, ctx) => false), b), c);

                // Then
                Assert.AreEqual(1, a.ExecuteCount);
                Assert.AreEqual(0, b.ExecuteCount);
                Assert.AreEqual(1, c.ExecuteCount);
                Assert.AreEqual(1, a.InputCount);
                Assert.AreEqual(0, b.InputCount);
                Assert.AreEqual(3, c.InputCount);
                Assert.AreEqual(3, a.OutputCount);
                Assert.AreEqual(0, b.OutputCount);
                Assert.AreEqual(12, c.OutputCount);
            }

            [Test]
            public async Task UnmatchedDocumentsAreNotAddedToResults()
            {
                // Given
                CountModule a = new CountModule("A")
                {
                    AdditionalOutputs = 2,
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
                await ExecuteAsync(a, new If(false, b).WithoutUnmatchedDocuments(), c);

                // Then
                Assert.AreEqual(1, a.ExecuteCount);
                Assert.AreEqual(0, b.ExecuteCount);
                Assert.AreEqual(1, c.ExecuteCount);
                Assert.AreEqual(1, a.InputCount);
                Assert.AreEqual(0, b.InputCount);
                Assert.AreEqual(0, c.InputCount);
                Assert.AreEqual(3, a.OutputCount);
                Assert.AreEqual(0, b.OutputCount);
                Assert.AreEqual(0, c.OutputCount);
            }
        }
    }
}
