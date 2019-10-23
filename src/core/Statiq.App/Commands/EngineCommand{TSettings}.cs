using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Cli;
using Statiq.Common;

namespace Statiq.App
{
    public abstract class EngineCommand<TSettings> : BaseCommand<TSettings>
        where TSettings : BaseCommandSettings
    {
        protected EngineCommand(
            IConfiguratorCollection configurators,
            IConfigurationSettings configurationSettings,
            IServiceCollection serviceCollection,
            IConfigurationRoot configurationRoot,
            IBootstrapper bootstrapper)
            : base(configurators, configurationSettings, serviceCollection)
        {
            ConfigurationRoot = configurationRoot;
            Bootstrapper = bootstrapper;
        }

        public IConfigurationRoot ConfigurationRoot { get; }

        public IBootstrapper Bootstrapper { get; }

        public override sealed async Task<int> ExecuteCommandAsync(CommandContext commandContext, TSettings commandSettings)
        {
            // We need to get the engine command settings to pass to the engine manager
            // First try the actual command settings
            if (!(commandSettings is EngineCommandSettings engineCommandSettings))
            {
                // Then try the command data or create one and either way copy over the base command settings
                engineCommandSettings = commandContext.Data as EngineCommandSettings ?? new EngineCommandSettings();
                engineCommandSettings.LogLevel = commandSettings.LogLevel;
                engineCommandSettings.Attach = commandSettings.Attach;
                engineCommandSettings.LogFile = commandSettings.LogFile;
            }

            // Execute the engine manager and dispose it when done
            using (EngineManager engineManager =
                new EngineManager(
                    commandContext,
                    engineCommandSettings,
                    ConfigurationSettings,
                    ConfigurationRoot,
                    ServiceCollection,
                    Bootstrapper))
            {
                return await ExecuteEngineAsync(commandContext, commandSettings, engineManager);
            }
        }

        protected abstract Task<int> ExecuteEngineAsync(CommandContext commandContext, TSettings commandSettings, IEngineManager engineManager);
    }
}
