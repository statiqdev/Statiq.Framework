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
        private readonly IConfigurationSettings _configurationSettings;

        protected EngineCommand(
            IConfiguratorCollection configurators,
            IConfigurationSettings configurationSettings,
            IServiceCollection serviceCollection,
            IConfigurationRoot configurationRoot,
            Bootstrapper bootstrapper)
            : base(configurators, configurationSettings, serviceCollection)
        {
            ConfigurationRoot = configurationRoot;
            Bootstrapper = bootstrapper;
            _configurationSettings = configurationSettings;
        }

        public IConfigurationRoot ConfigurationRoot { get; }

        public Bootstrapper Bootstrapper { get; }

        public override sealed async Task<int> ExecuteCommandAsync(CommandContext commandContext, TSettings commandSettings)
        {
            // We need to get the engine command settings to pass to the engine manager
            // First try the actual command settings
            if (!(commandSettings is EngineCommandSettings engineCommandSettings))
            {
                // Then try the command data or create one and either way copy over the base command settings
                engineCommandSettings = commandContext.Data as EngineCommandSettings ?? new EngineCommandSettings();
                engineCommandSettings.LogLevel = commandSettings.LogLevel;
                engineCommandSettings.Attach = commandSettings.Debug;
                engineCommandSettings.Debug = commandSettings.Attach;
                engineCommandSettings.LogFile = commandSettings.LogFile;
            }

            // Execute the engine manager and dispose it when done
            // Once the engine manager is created, the configuration settings cannot be used (they will have been copied over)
            using (EngineManager engineManager =
                new EngineManager(
                    commandContext,
                    engineCommandSettings,
                    _configurationSettings,
                    ServiceCollection,
                    Bootstrapper))
            {
                return await ExecuteEngineAsync(commandContext, commandSettings, engineManager);
            }
        }

        protected abstract Task<int> ExecuteEngineAsync(CommandContext commandContext, TSettings commandSettings, IEngineManager engineManager);
    }
}
