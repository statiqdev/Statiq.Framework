using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Control
{
    [TestFixture]
    public class ExecuteIfFixture : BaseFixture
    {
        public class ExecuteTests : ExecuteIfFixture
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
                await ExecuteAsync(a, new ExecuteIf(Config.FromDocument(async doc => await doc.GetContentStringAsync() == "1"), b), c);

                // Then
                a.ExecuteCount.ShouldBe(1);
                b.ExecuteCount.ShouldBe(1);
                c.ExecuteCount.ShouldBe(1);
                a.InputCount.ShouldBe(1);
                b.InputCount.ShouldBe(1);
                c.InputCount.ShouldBe(5);
                a.OutputCount.ShouldBe(3);
                b.OutputCount.ShouldBe(3);
                c.OutputCount.ShouldBe(20);
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
                    new ExecuteIf(Config.FromDocument(async doc => await doc.GetContentStringAsync() == "1"), b)
                        .ElseIf(Config.FromDocument(async doc => await doc.GetContentStringAsync() == "2"), c),
                    d);

                // Then
                a.ExecuteCount.ShouldBe(1);
                b.ExecuteCount.ShouldBe(1);
                c.ExecuteCount.ShouldBe(1);
                d.ExecuteCount.ShouldBe(1);
                a.InputCount.ShouldBe(1);
                b.InputCount.ShouldBe(1);
                c.InputCount.ShouldBe(1);
                d.InputCount.ShouldBe(8);
                a.OutputCount.ShouldBe(3);
                b.OutputCount.ShouldBe(3);
                c.OutputCount.ShouldBe(4);
                d.OutputCount.ShouldBe(24);
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
                    new ExecuteIf(Config.FromDocument(async doc => await doc.GetContentStringAsync() == "1"), b)
                        .Else(c),
                    d);

                // Then
                a.ExecuteCount.ShouldBe(1);
                b.ExecuteCount.ShouldBe(1);
                c.ExecuteCount.ShouldBe(1);
                d.ExecuteCount.ShouldBe(1);
                a.InputCount.ShouldBe(1);
                b.InputCount.ShouldBe(1);
                c.InputCount.ShouldBe(2);
                d.InputCount.ShouldBe(11);
                a.OutputCount.ShouldBe(3);
                b.OutputCount.ShouldBe(3);
                c.OutputCount.ShouldBe(8);
                d.OutputCount.ShouldBe(33);
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
                    new ExecuteIf(Config.FromDocument(async doc => await doc.GetContentStringAsync() == "1"), b)
                        .ElseIf(Config.FromDocument(async doc => await doc.GetContentStringAsync() == "3"), c)
                        .Else(d),
                    e);

                // Then
                a.ExecuteCount.ShouldBe(1);
                b.ExecuteCount.ShouldBe(1);
                c.ExecuteCount.ShouldBe(1);
                d.ExecuteCount.ShouldBe(1);
                e.ExecuteCount.ShouldBe(1);
                a.InputCount.ShouldBe(1);
                b.InputCount.ShouldBe(1);
                c.InputCount.ShouldBe(1);
                d.InputCount.ShouldBe(2);
                e.InputCount.ShouldBe(13);
                a.OutputCount.ShouldBe(4);
                b.OutputCount.ShouldBe(3);
                c.OutputCount.ShouldBe(4);
                d.OutputCount.ShouldBe(6);
                e.OutputCount.ShouldBe(52);
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
                await ExecuteAsync(a, new ExecuteIf(Config.FromContext(x => true), b), c);

                // Then
                a.ExecuteCount.ShouldBe(1);
                b.ExecuteCount.ShouldBe(1);
                c.ExecuteCount.ShouldBe(1);
                a.InputCount.ShouldBe(1);
                b.InputCount.ShouldBe(3);
                c.InputCount.ShouldBe(9);
                a.OutputCount.ShouldBe(3);
                b.OutputCount.ShouldBe(9);
                c.OutputCount.ShouldBe(36);
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
                await ExecuteAsync(a, new ExecuteIf(Config.FromContext(x => false), b), c);

                // Then
                a.ExecuteCount.ShouldBe(1);
                b.ExecuteCount.ShouldBe(0);
                c.ExecuteCount.ShouldBe(1);
                a.InputCount.ShouldBe(1);
                b.InputCount.ShouldBe(0);
                c.InputCount.ShouldBe(3);
                a.OutputCount.ShouldBe(3);
                b.OutputCount.ShouldBe(0);
                c.OutputCount.ShouldBe(12);
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
                await ExecuteAsync(a, new ExecuteIf(Config.FromDocument((doc, ctx) => false), b), c);

                // Then
                a.ExecuteCount.ShouldBe(1);
                b.ExecuteCount.ShouldBe(0);
                c.ExecuteCount.ShouldBe(1);
                a.InputCount.ShouldBe(1);
                b.InputCount.ShouldBe(0);
                c.InputCount.ShouldBe(3);
                a.OutputCount.ShouldBe(3);
                b.OutputCount.ShouldBe(0);
                c.OutputCount.ShouldBe(12);
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
                await ExecuteAsync(a, new ExecuteIf(false, b).WithoutUnmatchedDocuments(), c);

                // Then
                a.ExecuteCount.ShouldBe(1);
                b.ExecuteCount.ShouldBe(0);
                c.ExecuteCount.ShouldBe(1);
                a.InputCount.ShouldBe(1);
                b.InputCount.ShouldBe(0);
                c.InputCount.ShouldBe(0);
                a.OutputCount.ShouldBe(3);
                b.OutputCount.ShouldBe(0);
                c.OutputCount.ShouldBe(0);
            }

            [Test]
            public async Task MetadataKeyTrueCondition()
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
                await ExecuteAsync(a, new ExecuteIf("A", b), c);

                // Then
                a.ExecuteCount.ShouldBe(1);
                b.ExecuteCount.ShouldBe(1);
                c.ExecuteCount.ShouldBe(1);
                a.InputCount.ShouldBe(1);
                b.InputCount.ShouldBe(3);
                c.InputCount.ShouldBe(9);
                a.OutputCount.ShouldBe(3);
                b.OutputCount.ShouldBe(9);
                c.OutputCount.ShouldBe(36);
            }

            [Test]
            public async Task MetadataKeyFalseCondition()
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
                await ExecuteAsync(a, new ExecuteIf("B", b), c);

                // Then
                a.ExecuteCount.ShouldBe(1);
                b.ExecuteCount.ShouldBe(0);
                c.ExecuteCount.ShouldBe(1);
                a.InputCount.ShouldBe(1);
                b.InputCount.ShouldBe(0);
                c.InputCount.ShouldBe(3);
                a.OutputCount.ShouldBe(3);
                b.OutputCount.ShouldBe(0);
                c.OutputCount.ShouldBe(12);
            }

            [Test]
            public async Task ExecutesWhenNoInputDocumentsForTrue()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings.Set("Foo", "true");
                ExecuteIf module = new ExecuteIf(Config.FromContext(x => x.Settings.GetBool("Foo")), new CreateDocuments("Bar"));

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(context, module);

                // Then
                results.Select(x => x.Content).ShouldBe(new[] { "Bar" });
            }

            [Test]
            public async Task DoesNotExecuteWhenNoInputDocumentsForFalse()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings.Set("Foo", "true");
                ExecuteIf module = new ExecuteIf(Config.FromContext(x => !x.Settings.GetBool("Foo")), new CreateDocuments("Bar"));

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(context, module);

                // Then
                results.ShouldBeEmpty();
            }
        }
    }
}
