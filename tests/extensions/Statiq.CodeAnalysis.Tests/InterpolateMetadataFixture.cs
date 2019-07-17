using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.CodeAnalysis.Tests
{
    [TestFixture]
    public class InterpolateMetadataFixture : BaseFixture
    {
        public class ExecuteTests : InterpolateMetadataFixture
        {
            [Test]
            public async Task InterpolatesMetadata()
            {
                // Given
                TestDocument document = new TestDocument
                {
                    { "Foo", "ABC {1+2} XYZ" }
                };
                InterpolateMetadata interpolate = new InterpolateMetadata();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, interpolate);

                // Then
                results.ShouldHaveSingleItem();
                results[0]["Foo"].ShouldBe("ABC 3 XYZ");
            }

            [Test]
            public async Task DoesNotInterpolateNonStrings()
            {
                // Given
                TestDocument document = new TestDocument
                {
                    { "Foo", new StringBuilder("ABC {1+2} XYZ") }
                };
                InterpolateMetadata interpolate = new InterpolateMetadata();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, interpolate);

                // Then
                results.ShouldHaveSingleItem();
                results[0]["Foo"].ToString().ShouldBe("ABC {1+2} XYZ");
            }

            [Test]
            public async Task NonInterpolatedMetadataIsAvailable()
            {
                // Given
                TestDocument document = new TestDocument
                {
                    { "Foo", "ABC {Bar} XYZ" },
                    { "Bar", 5 }
                };
                InterpolateMetadata interpolate = new InterpolateMetadata();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, interpolate);

                // Then
                results.ShouldHaveSingleItem();
                results[0]["Foo"].ShouldBe("ABC 5 XYZ");
            }
        }
    }
}
