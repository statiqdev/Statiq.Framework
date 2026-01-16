using System.ComponentModel;
using Spectre.Console.Cli;

namespace Statiq.App
{
    public class PipelinesCommandSettings : EngineCommandSettings
    {
        [CommandOption("-n|--normal")]
        [Description("Executes normal pipelines as well as those specified.")]
        public bool NormalPipelines { get; set; }

        [CommandOption("--no-interactive")]
        [Description("Disables the interactive console/REPL. Useful for CI/CD, containers, background processes, or environments without a console.")]
        public bool NoInteractive { get; set; }

        [CommandArgument(0, "[pipelines]")]
        [Description("The pipeline(s) to execute.")]
        public string[] Pipelines { get; set; }
    }
}
