using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Spectre.Cli;
using Wyam.App.Configuration;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Core.Execution;

namespace Wyam.App.Commands
{
    [Description("Builds the site.")]
    public class BuildCommand : Command<BuildCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            // TODO: uncomment CommandOption on new version of Spectre.Cli
            // [CommandOption("-i|--input")]
            [Description("The path(s) of input files, can be absolute or relative to the current folder.")]
            public string[] InputPaths { get; set; }

            [CommandOption("-o|--output")]
            [Description("The path to output files, can be absolute or relative to the current folder.")]
            public string OutputPath { get; set; }

            [CommandOption("--noclean")]
            [Description("Prevents cleaning of the output path on each execution.")]
            public bool NoClean { get; set; }

            [CommandOption("--nocache")]
            [Description("Prevents caching information during execution (less memory usage but slower execution).")]
            public bool NoCache { get; set; }

            [CommandOption("--stdin")]
            [Description("Reads standard input at startup and sets ApplicationInput in the execution context.")]
            public bool StdIn { get; set; }

            [CommandOption("-l|--log")]
            [Description("Log all trace messages to a log file named wyam-[datetime].txt.")]
            public bool Log { get; set; }

            [CommandOption("--log-file")]
            [Description("Log all trace messages to the specified log file.")]
            public string LogFile { get; set; }

            // TODO: uncomment CommandOption on new version of Spectre.Cli
            // [CommandOption("-s|--setting")]
            [Description("Specifies a setting as a key=value pair. Use the syntax [x,y] to specify an array value.")]
            public string[] MetadataSettings { get; set; }
        }

        private readonly IConfigurableBootstrapper _bootstrapper;
        private readonly IServiceProvider _serviceProvider;

        public BuildCommand(IConfigurableBootstrapper bootstrapper, IServiceProvider serviceProvider)
        {
            _bootstrapper = bootstrapper;
            _serviceProvider = serviceProvider;
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            using (Engine engine = new Engine())
            {
                // Set no cache if requested
                if (settings.NoCache)
                {
                    engine.Settings[Keys.UseCache] = false;
                }

                if (settings.MetadataSettings?.Length > 0)
                {
                    IReadOnlyDictionary<string, object> metadataSettings = MetadataParser.Parse(settings.MetadataSettings);
                }

                // Get the standard input stream
                if (settings.StdIn)
                {
                    using (StreamReader reader = new StreamReader(Console.OpenStandardInput(), Console.InputEncoding))
                    {
                        engine.ApplicationInput = reader.ReadToEnd();
                    }
                }

                // Run configurators after command line has been applied
                _bootstrapper.Configurators.Configure<IEngine>(engine);

                // Make sure we clear out anything in the JavaScriptEngineSwitcher instance
                Engine.ResetJsEngines();

                // Execute the engine
                try
                {
                    engine.Execute(_serviceProvider);
                }
                catch (Exception)
                {
                    return (int)ExitCode.ExecutionError;
                }

                return (int)ExitCode.Normal;
            }
        }
    }
}
