using System;
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
    public class EngineFixture : BaseFixture
    {
        public class GetServiceTests : EngineFixture
        {
            [Test]
            public void GetsEngineService()
            {
                // Given
                Engine engine = new Engine();

                // When
                IReadOnlyFileSystem fileSystem = engine.Services.GetRequiredService<IReadOnlyFileSystem>();

                // Then
                fileSystem.ShouldBe(engine.FileSystem);
            }

            [Test]
            public void GetsExternalService()
            {
                // Given
                TestFileProvider testFileProvider = new TestFileProvider();
                ServiceCollection serviceCollection = new ServiceCollection();
                serviceCollection.AddSingleton<IFileProvider>(testFileProvider);
                Engine engine = new Engine(serviceCollection);

                // When
                IFileProvider fileProvider = engine.Services.GetRequiredService<IFileProvider>();

                // Then
                fileProvider.ShouldBe(testFileProvider);
            }

            [Test]
            public void GetsEngineServiceInNestedScope()
            {
                // Given
                Engine engine = new Engine();
                IServiceScopeFactory serviceScopeFactory = engine.Services.GetRequiredService<IServiceScopeFactory>();
                IServiceScope serviceScope = serviceScopeFactory.CreateScope();

                // When
                IReadOnlyFileSystem fileSystem = serviceScope.ServiceProvider.GetRequiredService<IReadOnlyFileSystem>();

                // Then
                fileSystem.ShouldBe(engine.FileSystem);
            }
        }

        public class ExecuteTests : EngineFixture
        {
            [Test]
            public async Task ExecutesModule()
            {
                // Given
                Engine engine = new Engine();
                IPipeline pipeline = engine.Pipelines.Add("TestPipeline");
                CountModule module = new CountModule("Foo")
                {
                    EnsureInputDocument = true
                };
                pipeline.ProcessModules.Add(module);
                CancellationTokenSource cts = new CancellationTokenSource();

                // When
                IPipelineOutputs outputs = await engine.ExecuteAsync(cts);

                // Then
                module.ExecuteCount.ShouldBe(1);
                outputs["TestPipeline"].Select(x => x.GetInt("Foo")).ShouldBe(new int[] { 1 });
            }

            [Test]
            public async Task BeforeModuleEventOverriddesOutputs()
            {
                // Given
                Engine engine = new Engine();
                IPipeline pipeline = engine.Pipelines.Add("TestPipeline");
                CountModule module = new CountModule("Foo")
                {
                    EnsureInputDocument = true
                };
                pipeline.ProcessModules.Add(module);
                CancellationTokenSource cts = new CancellationTokenSource();
                engine.Events.Subscribe<BeforeModuleExecution>(x => x.OverrideOutputs(new TestDocument()
                {
                    { "Foo", 123 }
                }.Yield()));

                // When
                IPipelineOutputs outputs = await engine.ExecuteAsync(cts);

                // Then
                module.ExecuteCount.ShouldBe(0);
                outputs["TestPipeline"].Select(x => x.GetInt("Foo")).ShouldBe(new int[] { 123 });
            }

            [Test]
            public async Task AfterModuleEventOverriddesOutputs()
            {
                // Given
                Engine engine = new Engine();
                IPipeline pipeline = engine.Pipelines.Add("TestPipeline");
                CountModule module = new CountModule("Foo")
                {
                    EnsureInputDocument = true
                };
                pipeline.ProcessModules.Add(module);
                CancellationTokenSource cts = new CancellationTokenSource();
                engine.Events.Subscribe<AfterModuleExecution>(x => x.OverrideOutputs(new TestDocument()
                {
                    { "Foo", x.Outputs[0].GetInt("Foo") + 123 }
                }.Yield()));

                // When
                IPipelineOutputs outputs = await engine.ExecuteAsync(cts);

                // Then
                module.ExecuteCount.ShouldBe(1);
                outputs["TestPipeline"].Select(x => x.GetInt("Foo")).ShouldBe(new int[] { 124 });
            }
        }
    }
}
