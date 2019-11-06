using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Spectre.Cli;

namespace Statiq.App
{
    public class BaseCommandSettings : CommandSettings
    {
        [CommandOption("-l|--log-level <LEVEL>")]
        [Description("Sets the minimum log level (\"Critical\", \"Error\", \"Warning\", \"Information\", \"Debug\", \"Trace\", \"None\").")]
        public LogLevel LogLevel { get; set; } = LogLevel.Information;

        [CommandOption("--attach")]
        [Description("Pause execution at the start of the program until a debugger is attached.")]
        public bool Attach { get; set; }

        [CommandOption("-f|--log-file <LOGFILE>")]
        [Description("Log all messages to the specified log file.")]
        public string LogFile { get; set; }

        [CommandOption("-s|--setting <SETTING>")]
        [Description("Specifies a setting as a key=value pair (the value can be omited).")]
        public string[] Settings { get; set; }
    }
}
