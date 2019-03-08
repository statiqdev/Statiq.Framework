using Microsoft.Extensions.Logging;

namespace Wyam.App.Tracing
{
    public class TraceLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new TraceLogger(categoryName);

        public void Dispose()
        {
        }
    }
}
