using System;
using Microsoft.Extensions.Logging;

namespace Statiq.App
{
    internal class LogMessage
    {
        public LogMessage(
            string categoryName,
            in DateTimeOffset timestamp,
            LogLevel logLevel,
            in EventId eventId,
            string formattedMessage,
            Exception exception)
        {
            CategoryName = categoryName;
            Timestamp = timestamp;
            LogLevel = logLevel;
            EventId = eventId;
            FormattedMessage = formattedMessage;
            Exception = exception;
        }

        public string CategoryName { get; }
        public DateTimeOffset Timestamp { get; }
        public LogLevel LogLevel { get; }
        public EventId EventId { get; }
        public string FormattedMessage { get; }
        public Exception Exception { get; }
    }
}
