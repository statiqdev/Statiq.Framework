using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Cli;
using Statiq.Common;

namespace Statiq.App
{
    [Description("Executes the pipelines.")]
    internal class BuildCommand<TSettings> : EngineCommand<TSettings>
        where TSettings : BaseCommandSettings
    {
        public BuildCommand(
            IConfiguratorCollection configurators,
            IConfigurationSettings configurationSettings,
            IServiceCollection serviceCollection,
            IConfigurationRoot configurationRoot,
            IBootstrapper bootstrapper)
            : base(
                  configurators,
                  configurationSettings,
                  serviceCollection,
                  configurationRoot,
                  bootstrapper)
        {
        }

        protected override async Task<int> ExecuteEngineAsync(
            CommandContext commandContext,
            TSettings commandSettings,
            IEngineManager engineManager)
        {
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                return await engineManager.ExecuteAsync(cancellationTokenSource)
                    ? (int)ExitCode.Normal
                    : (int)ExitCode.ExecutionError;
            }
        }
    }
}
