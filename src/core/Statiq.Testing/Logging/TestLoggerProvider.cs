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
        public ConcurrentQueue<TestLogger> Loggers { get; } = new ConcurrentQueue<TestLogger>();

        public ILogger CreateLogger(string categoryName)
        {
            TestLogger logger = new TestLogger(categoryName);
            Loggers.Enqueue(logger);
            return logger;
        }

        public void Dispose()
        {
        }
    }
}
