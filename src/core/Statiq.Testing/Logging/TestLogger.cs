using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestLogger : ILogger
    {
        public ConcurrentQueue<string> Messages { get; } = new ConcurrentQueue<string>();

        public string CategoryName { get; }

        public TestLogger(string categoryName)
        {
            CategoryName = categoryName;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Messages.Enqueue($"{logLevel} [{eventId}]: {formatter(state, exception)}");
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable BeginScope<TState>(TState state)
        {
            return new EmptyDisposable();
        }
    }
}
