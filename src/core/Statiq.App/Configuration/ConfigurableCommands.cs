using Statiq.Common;

namespace Statiq.App
{
    public class ConfigurableCommands : IConfigurable
    {
        internal ConfigurableCommands(Spectre.Cli.IConfigurator commandConfigurator) => CommandConfigurator = commandConfigurator;

        public Spectre.Cli.IConfigurator CommandConfigurator { get; }
    }
}
