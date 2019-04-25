using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common;
using Wyam.Common.Tracing;
using Wyam.Core.Execution;
using Wyam.Core.Modules.Control;
using Wyam.Testing;
using Wyam.Testing.Modules;
using Wyam.Common.Execution;
using Wyam.Testing.Execution;
using Shouldly;

namespace Wyam.Core.Tests.Execution
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class PipelineFixture : BaseFixture
    {
        public class ExecuteTests : PipelineFixture
        {
            [Test]
            public async Task SameSourceIsIgnoredIfAlreadySet()
            {
                // Given
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                CountModule a = new CountModule("A")
                {
                    CloneSource = true
                };
                CountModule b = new CountModule("A")
                {
                    CloneSource = true
                };
                engine.Pipelines.Add("Count", a, b);

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                Assert.AreEqual(1, engine.Documents.FromPipeline("Count").Count());
                Assert.AreEqual(1, a.ExecuteCount);
                Assert.AreEqual(1, b.ExecuteCount);
                Assert.AreEqual(1, a.InputCount);
                Assert.AreEqual(1, b.InputCount);
                Assert.AreEqual(1, a.OutputCount);
                Assert.AreEqual(1, b.OutputCount);
            }

            [Test]
            public async Task SameSourceThrowsException()
            {
                // Given
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                CountModule a = new CountModule("A")
                {
                    CloneSource = true
                };
                CountModule b = new CountModule("B");
                CountModule c = new CountModule("A")
                {
                    CloneSource = true
                };
                engine.Pipelines.Add("Count", a, new Concat(b, c));

                // When, Then
                await Should.ThrowAsync<Exception>(async () => await engine.ExecuteAsync(serviceProvider));
            }

            [Test]
            public void DisposingPipelineDisposesModules()
            {
                // Given
                Engine engine = new Engine();
                DisposableCountModule a = new DisposableCountModule("A");
                DisposableCountModule b = new DisposableCountModule("B");
                CountModule c = new CountModule("C");
                engine.Pipelines.Add("Count", a, new Concat(b, c));

                // When
                engine.Dispose();

                // Then
                Assert.IsTrue(a.Disposed);
                Assert.IsTrue(b.Disposed);
            }

            private class DisposableCountModule : CountModule, IDisposable
            {
                public bool Disposed { get; private set; }

                public DisposableCountModule(string valueKey)
                    : base(valueKey)
                {
                }

                public void Dispose()
                {
                    Disposed = true;
                }
            }
        }
    }
}
