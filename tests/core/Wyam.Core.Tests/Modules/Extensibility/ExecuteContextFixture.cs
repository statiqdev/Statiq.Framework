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
    public class ExecuteContextFixture : BaseFixture
    {
        public class ExecuteTests : ExecuteContextFixture
        {
            [Test]
            public async Task DoesNotThrowForNullResult()
            {
                // Given
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                Execute execute = new ExecuteContext(_ => null);
                engine.Pipelines.Add(execute);

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
            }

            [Test]
            public async Task ThrowsForObjectResult()
            {
                // Given
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                Execute execute = new ExecuteContext((ContextConfig)1);
                engine.Pipelines.Add(execute);

                // When, Then
                await Should.ThrowAsync<Exception>(async () => await engine.ExecuteAsync(serviceProvider));
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
                Execute execute = new ExecuteContext((ContextConfig)null);

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
                Execute execute = new ExecuteContext(c => { a = a + 1; });
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
                IDocument document = new TestDocument();
                Execute execute = new ExecuteContext((ContextConfig)document);
                engine.Pipelines.Add("Test", execute);

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                CollectionAssert.AreEquivalent(new[] { document }, engine.Documents["Test"]);
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
                Execute execute = new ExecuteContext(Config.FromContext(c =>
                {
                    count++;
                    return null;
                }));

                // When
                await ((IModule)execute).ExecuteAsync(inputs, context).ToListAsync();

                // Then
                count.ShouldBe(1);
            }
        }
    }
}
