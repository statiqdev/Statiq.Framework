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
    public class EngineFixture : BaseFixture
    {
        public class GetExecutingPipelines : EngineFixture
        {
            [Test]
            public void NullPipelinesAndNoDefaults()
            {
                // Given
                Engine engine = GetEngine();

                // When
                HashSet<string> executingPipelines = engine.GetExecutingPipelines(null, false);

                // Then
                executingPipelines.ShouldBe(new[] { "F" }, true);
            }

            [Test]
            public void NullPipelinesAndDefaults()
            {
                // Given
                Engine engine = GetEngine();

                // When
                HashSet<string> executingPipelines = engine.GetExecutingPipelines(null, true);

                // Then
                executingPipelines.ShouldBe(new[] { "A", "D", "E", "F" }, true);
            }

            [Test]
            public void ZeroLengthAndNoDefaults()
            {
                // Given
                Engine engine = GetEngine();

                // When
                HashSet<string> executingPipelines = engine.GetExecutingPipelines(Array.Empty<string>(), false);

                // Then
                executingPipelines.ShouldBe(new[] { "F" }, true);
            }

            [Test]
            public void ZeroLengthAndDefaults()
            {
                // Given
                Engine engine = GetEngine();

                // When
                HashSet<string> executingPipelines = engine.GetExecutingPipelines(Array.Empty<string>(), true);

                // Then
                executingPipelines.ShouldBe(new[] { "A", "D", "E", "F" }, true);
            }

            [Test]
            public void SpecifiedAndNoDefaults()
            {
                // Given
                Engine engine = GetEngine();

                // When
                HashSet<string> executingPipelines = engine.GetExecutingPipelines(new[] { "A", "B" }, false);

                // Then
                executingPipelines.ShouldBe(new[] { "A", "B", "F" }, true);
            }

            [Test]
            public void SpecifiedAndDefaults()
            {
                // Given
                Engine engine = GetEngine();

                // When
                HashSet<string> executingPipelines = engine.GetExecutingPipelines(new[] { "A", "B" }, true);

                // Then
                executingPipelines.ShouldBe(new[] { "A", "B", "E", "D", "F" }, true);
            }

            [Test]
            public void SpecifiedAndTransitiveAndNoDefaults()
            {
                // Given
                Engine engine = GetEngine();

                // When
                HashSet<string> executingPipelines = engine.GetExecutingPipelines(new[] { "E" }, false);

                // Then
                executingPipelines.ShouldBe(new[] { "A", "D", "E", "F" }, true);
            }

            [Test]
            public void SpecifiedAndTransitiveAndDefaults()
            {
                // Given
                Engine engine = GetEngine();

                // When
                HashSet<string> executingPipelines = engine.GetExecutingPipelines(new[] { "E" }, true);

                // Then
                executingPipelines.ShouldBe(new[] { "A", "D", "E", "F" }, true);
            }

            [Test]
            public void ThrowsForUndefinedPipeline()
            {
                // Given
                Engine engine = GetEngine();

                // When, Then
                Should.Throw<ArgumentException>(() => engine.GetExecutingPipelines(new[] { "Z" }, false));
            }

            private Engine GetEngine()
            {
                Engine engine = new Engine();
                engine.Pipelines.Add("A", new TestPipeline
                {
                    ExecutionPolicy = ExecutionPolicy.Default
                });
                engine.Pipelines.Add("B", new TestPipeline
                {
                    ExecutionPolicy = ExecutionPolicy.Manual
                });
                engine.Pipelines.Add("C", new TestPipeline
                {
                    ExecutionPolicy = ExecutionPolicy.Manual
                });
                engine.Pipelines.Add("D", new TestPipeline
                {
                    ExecutionPolicy = ExecutionPolicy.Manual,
                    Dependencies = new HashSet<string>(new[] { "A" })
                });
                engine.Pipelines.Add("E", new TestPipeline
                {
                    ExecutionPolicy = ExecutionPolicy.Default,
                    Dependencies = new HashSet<string>(new[] { "D" })
                });
                engine.Pipelines.Add("F", new TestPipeline
                {
                    ExecutionPolicy = ExecutionPolicy.Always
                });
                return engine;
            }
        }

        public class GetPipelinePhasesTests : EngineFixture
        {
            [Test]
            public void ThrowsForIsolatedPipelineWithDependencies()
            {
                // Given
                IPipelineCollection pipelines = new TestPipelineCollection();
                pipelines.Add("Bar");
                pipelines.Add("Foo", new TestPipeline
                {
                    Dependencies = new HashSet<string>(new[] { "Bar" }),
                    Isolated = true
                });
                ILogger logger = new TestLoggerProvider().CreateLogger(null);

                // When, Then
                Should.Throw<PipelineException>(() => Engine.GetPipelinePhases(pipelines, logger));
            }

            [Test]
            public void ThrowsForMissingDependency()
            {
                // Given
                IPipelineCollection pipelines = new TestPipelineCollection();
                pipelines.Add("Foo", new TestPipeline
                {
                    Dependencies = new HashSet<string>(new[] { "Bar" })
                });
                ILogger logger = new TestLoggerProvider().CreateLogger(null);

                // When, Then
                Should.Throw<PipelineException>(() => Engine.GetPipelinePhases(pipelines, logger));
            }

            [Test]
            public void ThrowsForSelfDependency()
            {
                // Given
                IPipelineCollection pipelines = new TestPipelineCollection();
                pipelines.Add("Foo", new TestPipeline
                {
                    Dependencies = new HashSet<string>(new[] { "Foo" })
                });
                ILogger logger = new TestLoggerProvider().CreateLogger(null);

                // When, Then
                Should.Throw<PipelineException>(() => Engine.GetPipelinePhases(pipelines, logger));
            }

            [Test]
            public void ThrowsForCyclicDependency()
            {
                // Given
                IPipelineCollection pipelines = new TestPipelineCollection();
                pipelines.Add("Baz", new TestPipeline
                {
                    Dependencies = new HashSet<string>(new[] { "Foo" })
                });
                pipelines.Add("Bar", new TestPipeline
                {
                    Dependencies = new HashSet<string>(new[] { "Baz" })
                });
                pipelines.Add("Foo", new TestPipeline
                {
                    Dependencies = new HashSet<string>(new[] { "Bar" })
                });
                ILogger logger = new TestLoggerProvider().CreateLogger(null);

                // When, Then
                Should.Throw<PipelineException>(() => Engine.GetPipelinePhases(pipelines, logger));
            }

            [Test]
            public void DoesNotThrowForManualDependency()
            {
                // Given
                IPipelineCollection pipelines = new TestPipelineCollection();
                pipelines.Add("Bar", new TestPipeline
                {
                    ExecutionPolicy = ExecutionPolicy.Manual
                });
                pipelines.Add("Foo", new TestPipeline
                {
                    Dependencies = new HashSet<string>(new[] { "Bar" })
                });
                ILogger logger = new TestLoggerProvider().CreateLogger(null);

                // When
                PipelinePhase[] phases = Engine.GetPipelinePhases(pipelines, logger);

                // Then
                phases.Select(x => (x.PipelineName, x.Phase)).ShouldBe(new (string, Phase)[]
                {
                    ("Bar", Phase.Input),
                    ("Foo", Phase.Input),
                    ("Bar", Phase.Process),
                    ("Foo", Phase.Process),
                    ("Bar", Phase.Transform),
                    ("Foo", Phase.Transform),
                    ("Bar", Phase.Output),
                    ("Foo", Phase.Output)
                });
            }

            [Test]
            public void DependenciesAreCaseInsensitive()
            {
                // Given
                IPipelineCollection pipelines = new TestPipelineCollection();
                pipelines.Add("Bar");
                pipelines.Add("Foo", new TestPipeline
                {
                    Dependencies = new HashSet<string>(new[] { "bar" })
                });
                ILogger logger = new TestLoggerProvider().CreateLogger(null);

                // When
                PipelinePhase[] phases = Engine.GetPipelinePhases(pipelines, logger);

                // Then
                phases.Select(x => (x.PipelineName, x.Phase)).ShouldBe(new (string, Phase)[]
                {
                    ("Bar", Phase.Input),
                    ("Foo", Phase.Input),
                    ("Bar", Phase.Process),
                    ("Foo", Phase.Process),
                    ("Bar", Phase.Transform),
                    ("Foo", Phase.Transform),
                    ("Bar", Phase.Output),
                    ("Foo", Phase.Output)
                });
            }

            [Test]
            public void DeploymentPipelinesDependOnOutputPhases()
            {
                // Given
                IPipelineCollection pipelines = new TestPipelineCollection();
                pipelines.Add("Bar", new TestPipeline
                {
                    Deployment = true
                });
                pipelines.Add("Foo", new TestPipeline
                {
                });
                ILogger logger = new TestLoggerProvider().CreateLogger(null);

                // When
                PipelinePhase[] phases = Engine.GetPipelinePhases(pipelines, logger);

                // Then
                phases
                    .Single(x => x.Pipeline.Deployment && x.Phase == Phase.Output)
                    .Dependencies
                    .Select(x => (x.PipelineName, x.Phase))
                    .ShouldBe(new (string, Phase)[]
                    {
                        ("Bar", Phase.Transform),
                        ("Foo", Phase.Output)
                    });
                phases
                    .Single(x => !x.Pipeline.Deployment && x.Phase == Phase.Output)
                    .Dependencies
                    .Select(x => (x.PipelineName, x.Phase))
                    .ShouldBe(new (string, Phase)[]
                    {
                        ("Foo", Phase.Transform)
                    });
            }
        }

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
