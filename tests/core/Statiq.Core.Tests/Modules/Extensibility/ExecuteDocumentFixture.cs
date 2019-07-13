using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Extensibility
{
    [TestFixture]
    public class ExecuteDocumentFixture : BaseFixture
    {
        public class ExecuteTests : ExecuteDocumentFixture
        {
            [Test]
            public async Task DoesNotThrowForNullResult()
            {
                // Given
                ExecuteDocument execute = new ExecuteDocument((object)null);

                // When
                await ExecuteAsync(execute);

                // Then
            }

            [Test]
            public async Task ReturnsInputsForNullResult()
            {
                // Given
                TestDocument[] inputs =
                {
                    new TestDocument(),
                    new TestDocument()
                };
                ExecuteDocument execute = new ExecuteDocument((object)null);

                // When
                IReadOnlyList<TestDocument> outputs = await ExecuteAsync(inputs, execute);

                // Then
                CollectionAssert.AreEqual(inputs, outputs);
            }

            [Test]
            public async Task DoesNotRequireReturnValue()
            {
                // Given
                int a = 0;
                ExecuteDocument execute = new ExecuteDocument((d, c) => { a = a + 1; });

                // When
                await ExecuteAsync(execute);

                // Then
            }

            [Test]
            public async Task ReturnsDocumentForSingleResultDocument()
            {
                // Given
                TestDocument document = new TestDocument();
                CountModule count = new CountModule("A")
                {
                    EnsureInputDocument = true
                };
                ExecuteDocument execute = new ExecuteDocument(document);

                // When
                IReadOnlyList<TestDocument> result = await ExecuteAsync(count, execute);

                // Then
                CollectionAssert.AreEquivalent(document, result.Single());
            }

            [Test]
            public async Task RunsModuleAgainstEach()
            {
                // Given
                TestDocument[] inputs =
                {
                    new TestDocument(),
                    new TestDocument()
                };
                int count = 0;
                ExecuteDocument execute = new ExecuteDocument(
                    Config.FromDocument((d, c) =>
                    {
                        count++;
                        return (object)null;
                    }),
                    false);

                // When
                await ExecuteAsync(inputs, execute);

                // Then
                count.ShouldBe(2);
            }

            [Test]
            public async Task SetsNewContent()
            {
                // Given
                TestDocument[] inputs =
                {
                    new TestDocument(),
                    new TestDocument()
                };
                int count = 0;
                ExecuteDocument execute = new ExecuteDocument(Config.FromDocument((d, c) => (object)count++));

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(inputs, execute);

                // Then
                results.Select(x => x.Content).ShouldBe(new[] { "0", "1" }, true);
            }
        }
    }
}
