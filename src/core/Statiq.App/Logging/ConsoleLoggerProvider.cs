using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetEscapades.Extensions.Logging.RollingFile.Internal;

namespace Statiq.App
{
    public class ConsoleLoggerProvider : FlexibleBatchingLoggerProvider
    {
        public ConsoleLoggerProvider(IOptionsMonitor<BatchingLoggerOptions> options)
            : base(options)
        {
        }

        protected override Task WriteMessagesAsync(IEnumerable<FlexibleLogMessage> messages, CancellationToken token)
        {
            foreach (FlexibleLogMessage message in messages)
            {
                string logLevelString = GetLogLevelString(message.LogLevel);
                ConsoleColors? consoleColors = GetLogLevelConsoleColors(message.LogLevel);

                if (consoleColors.HasValue)
                {
                    Console.ForegroundColor = consoleColors.Value.Foreground;
                    Console.BackgroundColor = consoleColors.Value.Background;
                    Console.Write(logLevelString);
                    Console.ResetColor();
                }
                else
                {
                    Console.ResetColor();
                    Console.Write(logLevelString);
                }
                Console.Write(": ");
                Console.WriteLine(message.FormattedMessage);
                if (message.Exception != null)
                {
                    Console.WriteLine(message.Exception.ToString());
                }
            }
            return Task.CompletedTask;
        }

        private static string GetLogLevelString(LogLevel logLevel) =>
            logLevel switch
            {
                LogLevel.Trace => "TRCE",
                LogLevel.Debug => "DBUG",
                LogLevel.Information => "INFO",
                LogLevel.Warning => "WARN",
                LogLevel.Error => "FAIL",
                LogLevel.Critical => "CRIT",
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel)),
            };

        // We must explicitly set the background color if we are setting the foreground color,
        // since just setting one can look bad on the users console.
        private ConsoleColors? GetLogLevelConsoleColors(LogLevel logLevel) =>
            logLevel switch
            {
                LogLevel.Critical => new ConsoleColors(ConsoleColor.White, ConsoleColor.Red),
                LogLevel.Error => new ConsoleColors(ConsoleColor.Black, ConsoleColor.Red),
                LogLevel.Warning => new ConsoleColors(ConsoleColor.Yellow, ConsoleColor.Black),
                LogLevel.Information => new ConsoleColors(ConsoleColor.DarkGreen, ConsoleColor.Black),
                LogLevel.Debug => new ConsoleColors(ConsoleColor.Gray, ConsoleColor.Black),
                LogLevel.Trace => new ConsoleColors(ConsoleColor.Gray, ConsoleColor.Black),
                _ => (ConsoleColors?)null,
            };

        private readonly struct ConsoleColors
        {
            public ConsoleColors(ConsoleColor foreground, ConsoleColor background)
            {
                Foreground = foreground;
                Background = background;
            }

            public ConsoleColor Foreground { get; }

            public ConsoleColor Background { get; }
        }
    }
}
