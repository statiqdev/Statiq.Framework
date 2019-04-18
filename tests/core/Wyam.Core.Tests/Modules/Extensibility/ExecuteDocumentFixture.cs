using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Common.Util;
using Wyam.Core.Execution;
using Wyam.Core.Modules.Extensibility;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;
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
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                Execute execute = new ExecuteDocument((object)null);
                engine.Pipelines.Add(execute);

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
            }

            [Test]
            public async Task ReturnsInputsForNullResult()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument[] inputs =
                {
                    new TestDocument(),
                    new TestDocument()
                };
                Execute execute = new ExecuteDocument((object)null);

                // When
                IEnumerable<IDocument> outputs = await ((IModule)execute).ExecuteAsync(inputs, context).ToListAsync();

                // Then
                CollectionAssert.AreEqual(inputs, outputs);
            }

            [Test]
            public async Task DoesNotRequireReturnValue()
            {
                // Given
                IServiceProvider serviceProvider = new TestServiceProvider();
                int a = 0;
                Engine engine = new Engine();
                Execute execute = new ExecuteDocument((d, c) => { a = a + 1; });
                engine.Pipelines.Add(execute);

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
            }

            [Test]
            public async Task ReturnsDocumentForSingleResultDocument()
            {
                // Given
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                TestDocument document = new TestDocument();
                Execute execute = new ExecuteDocument(document);
                engine.Pipelines.Add("Test", execute);

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                CollectionAssert.AreEquivalent(new[] { document }, engine.Documents["Test"]);
            }

            [Test]
            public async Task RunsModuleAgainstEach()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument[] inputs =
                {
                    new TestDocument(),
                    new TestDocument()
                };
                int count = 0;
                Execute execute = new ExecuteDocument(Config.FromDocument((d, c) =>
                {
                    count++;
                    return (object)null;
                }));

                // When
                await ((IModule)execute).ExecuteAsync(inputs, context).ToListAsync();

                // Then
                count.ShouldBe(2);
            }

            [Test]
            public async Task SetsNewContent()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument[] inputs =
                {
                    new TestDocument(),
                    new TestDocument()
                };
                int count = 0;
                Execute execute = new ExecuteDocument(Config.FromDocument((d, c) => (object)count++));

                // When
                List<IDocument> results = await ((IModule)execute).ExecuteAsync(inputs, context).ToListAsync();

                // Then
                CollectionAssert.AreEquivalent(results.Select(x => x.Content), new[] { "0", "1" });
            }
        }
    }
}
