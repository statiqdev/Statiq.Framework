using System.ComponentModel;
using Spectre.Cli;

namespace Statiq.Bootstrap.Commands
{
    public class BaseSettings : CommandSettings
    {
        [CommandOption("-v|--verbose")]
        [Description("Turns on verbose output showing additional trace message useful for debugging.")]
        public bool Verbose { get; set; }

        [CommandOption("--attach")]
        [Description("Pause execution at the start of the program until a debugger is attached.")]
        public bool Attach { get; set; }

        [CommandOption("-l|--log")]
        [Description("Log all trace messages to a log file named log-[datetime].txt.")]
        public bool Log { get; set; }

        [CommandOption("--log-file")]
        [Description("Log all trace messages to the specified log file.")]
        public string LogFile { get; set; }
    }
}
