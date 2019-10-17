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
    internal class CustomBuildCommand : EngineCommand<BaseCommandSettings>
    {
        public CustomBuildCommand(SettingsConfigurationProvider settingsProvider, IConfigurationRoot configurationRoot, IServiceCollection serviceCollection, IBootstrapper bootstrapper)
            : base(settingsProvider, configurationRoot, serviceCollection, bootstrapper)
        {
        }

        public override async Task<int> ExecuteCommandAsync(CommandContext context, BaseCommandSettings settings)
        {
            EngineCommandSettings engineSettings = context.Data as EngineCommandSettings ?? new EngineCommandSettings();

            // Copy over the base settings
            engineSettings.LogLevel = settings.LogLevel;
            engineSettings.Attach = settings.Attach;
            engineSettings.LogFile = settings.LogFile;

            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                using (EngineManager engineManager = new EngineManager(context, this, engineSettings))
                {
                    return await engineManager.ExecuteAsync(cancellationTokenSource)
                        ? (int)ExitCode.Normal
                        : (int)ExitCode.ExecutionError;
                }
            }
        }
    }
}
