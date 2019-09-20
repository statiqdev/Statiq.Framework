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
        private static readonly List<ConsoleContent> ConsoleContents = new List<ConsoleContent>();
        private static readonly object WriteLock = new object();

        public ConsoleLoggerProvider(IOptionsMonitor<BatchingLoggerOptions> options)
            : base(options)
        {
        }

        protected override void WriteMessages(IEnumerable<FlexibleLogMessage> messages, CancellationToken token)
        {
            lock (WriteLock)
            {
                foreach (FlexibleLogMessage message in messages)
                {
                    ConsoleContents.Clear();

                    // Add the log level message
                    string logLevelString = GetLogLevelString(message.LogLevel);
                    ConsoleContents.Add(GetLogLevelConsoleContent(message.LogLevel, logLevelString.AsMemory()));

                    // If this is a debug or trace message, just log the message
                    if ((int)message.LogLevel <= (int)LogLevel.Debug)
                    {
                        ConsoleContents.Add(GetLogLevelConsoleContent(message.LogLevel, message.FormattedMessage.AsMemory()));
                    }
                    else
                    {
                        // Find » and color pipelines/modules differently
                        int lastAngle = message.FormattedMessage.LastIndexOf('»');
                        if (lastAngle > 0)
                        {
                            int firstAngle = message.FormattedMessage.IndexOf('»');
                            ConsoleContents.Add(new ConsoleContent(ConsoleColor.DarkBlue, ConsoleColor.Black, message.FormattedMessage.AsMemory(0, firstAngle + 1)));
                            if (firstAngle < lastAngle)
                            {
                                ConsoleContents.Add(new ConsoleContent(ConsoleColor.Blue, ConsoleColor.Black, message.FormattedMessage.AsMemory(firstAngle + 1, lastAngle - (firstAngle + 1) + 1)));
                            }
                        }
                        else
                        {
                            lastAngle = -1;
                        }

                        // Scan for parens and split into segments
                        int normalStart = lastAngle + 1;
                        int openStart = -1;
                        int openCount = 0;
                        for (int c = normalStart; c < message.FormattedMessage.Length; c++)
                        {
                            if (message.FormattedMessage[c] == '(')
                            {
                                if (openCount == 0)
                                {
                                    openStart = c;
                                }
                                openCount++;
                            }
                            else if (message.FormattedMessage[c] == ')')
                            {
                                if (openCount > 0)
                                {
                                    openCount--;
                                    if (openCount == 0)
                                    {
                                        // Ignore "(s)"
                                        if (c < 2 || message.FormattedMessage[c - 1] != 's' || message.FormattedMessage[c - 2] != '(')
                                        {
                                            if (openStart > normalStart)
                                            {
                                                ConsoleContents.Add(new ConsoleContent(
                                                    message.FormattedMessage.AsMemory(normalStart, openStart - normalStart)));
                                            }

                                            // Inside parens
                                            ConsoleContents.Add(new ConsoleContent(
                                                ConsoleColor.DarkGray,
                                                ConsoleColor.Black,
                                                message.FormattedMessage.AsMemory(openStart, c - openStart + 1)));

                                            normalStart = c + 1;
                                        }

                                        openStart = -1;
                                    }
                                }
                            }
                        }
                        if (normalStart <= message.FormattedMessage.Length - 1)
                        {
                            ConsoleContents.Add(new ConsoleContent(
                                message.FormattedMessage.AsMemory(normalStart, message.FormattedMessage.Length - normalStart)));
                        }
                    }

                    // Write any exceptions
                    if (message.Exception != null)
                    {
                        ConsoleContents.Add(GetLogLevelConsoleContent(LogLevel.Error, (Environment.NewLine + message.Exception.ToString()).AsMemory()));
                    }

                    // Write to the console
                    foreach (ConsoleContent content in ConsoleContents)
                    {
                        Console.ForegroundColor = content.Foreground;
                        Console.BackgroundColor = content.Background;
                        Console.Write(content.Message.ToString());
                    }
                    Console.WriteLine();
                    Console.ResetColor();
                }
            }
        }

        private static string GetLogLevelString(LogLevel logLevel) =>
            logLevel switch
            {
                LogLevel.Trace => "[TRCE] ",
                LogLevel.Debug => "[DBUG] ",
                LogLevel.Information => "[INFO] ",
                LogLevel.Warning => "[WARN] ",
                LogLevel.Error => "[FAIL] ",
                LogLevel.Critical => "[CRIT] ",
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel)),
            };

        // We must explicitly set the background color if we are setting the foreground color,
        // since just setting one can look bad on the users console.
        private ConsoleContent GetLogLevelConsoleContent(LogLevel logLevel, in ReadOnlyMemory<char> message) =>
            logLevel switch
            {
                LogLevel.Critical => new ConsoleContent(ConsoleColor.White, ConsoleColor.Red, message),
                LogLevel.Error => new ConsoleContent(ConsoleColor.Red, ConsoleColor.Black, message),
                LogLevel.Warning => new ConsoleContent(ConsoleColor.Yellow, ConsoleColor.Black, message),
                LogLevel.Information => new ConsoleContent(ConsoleColor.DarkGreen, ConsoleColor.Black, message),
                LogLevel.Debug => new ConsoleContent(ConsoleColor.DarkGray, ConsoleColor.Black, message),
                LogLevel.Trace => new ConsoleContent(ConsoleColor.DarkGray, ConsoleColor.Black, message),
                _ => new ConsoleContent(message),
            };

        private struct ConsoleContent
        {
            public ConsoleContent(ConsoleColor foreground, ConsoleColor background, ReadOnlyMemory<char> message)
            {
                Foreground = foreground;
                Background = background;
                Message = message;
            }

            public ConsoleContent(ReadOnlyMemory<char> message)
            {
                Foreground = ConsoleColor.Gray;
                Background = ConsoleColor.Black;
                Message = message;
            }

            public ConsoleColor Foreground { get; }

            public ConsoleColor Background { get; }

            public ReadOnlyMemory<char> Message { get; }
        }
    }
}
