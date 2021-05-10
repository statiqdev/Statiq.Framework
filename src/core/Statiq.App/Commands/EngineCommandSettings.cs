using System.ComponentModel;
using Spectre.Console.Cli;
using Statiq.Common;

namespace Statiq.App
{
    public class EngineCommandSettings : BaseCommandSettings
    {
        [CommandOption("-i|--input <PATH>")]
        [Description("The path(s) of input files, can be absolute or relative to the current folder.")]
        public string[] InputPaths { get; set; }

        [CommandOption("-o|--output <PATH>")]
        [Description("The path to output files, can be absolute or relative to the current folder.")]
        public string OutputPath { get; set; }

        [CommandOption("--temp <PATH>")]
        [Description("The path to output temporary files, can be absolute or relative to the current folder.")]
        public string TempPath { get; set; }

        [CommandOption("--cache <PATH>")]
        [Description("The path to output cache files, can be absolute or relative to the current folder.")]
        public string CachePath { get; set; }

        [CommandOption("--noclean")]
        [Description("Prevents cleaning of the output path on each execution (same as \"--clean-mode None\").")]
        public bool NoClean { get; set; }

        [CommandOption("--clean-mode")]
        [Description("Specifies how the output path will be cleaned between each execution.")]
        public CleanMode CleanMode { get; set; } = CleanMode.Unwritten;

        [CommandOption("--nocache")]
        [Description("Prevents caching information during execution (less memory usage but slower execution).")]
        public bool NoCache { get; set; }

        [CommandOption("--stdin")]
        [Description("Reads standard input at startup and sets ApplicationInput in the execution context.")]
        public bool StdIn { get; set; }

        [CommandOption("--serial")]
        [Description("Executes pipeline phases and modules in serial.")]
        public bool SerialExecution { get; set; }

        [CommandOption("-r|--root")]
        [Description("The root folder to use.")]
        public string RootPath { get; set; }

        [CommandOption("-a|--analyzer <ANALYZER>")]
        [Description("Specifies the log level for the specified analyzer as \"[[analyzer]]=[[log level]]\" (log level is optional, \"All\" to set all analyzers).")]
        public string[] Analyzers { get; set; }
    }
}
