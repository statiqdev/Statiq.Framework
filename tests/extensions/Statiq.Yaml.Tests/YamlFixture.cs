using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;
using Statiq.Testing.Documents;
using Statiq.Yaml.Dynamic;

namespace Statiq.Yaml.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class YamlFixture : BaseFixture
    {
        public class ExecuteTests : YamlFixture
        {
            [Test]
            public async Task SetsMetadataKey()
            {
                // Given
                TestDocument document = new TestDocument("A: 1");
                Yaml yaml = new Yaml("MyYaml");

                // When
                TestDocument result = await ExecuteAsync(document, yaml).SingleAsync();

                // Then
                result.Keys.ShouldBe(new[] { "MyYaml", "Id", "Source", "Destination", "ContentProvider", "HasContent" }, true);
            }

            [Test]
            public async Task GeneratesDynamicObject()
            {
                // Given
                TestDocument document = new TestDocument(@"
A: 1
B: true
C: Yes
");
                Yaml yaml = new Yaml("MyYaml");

                // When
                TestDocument result = await ExecuteAsync(document, yaml).SingleAsync();

                // Then
                result.Keys.ShouldBe(new[] { "MyYaml", "Id", "Source", "Destination", "ContentProvider", "HasContent" }, true);
                result["MyYaml"].ShouldBeOfType<DynamicYaml>();
                ((int)((dynamic)result["MyYaml"]).A).ShouldBe(1);
                ((bool)((dynamic)result["MyYaml"]).B).ShouldBe(true);
                ((string)((dynamic)result["MyYaml"]).C).ShouldBe("Yes");
            }

            [Test]
            public async Task FlattensTopLevelScalarNodes()
            {
                // Given
                TestDocument document = new TestDocument(@"
A: 1
B: true
C: Yes
");
                Yaml yaml = new Yaml();

                // When
                TestDocument result = await ExecuteAsync(document, yaml).SingleAsync();

                // Then
                result.Keys.ShouldBe(new[] { "A", "B", "C", "Id", "Source", "Destination", "ContentProvider", "HasContent" }, true);
                result["A"].ShouldBe("1");
                result["B"].ShouldBe("true");
                result["C"].ShouldBe("Yes");
            }

            [Test]
            public async Task GeneratesDynamicObjectAndFlattens()
            {
                // Given
                TestDocument document = new TestDocument(@"
A: 1
B: true
C: Yes
");
                Yaml yaml = new Yaml("MyYaml", true);

                // When
                TestDocument result = await ExecuteAsync(document, yaml).SingleAsync();

                // Then
                result.Keys.ShouldBe(new[] { "MyYaml", "A", "B", "C", "Id", "Source", "Destination", "ContentProvider", "HasContent" }, true);
                result["MyYaml"].ShouldBeOfType<DynamicYaml>();
                ((int)((dynamic)result["MyYaml"]).A).ShouldBe(1);
                ((bool)((dynamic)result["MyYaml"]).B).ShouldBe(true);
                ((string)((dynamic)result["MyYaml"]).C).ShouldBe("Yes");
                result["A"].ShouldBe("1");
                result["B"].ShouldBe("true");
                result["C"].ShouldBe("Yes");
            }

            [Test]
            public async Task ReturnsDocumentIfEmptyInputAndFlatten()
            {
                // Given
                TestDocument document = new TestDocument(@"
");
                Yaml yaml = new Yaml();

                // When
                TestDocument result = await ExecuteAsync(document, yaml).SingleAsync();

                // Then
                result.Keys.ShouldBe(new[] { "Id", "Source", "Destination", "ContentProvider", "HasContent" }, true);
            }

            [Test]
            public async Task EmptyReturnIfEmptyInputAndNotFlatten()
            {
                // Given
                TestDocument document = new TestDocument(@"
");
                Yaml yaml = new Yaml("Foo");

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, yaml);

                // Then
                results.ShouldBeEmpty();
            }

            [Test]
            public async Task UsesDocumentNestingForComplexChildren()
            {
                // Given
                TestDocument document = new TestDocument(@"
C:
  - X: 1
    Y: 2
  - X: 4
    Z: 5
");
                Yaml yaml = new Yaml();

                // When
                TestDocument result = await ExecuteAsync(document, yaml).SingleAsync();

                // Then
                result.Keys.ShouldBe(new[] { "C", "Id", "Source", "Destination", "ContentProvider", "HasContent" }, true);
                result["C"].ShouldBeOfType<IDocument[]>();
                IDocument[] subDocuments = (IDocument[])result["C"];
                subDocuments.Length.ShouldBe(2);
                subDocuments[0].Keys.ShouldBe(new[] { "X", "Y", "Id", "Source", "Destination", "ContentProvider", "HasContent" }, true);
                subDocuments[0]["X"].ShouldBe("1");
                subDocuments[0]["Y"].ShouldBe("2");
                subDocuments[1].Keys.ShouldBe(new[] { "X", "Z", "Id", "Source", "Destination", "ContentProvider", "HasContent" }, true);
                subDocuments[1]["X"].ShouldBe("4");
                subDocuments[1]["Z"].ShouldBe("5");
            }
        }
    }
}