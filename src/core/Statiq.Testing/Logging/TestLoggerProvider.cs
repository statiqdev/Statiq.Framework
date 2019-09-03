using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestLoggerProvider : ILoggerProvider
    {
        public ConcurrentQueue<TestMessage> Messages { get; }

        public TestLoggerProvider(ConcurrentQueue<TestMessage> messages = null)
        {
            Messages = messages ?? new ConcurrentQueue<TestMessage>();
        }

        public LogLevel ThrowLogLevel { get; set; } = LogLevel.Warning;

        public ILogger CreateLogger(string categoryName) => new TestLogger(categoryName, ThrowLogLevel, Messages);

        public void Dispose()
        {
        }
    }
}
