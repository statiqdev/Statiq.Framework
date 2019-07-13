using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Extensibility
{
    [TestFixture]
    public class TraceMessageFixture : BaseFixture
    {
        public class ExecuteTests : TraceMessageFixture
        {
            [TestCase(TraceEventType.Critical)]
            [TestCase(TraceEventType.Error)]
            [TestCase(TraceEventType.Warning)]
            public async Task TestTraceListenerThrows(TraceEventType traceEventType)
            {
                // Given
                TraceMessage trace = new TraceMessage(traceEventType.ToString()).EventType(traceEventType);

                // When, Then
                await Should.ThrowAsync<Exception>(async () => await ExecuteAsync(trace));
            }
        }
    }
}
