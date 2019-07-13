using NUnit.Framework;
using Shouldly;
using Statiq.Testing;
using Statiq.Testing.Documents;
using Statiq.Testing.Execution;

namespace Statiq.CodeAnalysis.Tests
{
    [TestFixture]
    public class IDocumentExtensionsFixture : BaseFixture
    {
        public class InterpolateTests : IDocumentExtensionsFixture
        {
            [Test]
            public void SimpleInterpolation()
            {
                // Given
                TestDocument document = new TestDocument();
                TestExecutionContext context = new TestExecutionContext();

                // When
                string result = document.Interpolate("ABC {1+2} XYZ", context);

                // Then
                result.ShouldBe("ABC 3 XYZ");
            }

            [Test]
            public void DocumentMetadataIsAvailable()
            {
                // Given
                TestDocument document = new TestDocument()
                {
                    { "Foo", 5 }
                };
                TestExecutionContext context = new TestExecutionContext();

                // When
                string result = document.Interpolate("ABC {Foo} XYZ", context);

                // Then
                result.ShouldBe("ABC 5 XYZ");
            }
        }
    }
}
