using System;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Statiq.App
{
    internal static class CommandUtilities
    {
        internal static void WaitForControlC(Action action, ILogger logger)
        {
            // Only wait for a key if console input has not been redirected, otherwise it's on the caller to exit
            if (!Console.IsInputRedirected)
            {
                // Start the key listening thread
                Thread thread = new Thread(() =>
                {
                    logger.LogInformation("Hit Ctrl-C to exit");
                    Console.TreatControlCAsInput = true;
                    while (true)
                    {
                        // Would have preferred to use Console.CancelKeyPress, but that bubbles up to calling batch files
                        // The (ConsoleKey)3 check is to support a bug in VS Code: https://github.com/Microsoft/vscode/issues/9347
                        ConsoleKeyInfo consoleKey = Console.ReadKey(true);
                        if (consoleKey.Key == (ConsoleKey)3 || (consoleKey.Key == ConsoleKey.C && (consoleKey.Modifiers & ConsoleModifiers.Control) != 0))
                        {
                            action();
                            break;
                        }
                    }
                })
                {
                    IsBackground = true
                };
                thread.Start();
            }
        }
    }
}
