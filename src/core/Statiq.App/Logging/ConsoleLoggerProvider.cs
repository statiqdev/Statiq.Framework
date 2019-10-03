using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetEscapades.Extensions.Logging.RollingFile.Internal;
using Statiq.Common;

namespace Statiq.App
{
    public class ConsoleLoggerProvider : ILoggerProvider
    {
        private const string NamespacePrefix = nameof(Statiq) + ".";

        private static readonly object WriteLock = new object();
        private static readonly List<ConsoleContent> ConsoleContents = new List<ConsoleContent>();
        private static readonly ConcurrentBag<ConsoleLoggerProvider> Instances = new ConcurrentBag<ConsoleLoggerProvider>();

        private readonly BlockingCollection<LogMessage> _messages =
            new BlockingCollection<LogMessage>(new ConcurrentQueue<LogMessage>());
        private readonly AutoResetEvent _doneProcessing = new AutoResetEvent(false);

        public ConsoleLoggerProvider()
        {
            Instances.Add(this);
            new Thread(() =>
            {
                LogMessage message;
                while ((message = TakeMessage()) != null)
                {
                    WriteMessage(message);
                }
                _doneProcessing.Set();
            }).Start();
        }

        private LogMessage TakeMessage()
        {
            if (!_messages.IsCompleted)
            {
                try
                {
                    return _messages.Take();
                }
                catch (InvalidOperationException)
                {
                }
            }
            return null;
        }

        internal void AddMessage(LogMessage message) => _messages.Add(message);

        public ILogger CreateLogger(string categoryName) => new ConsoleLogger(this, categoryName);

        private void WriteMessage(LogMessage message)
        {
            lock (WriteLock)
            {
                ConsoleContents.Clear();

                // Add the log level message
                string logLevelString = GetLogLevelString(message.LogLevel);
                ConsoleContents.Add(GetLogLevelConsoleContent(message.LogLevel, logLevelString.AsMemory()));

                // If this is an information message, do some fancy colorization
                if (message.LogLevel == LogLevel.Information)
                {
                    // Find » and color pipelines/modules differently
                    int lastAngle = message.FormattedMessage.LastIndexOf('»');
                    if (lastAngle > 0)
                    {
                        int firstAngle = message.FormattedMessage.IndexOf('»');
                        ConsoleContents.Add(new ConsoleContent(ConsoleColor.Blue, ConsoleColor.Black, message.FormattedMessage.AsMemory(0, firstAngle + 1)));
                        if (firstAngle < lastAngle)
                        {
                            ConsoleContents.Add(new ConsoleContent(ConsoleColor.Cyan, ConsoleColor.Black, message.FormattedMessage.AsMemory(firstAngle + 1, lastAngle - (firstAngle + 1) + 1)));
                        }
                    }
                    else
                    {
                        lastAngle = -1;
                    }

                    // Then add the category
                    int normalStart = lastAngle + 1;
                    if (message.CategoryName?.StartsWith(NamespacePrefix) == false)
                    {
                        ConsoleContents.Add(new ConsoleContent(ConsoleColor.DarkGray, ConsoleColor.Black, $"[{message.CategoryName}] ".AsMemory()));
                        ConsoleContents.Add(new ConsoleContent(
                            message.FormattedMessage.AsMemory(normalStart, message.FormattedMessage.Length - normalStart)));
                    }
                    else
                    {
                        // Scan for parenthesis and split into segments (but only if we're not in an external category)
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

                                            // Inside parenthesis
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
                }
                else
                {
                    // Add the category
                    if (message.CategoryName?.StartsWith(NamespacePrefix) == false)
                    {
                        ConsoleContents.Add(new ConsoleContent(ConsoleColor.DarkGray, ConsoleColor.Black, $"[{message.CategoryName}] ".AsMemory()));
                    }

                    // Then add the message
                    ConsoleContents.Add(GetLogLevelConsoleContent(message.LogLevel, message.FormattedMessage.AsMemory()));
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

        private static string GetLogLevelString(LogLevel logLevel) =>
            logLevel switch
            {
                LogLevel.Trace => "[TRCE] ",
                LogLevel.Debug => "[DBUG] ",
                LogLevel.Information => "[INFO] ",
                LogLevel.Warning => "[WARN] ",
                LogLevel.Error => "[ERRO] ",
                LogLevel.Critical => "[CRIT] ",
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel)),
            };

        // We must explicitly set the background color if we are setting the foreground color,
        // since just setting one can look bad on the users console.
        private ConsoleContent GetLogLevelConsoleContent(LogLevel logLevel, in ReadOnlyMemory<char> message) =>
            logLevel switch
            {
                LogLevel.Critical => new ConsoleContent(ConsoleColor.DarkRed, ConsoleColor.Black, message),
                LogLevel.Error => new ConsoleContent(ConsoleColor.Red, ConsoleColor.Black, message),
                LogLevel.Warning => new ConsoleContent(ConsoleColor.Yellow, ConsoleColor.Black, message),
                LogLevel.Information => new ConsoleContent(ConsoleColor.DarkGreen, ConsoleColor.Black, message),
                LogLevel.Debug => new ConsoleContent(ConsoleColor.DarkGray, ConsoleColor.Black, message),
                LogLevel.Trace => new ConsoleContent(ConsoleColor.DarkGray, ConsoleColor.Black, message),
                _ => new ConsoleContent(message),
            };

        internal static void DisposeAll()
        {
            foreach (ConsoleLoggerProvider instance in Instances)
            {
                instance.Dispose();
            }
            Instances.Clear();
        }

        public void Dispose()
        {
            if (!_messages.IsAddingCompleted)
            {
                try
                {
                    _messages.CompleteAdding();
                    _doneProcessing.WaitOne();
                }
                catch
                {
                }
                _messages.Dispose();
                _doneProcessing.Dispose();
            }
        }
    }
}
