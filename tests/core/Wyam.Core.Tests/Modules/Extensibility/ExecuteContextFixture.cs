using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Core.Modules.Extensibility;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Core.Tests.Modules.Extensibility
{
    [TestFixture]
    [NonParallelizable]
    public class ExecuteContextFixture : BaseFixture
    {
        public class ExecuteTests : ExecuteContextFixture
        {
            [Test]
            public void DoesNotThrowForNullResult()
            {
                // Given
                ExecuteContext execute = new ExecuteContext(_ => (object)null);

                // When, Then
                Should.NotThrow(() => ExecuteAsync(Array.Empty<TestDocument>(), execute).Result);
            }

            [Test]
            public async Task ThrowsForObjectResult()
            {
                // Given
                ExecuteContext execute = new ExecuteContext(_ => 1);

                // When, Then
                await Should.ThrowAsync<Exception>(async () => await ExecuteAsync(Array.Empty<TestDocument>(), execute));
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
                ExecuteContext execute = new ExecuteContext(_ => (object)null);

                // When
                IReadOnlyList<TestDocument> outputs = await ExecuteAsync(inputs, execute);

                // Then
                CollectionAssert.AreEqual(inputs, outputs);
            }

            [Test]
            public void DoesNotRequireReturnValue()
            {
                // Given
                int a = 0;
                ExecuteContext execute = new ExecuteContext(c => { a = a + 1; });

                // When, Then
                Should.NotThrow(() => ExecuteAsync(Array.Empty<TestDocument>(), execute).Result);
            }

            [Test]
            public async Task ReturnsDocumentForSingleResultDocument()
            {
                // Given
                TestDocument document = new TestDocument();
                ExecuteContext execute = new ExecuteContext(_ => document);

                // When
                TestDocument result = await ExecuteAsync(Array.Empty<TestDocument>(), execute).SingleAsync();

                // Then
                result.ShouldBe(document);
            }

            [Test]
            public async Task RunsModuleAgainstInputDocuments()
            {
                // Given
                TestDocument[] inputs =
                {
                    new TestDocument(),
                    new TestDocument()
                };
                int count = 0;
                ExecuteContext execute = new ExecuteContext(c =>
                {
                    count++;
                    return (object)null;
                });

                // When
                await ExecuteAsync(inputs, execute);

                // Then
                count.ShouldBe(1);
            }
        }
    }
}
