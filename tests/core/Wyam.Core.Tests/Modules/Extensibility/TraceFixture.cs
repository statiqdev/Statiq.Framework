using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Documents;
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
