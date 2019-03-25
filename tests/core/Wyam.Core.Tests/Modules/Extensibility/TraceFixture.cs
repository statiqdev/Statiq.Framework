using System;
using System.Diagnostics;
using NUnit.Framework;
using Wyam.Common.Execution;
using Wyam.Core.Execution;
using Wyam.Testing;
using Wyam.Testing.Execution;
using Trace = Wyam.Core.Modules.Extensibility.Trace;

namespace Wyam.Core.Tests.Modules.Extensibility
{
    [TestFixture]
    [NonParallelizable]
    public class TraceFixture : BaseFixture
    {
        public class ExecuteTests : TraceFixture
        {
            [TestCase(TraceEventType.Critical)]
            [TestCase(TraceEventType.Error)]
            [TestCase(TraceEventType.Warning)]
            public void TestTraceListenerThrows(TraceEventType traceEventType)
            {
                // Given
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                engine.Pipelines.Add(new Trace(traceEventType.ToString()).EventType(traceEventType));

                // When
                TestDelegate test = () => engine.ExecuteAsync(serviceProvider);

                // Then
                Assert.Throws<Exception>(test);
            }
        }
    }
}
