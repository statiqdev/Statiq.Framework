using System;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.App
{
    internal class ConsoleLogger : ILogger
    {
        private readonly ConsoleLoggerProvider _provider;
        private readonly string _categoryName;

        public ConsoleLogger(ConsoleLoggerProvider provider, string categoryName)
        {
            _provider = provider;
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state) => new EmptyDisposable();

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(DateTimeOffset timestamp, LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) =>
            _provider.AddMessage(new LogMessage(_categoryName, timestamp, logLevel, eventId, formatter(state, exception), exception));

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) =>
            Log(DateTimeOffset.Now, logLevel, eventId, state, exception, formatter);
    }
}
