using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Execution
{
    [TestFixture]
    public class ExecutionPipelineFixture : BaseFixture
    {
        public class ExecuteTests : ExecutionPipelineFixture
        {
            [Test]
            public async Task ExecutesCorrectPipelines()
            {
                // Given
                Engine engine = new Engine();
                engine.Pipelines.Add(new TestExecutionPipeline());
                CancellationTokenSource cts = new CancellationTokenSource();

                // When
                IPipelineOutputs outputs = await engine.ExecuteAsync(cts);

                // Then
                outputs["TestExecution"].Cast<TestDocument>().Select(x => x.Content).Single().ShouldBe("Foo");
            }
        }

        private class TestExecutionPipeline : ExecutionPipeline
        {
            protected override async Task<IEnumerable<IDocument>> ExecuteProcessPhaseAsync(IExecutionContext context)
            {
                return await new TestDocument("Foo").YieldAsync();
            }
        }
    }
}
