using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.CodeAnalysis.Tests
{
    [TestFixture]
    public class EvaluateScriptFixture : BaseFixture
    {
        public class ExecuteTests : EvaluateScriptFixture
        {
            [Test]
            public async Task ChangesContentWhenStringResult()
            {
                // Given
                TestDocument document = new TestDocument("return 1 + 2;");
                EvaluateScript evaluateScript = new EvaluateScript();

                // When
                IReadOnlyList<TestDocument> result = await ExecuteAsync(document, evaluateScript);

                // Then
                result.Single().Content.ShouldBe("3");
            }

            [Test]
            public async Task CanAccessMetadataAsGlobalProperty()
            {
                // Given
                TestDocument document = new TestDocument("return (int)Foo + 2;")
                {
                    { "Foo", 5 }
                };
                EvaluateScript evaluateScript = new EvaluateScript();

                // When
                IReadOnlyList<TestDocument> result = await ExecuteAsync(document, evaluateScript);

                // Then
                result.Single().Content.ShouldBe("7");
            }

            [Test]
            public async Task CanAccessMetadata()
            {
                // Given
                TestDocument document = new TestDocument("return GetInt(\"Foo\") + 2;")
                {
                    { "Foo", "5" }
                };
                EvaluateScript evaluateScript = new EvaluateScript();

                // When
                IReadOnlyList<TestDocument> result = await ExecuteAsync(document, evaluateScript);

                // Then
                result.Single().Content.ShouldBe("7");
            }

            [Test]
            public async Task CompileThenEvaluate()
            {
                // Given
                TestDocument document = new TestDocument("return (int)Foo + 2;")
                {
                    { "Foo", 5 }
                };
                CompileScript compileScript = new CompileScript();
                EvaluateScript evaluateScript = new EvaluateScript();

                // When
                IReadOnlyList<TestDocument> result = await ExecuteAsync(document, compileScript, evaluateScript);

                // Then
                result.Single().Content.ShouldBe("7");
            }

            [Test]
            public async Task MultipleStatements()
            {
                // Given
                TestDocument document = new TestDocument("int x = 1 + 2; return x;");
                EvaluateScript evaluateScript = new EvaluateScript();

                // When
                IReadOnlyList<TestDocument> result = await ExecuteAsync(document, evaluateScript);

                // Then
                result.Single().Content.ShouldBe("3");
            }

            [Test]
            public async Task DoesNotRequireReturn()
            {
                // Given
                TestDocument document = new TestDocument("1 + 2");
                EvaluateScript evaluateScript = new EvaluateScript();

                // When
                IReadOnlyList<TestDocument> result = await ExecuteAsync(document, evaluateScript);

                // Then
                result.Single().Content.ShouldBe("3");
            }

            [Test]
            public async Task ReturnsEmptyContentForExpression()
            {
                // Given
                TestDocument document = new TestDocument("_ = 1 + 2;");
                EvaluateScript evaluateScript = new EvaluateScript();

                // When
                IReadOnlyList<TestDocument> result = await ExecuteAsync(document, evaluateScript);

                // Then
                result.Single().Content.ShouldBeEmpty();
            }
        }
    }
}
