using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestLogger : ILogger
    {
        public ConcurrentQueue<TestMessage> Messages { get; }

        public string CategoryName { get; }

        public TestLogger()
            : this(null, null, LogLevel.Warning)
        {
        }

        public TestLogger(LogLevel throwLogLevel)
            : this(null, null, throwLogLevel)
        {
        }

        public TestLogger(string categoryName)
            : this(categoryName, null, LogLevel.Warning)
        {
        }

        public TestLogger(string categoryName, ConcurrentQueue<TestMessage> messages, LogLevel throwLogLevel)
        {
            CategoryName = categoryName;
            Messages = messages ?? new ConcurrentQueue<TestMessage>();
            ThrowLogLevel = throwLogLevel;
        }

        public LogLevel ThrowLogLevel { get; set; } = LogLevel.Warning;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            string formatted = formatter(state, exception);
            Messages.Enqueue(new TestMessage(CategoryName, logLevel, eventId, state, exception, formatted));
            TestContext.Out.WriteLine(formatted);
            if ((int)logLevel >= (int)ThrowLogLevel)
            {
                throw new Exception(formatted);
            }
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable BeginScope<TState>(TState state)
        {
            return new EmptyDisposable();
        }
    }
}
