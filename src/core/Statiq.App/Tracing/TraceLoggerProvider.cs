using Microsoft.Extensions.Logging;

namespace Statiq.App
{
    public class TraceLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new TraceLogger(categoryName);

        public void Dispose()
        {
        }
    }
}
