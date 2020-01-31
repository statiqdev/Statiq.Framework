using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.CodeAnalysis.Tests
{
    [TestFixture]
    public class EvaluateMetadataFixture : BaseFixture
    {
        public class ExecuteTests : EvaluateMetadataFixture
        {
            [Test]
            public async Task IgnoresIfNoPrefix()
            {
                // Given
                TestDocument document = new TestDocument
                {
                    { "Foo", "$\"ABC {1+2} XYZ\"" }
                };
                EvaluateMetadata interpolate = new EvaluateMetadata();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, interpolate);

                // Then
                TestDocument result = results.ShouldHaveSingleItem();
                ((IMetadata)result).GetString("Foo").ShouldBe("$\"ABC {1+2} XYZ\"");
            }

            [Test]
            public async Task EvaluatesExpression()
            {
                // Given
                TestDocument document = new TestDocument
                {
                    { "Foo", "=> $\"ABC {1+2} XYZ\"" }
                };
                EvaluateMetadata interpolate = new EvaluateMetadata();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, interpolate);

                // Then
                TestDocument result = results.ShouldHaveSingleItem();
                ((IMetadata)result).GetString("Foo").ShouldBe("ABC 3 XYZ");
            }

            [Test]
            public async Task EvaluatesReturnStatement()
            {
                // Given
                TestDocument document = new TestDocument
                {
                    { "Foo", "=> return $\"ABC {1+2} XYZ\";" }
                };
                EvaluateMetadata interpolate = new EvaluateMetadata();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, interpolate);

                // Then
                TestDocument result = results.ShouldHaveSingleItem();
                ((IMetadata)result).GetString("Foo").ShouldBe("ABC 3 XYZ");
            }

            [Test]
            public async Task EvaluatesMultipleStatements()
            {
                // Given
                TestDocument document = new TestDocument
                {
                    { "Foo", "=> { int x = 1 + 2; return $\"ABC {x} XYZ\"; }" }
                };
                EvaluateMetadata interpolate = new EvaluateMetadata();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, interpolate);

                // Then
                TestDocument result = results.ShouldHaveSingleItem();
                ((IMetadata)result).GetString("Foo").ShouldBe("ABC 3 XYZ");
            }

            [Test]
            public async Task EvaluatesMultipleStatementsWithoutBraces()
            {
                // Given
                TestDocument document = new TestDocument
                {
                    { "Foo", "=> int x = 1 + 2; return $\"ABC {x} XYZ\";" }
                };
                EvaluateMetadata interpolate = new EvaluateMetadata();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, interpolate);

                // Then
                TestDocument result = results.ShouldHaveSingleItem();
                ((IMetadata)result).GetString("Foo").ShouldBe("ABC 3 XYZ");
            }

            [Test]
            public async Task DoesNotEvaluateNonStrings()
            {
                // Given
                TestDocument document = new TestDocument
                {
                    { "Foo", new StringBuilder("=> $\"ABC {1+2} XYZ\"") }
                };
                EvaluateMetadata interpolate = new EvaluateMetadata();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, interpolate);

                // Then
                TestDocument result = results.ShouldHaveSingleItem();
                ((IMetadata)result).GetString("Foo").ShouldBe("=> $\"ABC {1+2} XYZ\"");
            }

            [Test]
            public async Task NonEvaluatedMetadataIsAvailable()
            {
                // Given
                TestDocument document = new TestDocument
                {
                    { "Foo", "=> $\"ABC {Bar} XYZ\"" },
                    { "Bar", 5 }
                };
                EvaluateMetadata interpolate = new EvaluateMetadata();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, interpolate);

                // Then
                TestDocument result = results.ShouldHaveSingleItem();
                ((IMetadata)result).GetString("Foo").ShouldBe("ABC 5 XYZ");
            }
        }
    }
}
