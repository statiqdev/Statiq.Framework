using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Statiq.App
{
    internal class StrictLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new StrictLogger(this);

        public LogLevel MaximumLogLevel { get; set; }

        public void Dispose()
        {
        }
    }
}
