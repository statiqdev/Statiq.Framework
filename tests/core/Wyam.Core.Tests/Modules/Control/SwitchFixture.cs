using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Core.Execution;
using Wyam.Core.Modules.Control;
using Wyam.Testing;
using Wyam.Testing.Execution;
using Wyam.Testing.Modules;

namespace Wyam.Core.Tests.Modules.Control
{
    [TestFixture]
    [NonParallelizable]
    public class SwitchFixture : BaseFixture
    {
        public class ExecuteTests : SwitchFixture
        {
            [Test]
            public async Task SwitchResultsInCorrectCounts()
            {
                // Given
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                CountModule a = new CountModule("A") { AdditionalOutputs = 2 };
                CountModule b = new CountModule("B");
                CountModule c = new CountModule("C");
                CountModule d = new CountModule("D");

                engine.Pipelines.Add(a, new Switch(Config.FromDocument(x => (object)x.Content)).Case("1", b).Case("2", c).Default(d));

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                Assert.AreEqual(1, a.ExecuteCount);
                Assert.AreEqual(1, b.ExecuteCount);
                Assert.AreEqual(1, c.ExecuteCount);
                Assert.AreEqual(1, d.ExecuteCount);
            }

            [Test]
            public async Task SwitchNoCasesResultsInCorrectCounts()
            {
                // Given
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                CountModule a = new CountModule("A") { AdditionalOutputs = 2 };
                CountModule b = new CountModule("B");
                CountModule c = new CountModule("C");

                engine.Pipelines.Add(a, new Switch(Config.FromDocument(x => (object)x.Content)).Default(b), c);

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                Assert.AreEqual(1, a.ExecuteCount);
                Assert.AreEqual(1, b.ExecuteCount);
                Assert.AreEqual(3, b.InputCount);
                Assert.AreEqual(3, b.OutputCount);
                Assert.AreEqual(3, c.InputCount);
            }

            [Test]
            public async Task MissingDefaultResultsInCorrectCounts()
            {
                // Given
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                CountModule a = new CountModule("A") { AdditionalOutputs = 2 };
                CountModule b = new CountModule("B");
                CountModule c = new CountModule("C");

                engine.Pipelines.Add(a, new Switch(Config.FromDocument(x => (object)x.Content)).Case("1", b), c);

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                Assert.AreEqual(1, a.ExecuteCount);
                Assert.AreEqual(1, b.ExecuteCount);
                Assert.AreEqual(1, b.InputCount);
                Assert.AreEqual(1, b.OutputCount);
                Assert.AreEqual(3, c.InputCount);
            }

            [Test]
            public async Task ArrayInCaseResultsInCorrectCounts()
            {
                // Given
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                CountModule a = new CountModule("A") { AdditionalOutputs = 2 };
                CountModule b = new CountModule("B");
                CountModule c = new CountModule("C");

                engine.Pipelines.Add(a, new Switch(Config.FromDocument(x => (object)x.Content)).Case(new string[] { "1", "2" }, b), c);

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                Assert.AreEqual(1, a.ExecuteCount);
                Assert.AreEqual(1, b.ExecuteCount);
                Assert.AreEqual(2, b.InputCount);
                Assert.AreEqual(2, b.OutputCount);
                Assert.AreEqual(3, c.InputCount);
            }

            [Test]
            public async Task OmittingCasesAndDefaultResultsInCorrectCounts()
            {
                // Given
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                CountModule a = new CountModule("A") { AdditionalOutputs = 2 };
                CountModule b = new CountModule("B");

                engine.Pipelines.Add(a, new Switch(Config.FromDocument(x => (object)x.Content)), b);

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                Assert.AreEqual(3, b.InputCount);
            }
        }
    }
}
