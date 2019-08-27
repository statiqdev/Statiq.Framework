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
        public ConcurrentQueue<TestMessage> Messages { get; } = new ConcurrentQueue<TestMessage>();

        public LogLevel ThrowLogLevel { get; set; } = LogLevel.Warning;

        public ILogger CreateLogger(string categoryName) => new TestLogger(categoryName, Messages, ThrowLogLevel);

        public void Dispose()
        {
        }
    }
}
