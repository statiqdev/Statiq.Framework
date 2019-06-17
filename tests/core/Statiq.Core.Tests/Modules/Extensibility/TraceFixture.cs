using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;
using Trace = Statiq.Core.Modules.Extensibility.Trace;

namespace Statiq.Core.Tests.Modules.Extensibility
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
            public async Task TestTraceListenerThrows(TraceEventType traceEventType)
            {
                // Given
                Trace trace = new Trace(traceEventType.ToString()).EventType(traceEventType);

                // When, Then
                await Should.ThrowAsync<Exception>(async () => await ExecuteAsync(trace));
            }
        }
    }
}
