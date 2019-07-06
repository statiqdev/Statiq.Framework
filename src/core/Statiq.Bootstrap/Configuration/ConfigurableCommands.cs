using Statiq.Common.Configuration;

namespace Statiq.Bootstrap.Configuration
{
    public class ConfigurableCommands : IConfigurable
    {
        internal ConfigurableCommands(Spectre.Cli.IConfigurator commandConfigurator) => CommandConfigurator = commandConfigurator;

        public Spectre.Cli.IConfigurator CommandConfigurator { get; }
    }
}
