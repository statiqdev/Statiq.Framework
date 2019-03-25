using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Cli;
using Wyam.App.Configuration;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Tracing;
using Wyam.Core.Execution;

namespace Wyam.App.Commands
{
    internal class EngineManager : IDisposable
    {
        public EngineManager(IConfigurableBootstrapper bootstrapper, BuildCommand.Settings settings)
        {
            Engine = new Engine();

            // Set folders
            DirectoryPath currentDirectory = Environment.CurrentDirectory;
            Engine.FileSystem.RootPath = string.IsNullOrEmpty(settings.RootPath)
                ? currentDirectory
                : currentDirectory.Combine(settings.RootPath);
            if (settings.InputPaths?.Length > 0)
            {
                // Clear existing default paths if new ones are set
                // and reverse the inputs so the last one is first to match the semantics of multiple occurrence single options
                Engine.FileSystem.InputPaths.Clear();
                Engine.FileSystem.InputPaths.AddRange(settings.InputPaths.Select(x => new DirectoryPath(x)).Reverse());
            }
            if (!string.IsNullOrEmpty(settings.OutputPath))
            {
                Engine.FileSystem.OutputPath = settings.OutputPath;
            }
            if (settings.NoClean)
            {
                Engine.Settings[Keys.CleanOutputPath] = false;
            }

            // Set no cache if requested
            if (settings.NoCache)
            {
                Engine.Settings[Keys.UseCache] = false;
            }

            // Get the standard input stream
            if (settings.StdIn)
            {
                using (StreamReader reader = new StreamReader(Console.OpenStandardInput(), Console.InputEncoding))
                {
                    Engine.ApplicationInput = reader.ReadToEnd();
                }
            }

            // Add settings
            if (settings.MetadataSettings?.Length > 0)
            {
                foreach (KeyValuePair<string, object> metadata in MetadataParser.Parse(settings.MetadataSettings))
                {
                    Engine.Settings.Add(metadata);
                }
            }

            // Run configurators after command line has been applied
            bootstrapper.Configurators.Configure<IEngine>(Engine);

            // Trace the full environment
            Trace.Information($"Root path:{Environment.NewLine}    {Engine.FileSystem.RootPath}");
            Trace.Information($"Input path(s):{Environment.NewLine}    {string.Join(Environment.NewLine + "    ", Engine.FileSystem.InputPaths)}");
            Trace.Information($"Output path:{Environment.NewLine}    {Engine.FileSystem.OutputPath}");
            Trace.Information($"Temp path:{Environment.NewLine}    {Engine.FileSystem.TempPath}");
            Trace.Verbose($"Settings:{Environment.NewLine}    {string.Join(Environment.NewLine + "    ", Engine.Settings.Select(x => $"{x.Key}: {x.Value?.ToString() ?? "null"}"))}");

            // Make sure we clear out anything in the JavaScriptEngineSwitcher instance
            Engine.ResetJsEngines();
        }

        public Engine Engine { get; }

        public async Task<bool> ExecuteAsync(IServiceProvider serviceProvider)
        {
            try
            {
                await Engine.ExecuteAsync(serviceProvider);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public void Dispose() => Engine.Dispose();
    }
}
