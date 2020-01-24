using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Cli;
using Statiq.Common;

namespace Statiq.App
{
    [Description("Executes deployment pipelines.")]
    internal class DeployCommand : PipelinesCommand<DeployCommandSettings>
    {
        public DeployCommand(
            IConfiguratorCollection configurators,
            IConfigurationSettings configurationSettings,
            IServiceCollection serviceCollection,
            IConfigurationRoot configurationRoot,
            Bootstrapper bootstrapper)
            : base(
                  configurators,
                  configurationSettings,
                  serviceCollection,
                  configurationRoot,
                  bootstrapper)
        {
        }

        protected override void SetPipelines(CommandContext commandContext, DeployCommandSettings commandSettings, IEngineManager engineManager)
        {
            engineManager.Pipelines = engineManager.Engine.Pipelines.Where(x => x.Value.Deployment).Select(x => x.Key).ToArray();
            engineManager.NormalPipelines = !commandSettings.OnlyDeploy;
        }
    }
}
