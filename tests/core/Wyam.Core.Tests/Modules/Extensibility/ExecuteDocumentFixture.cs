using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Core.Modules.Extensibility;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Modules;

namespace Wyam.Core.Tests.Modules.Extensibility
{
    [TestFixture]
    [NonParallelizable]
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
                IDocument[] inputs =
                {
                    new TestDocument(),
                    new TestDocument()
                };
                ExecuteDocument execute = new ExecuteDocument((object)null);

                // When
                IEnumerable<IDocument> outputs = await ExecuteAsync(inputs, execute);

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
                IEnumerable<IDocument> result = await ExecuteAsync(count, execute);

                // Then
                CollectionAssert.AreEquivalent(document, result.Single());
            }

            [Test]
            public async Task RunsModuleAgainstEach()
            {
                // Given
                IDocument[] inputs =
                {
                    new TestDocument(),
                    new TestDocument()
                };
                int count = 0;
                ExecuteDocument execute = new ExecuteDocument(Config.FromDocument((d, c) =>
                {
                    count++;
                    return (object)null;
                }));

                // When
                await ExecuteAsync(inputs, execute);

                // Then
                count.ShouldBe(2);
            }

            [Test]
            public async Task SetsNewContent()
            {
                // Given
                IDocument[] inputs =
                {
                    new TestDocument(),
                    new TestDocument()
                };
                int count = 0;
                ExecuteDocument execute = new ExecuteDocument(Config.FromDocument((d, c) => (object)count++));

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(inputs, execute);

                // Then
                CollectionAssert.AreEquivalent(results.Select(x => x.Content), new[] { "0", "1" });
            }
        }
    }
}
