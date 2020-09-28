using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Cli;
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

        [CommandOption("--noclean")]
        [Description("Prevents cleaning of the output path on each execution.")]
        public bool NoClean { get; set; }

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
        [Description("Specifies the log level for the specified analyzer as \"[analyzer]=[log level]\" (or for all analyzers if only a log level is provided).")]
        public string[] Analyzers { get; set; }
    }
}
