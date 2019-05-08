using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Util;
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
            public async Task DoesNotThrowForNullResult()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                ExecuteContext execute = new ExecuteContext(_ => (object)null);

                // When
                await execute.ExecuteAsync(Array.Empty<IDocument>(), context);

                // Then
            }

            [Test]
            public async Task ThrowsForObjectResult()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                ExecuteContext execute = new ExecuteContext(_ => 1);

                // When, Then
                await Should.ThrowAsync<Exception>(async () => await execute.ExecuteAsync(Array.Empty<IDocument>(), context));
            }

            [Test]
            public async Task ReturnsInputsForNullResult()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                IDocument[] inputs =
                {
                    new TestDocument(),
                    new TestDocument()
                };
                ExecuteContext execute = new ExecuteContext(_ => (object)null);

                // When
                IEnumerable<IDocument> outputs = await execute.ExecuteAsync(inputs, context).ToListAsync();

                // Then
                CollectionAssert.AreEqual(inputs, outputs);
            }

            [Test]
            public async Task DoesNotRequireReturnValue()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                int a = 0;
                ExecuteContext execute = new ExecuteContext(c => { a = a + 1; });

                // When
                await execute.ExecuteAsync(Array.Empty<IDocument>(), context);

                // Then
            }

            [Test]
            public async Task ReturnsDocumentForSingleResultDocument()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument();
                ExecuteContext execute = new ExecuteContext(_ => document);

                // When
                IEnumerable<IDocument> result = await execute.ExecuteAsync(Array.Empty<IDocument>(), context);

                // Then
                CollectionAssert.AreEquivalent(document, result.Single());
            }

            [Test]
            public async Task RunsModuleAgainstInputDocuments()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument[] inputs =
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
                await execute.ExecuteAsync(inputs, context).ToListAsync();

                // Then
                count.ShouldBe(1);
            }
        }
    }
}
