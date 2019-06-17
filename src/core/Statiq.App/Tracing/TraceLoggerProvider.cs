using Microsoft.Extensions.Logging;

namespace Statiq.App.Tracing
{
    public class TraceLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new TraceLogger(categoryName);

        public void Dispose()
        {
        }
    }
}
