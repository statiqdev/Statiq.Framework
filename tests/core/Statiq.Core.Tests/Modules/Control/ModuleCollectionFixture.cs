using System.Threading.Tasks;
using NUnit.Framework;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Control
{
    [TestFixture]
    public class ModuleCollectionFixture : BaseFixture
    {
        public class ExecuteTests : ModuleCollectionFixture
        {
            [Test]
            public async Task ChildModulesAreExecuted()
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
                await ExecuteAsync(a, new ModuleCollection(b, c));

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
            }
        }
    }
}
