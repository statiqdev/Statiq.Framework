using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Cli;
using Statiq.Hosting;
using Statiq.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Statiq.App
{
    internal class PreviewCommandSettings : EngineCommandSettings
    {
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

        [CommandOption("--no-reload")]
        [Description("urns off LiveReload support in the preview server.")]
        public bool NoReload { get; set; }
    }
}
