using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Cli;
using Statiq.Common;

namespace Statiq.App
{
    public class PipelinesCommandSettings : EngineCommandSettings
    {
        [CommandOption("-n|--normal")]
        [Description("Executes normal pipelines as well as those specified.")]
        public bool NormalPipelines { get; set; }

        [CommandArgument(0, "[pipelines]")]
        [Description("The pipeline(s) to execute.")]
        public string[] Pipelines { get; set; }
    }
}
