using System.ComponentModel;
using Spectre.Cli;

namespace Statiq.App
{
    internal class ServeCommandSettings : BaseCommandSettings
    {
        [CommandArgument(0, "[root]")]
        [Description("The root folder to serve.")]
        public string RootPath { get; set; }

        [CommandOption("--port")]
        [Description("Start the preview web server on the specified port (default is 5080).")]
        public int Port { get; set; } = 5080;

        [CommandOption("--force-ext")]
        [Description("Force the use of extensions in the preview web server (by default, extensionless URLs may be used).")]
        public bool ForceExt { get; set; }

        [CommandOption("--virtual-dir")]
        [Description("Serve files in the preview web server under the specified virtual directory.")]
        public string VirtualDirectory { get; set; }

        [CommandOption("--content-type")]
        [Description("Specifies additional supported content types for the preview server as extension=contenttype.")]
        public string[] ContentTypes { get; set; }

        [CommandOption("--no-watch")]
        [Description("Turns off watching the input folder(s) for changes and rebuilding.")]
        public bool NoWatch { get; set; }
    }
}
