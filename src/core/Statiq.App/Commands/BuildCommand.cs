using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Cli;
using Statiq.Common;

namespace Statiq.App
{
    [Description("Runs the pipelines.")]
    internal class BuildCommand : EngineCommand<EngineCommandSettings>
    {
        public BuildCommand(SettingsConfigurationProvider settingsProvider, IConfigurationRoot configurationRoot, IServiceCollection serviceCollection, IBootstrapper bootstrapper)
            : base(settingsProvider, configurationRoot, serviceCollection, bootstrapper)
        {
        }

        public override async Task<int> ExecuteCommandAsync(CommandContext context, EngineCommandSettings settings)
        {
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                using (EngineManager engineManager = new EngineManager(context, this, settings))
                {
                    return await engineManager.ExecuteAsync(cancellationTokenSource)
                        ? (int)ExitCode.Normal
                        : (int)ExitCode.ExecutionError;
                }
            }
        }
    }
}
