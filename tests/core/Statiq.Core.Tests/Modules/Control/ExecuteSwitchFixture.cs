using System.Threading.Tasks;
using NUnit.Framework;
using Statiq.Common;
using Statiq.Testing;
using Statiq.Testing.Modules;

namespace Statiq.Core.Tests.Modules.Control
{
    [TestFixture]
    public class ExecuteSwitchFixture : BaseFixture
    {
        public class ExecuteTests : ExecuteSwitchFixture
        {
            [Test]
            public async Task SwitchResultsInCorrectCounts()
            {
                // Given
                CountModule a = new CountModule("A") { AdditionalOutputs = 2 };
                CountModule b = new CountModule("B");
                CountModule c = new CountModule("C");
                CountModule d = new CountModule("D");
                ExecuteSwitch switchModule = new ExecuteSwitch(Config.FromDocument(async x => (object)await x.GetStringAsync())).Case("1", b).Case("2", c).Default(d);

                // When
                await ExecuteAsync(a, switchModule);

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
                CountModule a = new CountModule("A")
                {
                    AdditionalOutputs = 2,
                    EnsureInputDocument = true
                };
                CountModule b = new CountModule("B");
                CountModule c = new CountModule("C");
                ExecuteSwitch switchModule = new ExecuteSwitch(Config.FromDocument(async x => (object)await x.GetStringAsync())).Default(b);

                // When
                await ExecuteAsync(a, switchModule, c);

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
                CountModule a = new CountModule("A")
                {
                    AdditionalOutputs = 2,
                    EnsureInputDocument = true
                };
                CountModule b = new CountModule("B");
                CountModule c = new CountModule("C");
                ExecuteSwitch switchModule = new ExecuteSwitch(Config.FromDocument(async x => (object)await x.GetStringAsync())).Case("1", b);

                // When
                await ExecuteAsync(a, switchModule, c);

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
                CountModule a = new CountModule("A")
                {
                    AdditionalOutputs = 2,
                    EnsureInputDocument = true
                };
                CountModule b = new CountModule("B");
                CountModule c = new CountModule("C");
                ExecuteSwitch switchModule = new ExecuteSwitch(Config.FromDocument(async x => (object)await x.GetStringAsync())).Case(new string[] { "1", "2" }, b);

                // When
                await ExecuteAsync(a, switchModule, c);

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
                CountModule a = new CountModule("A")
                {
                    AdditionalOutputs = 2,
                    EnsureInputDocument = true
                };
                CountModule b = new CountModule("B");
                ExecuteSwitch switchModule = new ExecuteSwitch(Config.FromDocument(async x => (object)await x.GetStringAsync()));

                // When
                await ExecuteAsync(a, switchModule, b);

                // Then
                Assert.AreEqual(3, b.InputCount);
            }
        }
    }
}
