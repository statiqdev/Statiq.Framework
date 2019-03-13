using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using Spectre.Cli;
using Wyam.App.Configuration;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Core.Execution;

namespace Wyam.App.Commands
{
    [Description("Builds the site and serves it, optionally watching for changes and rebuilding.")]
    public class PreviewCommand : BaseCommand<PreviewCommand.Settings>
    {
        public class Settings : BuildCommand.Settings
        {
            [CommandOption("-w|--watch")]
            [Description("Watches the input folder(s) for changes and rebuilds the site.")]
            public bool Watch { get; set; }

            [CommandOption("-p|--port")]
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
        }

        private readonly IConfigurableBootstrapper _bootstrapper;
        private readonly IServiceProvider _serviceProvider;

        public PreviewCommand(IConfigurableBootstrapper bootstrapper, IServiceProvider serviceProvider)
        {
            _bootstrapper = bootstrapper;
            _serviceProvider = serviceProvider;
        }

        public override int ExecuteCommand(CommandContext context, Settings settings)
        {
            // TODO: Set up the preview server

            using (EngineManager engineManager = new EngineManager(_bootstrapper, settings))
            {
                // TODO: Set up the watchers and call execute whenever files change

                return engineManager.Execute(_serviceProvider)
                    ? (int)ExitCode.Normal
                    : (int)ExitCode.ExecutionError;
            }
        }
    }
}
