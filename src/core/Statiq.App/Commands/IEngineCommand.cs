using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Statiq.App
{
    internal interface IEngineCommand
    {
        SettingsConfigurationProvider SettingsProvider { get; }

        IConfigurationRoot ConfigurationRoot { get; }

        IServiceCollection ServiceCollection { get; }

        IBootstrapper Bootstrapper { get; }
    }
}
