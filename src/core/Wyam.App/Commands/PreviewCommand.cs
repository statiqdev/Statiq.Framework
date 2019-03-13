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
    public class PreviewCommand : Command<PreviewCommand.Settings>
    {
        public class Settings : BuildCommand.Settings
        {
            [CommandOption("-w|--watch")]
            [Description("Watches the input folder(s) for changes and rebuilds the site.")]
            public bool Watch { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            throw new NotImplementedException();
        }
    }
}
