using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.App
{
    public class ConsoleLoggerProvider : ILoggerProvider
    {
        private static readonly object WriteLock = new object();
        private static readonly List<ConsoleContent> ConsoleContentBuffer = new List<ConsoleContent>();
        private static readonly ConcurrentBag<ConsoleLoggerProvider> Instances = new ConcurrentBag<ConsoleLoggerProvider>();

        private readonly BlockingCollection<ConsoleLogMessage> _messages =
            new BlockingCollection<ConsoleLogMessage>(new ConcurrentQueue<ConsoleLogMessage>());
        private readonly AutoResetEvent _doneProcessing = new AutoResetEvent(false);
        private readonly BuildServerLogHelper _buildServerLogHelper;

        public ConsoleLoggerProvider(IReadOnlyFileSystem fileSystem = null)
        {
            _buildServerLogHelper = new BuildServerLogHelper(fileSystem);
            Instances.Add(this);
            new Thread(() =>
            {
                ConsoleLogMessage message;
                while ((message = TakeMessage()) is object)
                {
                    WriteMessage(message);
                }
                _doneProcessing.Set();
            }).Start();
        }

        private ConsoleLogMessage TakeMessage()
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

        internal void AddMessage(ConsoleLogMessage message)
        {
            if (message.LogLevel != LogLevel.None)
            {
                _messages.Add(message);
            }
        }

        public ILogger CreateLogger(string categoryName) => new ConsoleLogger(this, categoryName);

        private void WriteMessage(ConsoleLogMessage message)
        {
            lock (WriteLock)
            {
                // Get and write the main message
                ConsoleContentBuffer.Clear();
                message.GetConsoleContent(ConsoleContentBuffer);
                foreach (ConsoleContent content in ConsoleContentBuffer)
                {
                    Console.ForegroundColor = content.Foreground;
                    Console.BackgroundColor = content.Background;
                    Console.Write(content.Message.ToString());
                }
                Console.WriteLine();
                Console.ResetColor();

                // Get and write a build server message if appropriate
                if (_buildServerLogHelper?.IsBuildServer == true && (message.State as StatiqLogState)?.LogToBuildServer == true)
                {
                    string buildServerMessage = _buildServerLogHelper.GetMessage(message.LogLevel, message.State as StatiqLogState, message.FormattedMessage);
                    if (!buildServerMessage.IsNullOrEmpty())
                    {
                        Console.WriteLine(buildServerMessage);
                    }
                }
            }
        }

        /// <summary>
        /// This blocks until all console messages are written, including new ones (so don't add messages while calling this).
        /// </summary>
        public static void FlushAndWait()
        {
            foreach (ConsoleLoggerProvider instance in Instances)
            {
                while (instance._messages.Count > 0)
                {
                }
            }
        }

        /// <summary>
        /// Disposes all provider instances.
        /// </summary>
        public static void DisposeAll()
        {
            while (Instances.TryTake(out ConsoleLoggerProvider instance))
            {
                instance.Dispose();
            }
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
