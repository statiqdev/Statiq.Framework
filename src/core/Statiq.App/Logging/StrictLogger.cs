using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.App
{
    internal class StrictLogger : ILogger
    {
        private readonly StrictLoggerProvider _provider;

        public StrictLogger(StrictLoggerProvider provider)
        {
            _provider = provider;
        }

        public IDisposable BeginScope<TState>(TState state) => EmptyDisposable.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logLevel != LogLevel.None && logLevel > _provider.MaximumLogLevel)
            {
                _provider.MaximumLogLevel = logLevel;
            }
        }
    }
}
