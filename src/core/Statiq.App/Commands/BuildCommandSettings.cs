using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Cli;
using Statiq.Common;

namespace Statiq.App
{
    public class BuildCommandSettings : EngineCommandSettings
    {
        [CommandOption("-p|--pipeline <PIPELINE>")]
        [Description("Explicitly specifies one or more pipelines to execute.")]
        public string[] Pipelines { get; set; }

        [CommandOption("-n|--normal")]
        [Description("If pipelines are explicitly specified, executes normal pipelines as well.")]
        public bool NormalPipelines { get; set; }
    }
}
