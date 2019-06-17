using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Cli;
using Statiq.App.Tracing;
using Trace = Statiq.Common.Tracing.Trace;

namespace Statiq.App.Commands
{
    public abstract class BaseCommand<TSettings> : AsyncCommand<TSettings>
        where TSettings : BaseSettings
    {
        public sealed override async Task<int> ExecuteAsync(CommandContext context, TSettings settings)
        {
            // Set verbose tracing
            if (settings.Verbose)
            {
                Trace.Level = SourceLevels.Verbose;
            }

            // Attach
            if (settings.Attach)
            {
                Trace.Information($"Waiting for a debugger to attach to process {Process.GetCurrentProcess().Id} (or press a key to continue)...");
                while (!Debugger.IsAttached && !Console.KeyAvailable)
                {
                    Thread.Sleep(100);
                }
                if (Console.KeyAvailable)
                {
                    Console.ReadKey(true);
                    Trace.Information("Key pressed, continuing execution");
                }
                else
                {
                    Trace.Information("Debugger attached, continuing execution");
                }
            }

            // Logging
            if (settings.Log && string.IsNullOrEmpty(settings.LogFile))
            {
                settings.LogFile = $"log-{DateTime.Now:yyyyMMddHHmmssfff}.txt";
            }
            if (!string.IsNullOrEmpty(settings.LogFile))
            {
                // Delete an exiting log file if one exists
                if (File.Exists(settings.LogFile))
                {
                    try
                    {
                        File.Delete(settings.LogFile);
                    }
                    catch (Exception)
                    {
                    }
                }

                Trace.AddListener(new SimpleFileTraceListener(settings.LogFile));
            }

            return await ExecuteCommandAsync(context, settings);
        }

        public abstract Task<int> ExecuteCommandAsync(CommandContext context, TSettings settings);
    }
}
