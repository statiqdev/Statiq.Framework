using System;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.App
{
    public class ConsoleListener
    {
        private readonly Action _cancelAction;
        private readonly Action<string> _readLineAction;
        private readonly object _readLineLock = new object();
        private StreamReader _lineReader;
        private InterlockedBool _stopReadingLines = new InterlockedBool();

        public ConsoleListener(Action cancelAction, Action<string> readLineAction = null, bool startReadingLines = false)
        {
            _cancelAction = cancelAction;
            _readLineAction = readLineAction;

            // Only listen if console input has not been redirected, otherwise it's on the caller
            if (!Console.IsInputRedirected)
            {
                Console.CancelKeyPress += CancelKeyPress;
            }

            if (startReadingLines)
            {
                StartReadingLines();
            }
        }

        public void StartReadingLines()
        {
            lock (_readLineLock)
            {
                if (!Console.IsInputRedirected && _lineReader is null && _readLineAction is object)
                {
                    _stopReadingLines.Unset();
                    _lineReader = new StreamReader(Console.OpenStandardInput());
                    new Thread(() =>
                    {
                        // Read lines until we receive a stop (triggered by cancel key or stop method)
                        while (true)
                        {
                            Console.Write("> ");
                            string line = _lineReader.ReadLine();

                            // We get a null line when the reader is closed
                            if (_stopReadingLines)
                            {
                                break;
                            }

                            // Run the action
                            _readLineAction(line);
                        }

                        _lineReader.Dispose();
                        _lineReader = null;

                        Console.WriteLine("CLOSED");
                        Console.WriteLine();
                    })
                    {
                        IsBackground = true
                    }.Start();
                }
            }
        }

        public void StopReadingLines()
        {
            lock (_readLineLock)
            {
                if (_lineReader is object)
                {
                    _stopReadingLines.Set();
                }
            }
        }

        private void CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.CancelKeyPress -= CancelKeyPress;
            StopReadingLines();
            Console.WriteLine();
            e.Cancel = true;
            _cancelAction?.Invoke();
        }
    }
}
