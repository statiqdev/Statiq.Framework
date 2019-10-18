using Spectre.Cli;

namespace Statiq.App
{
    internal interface IAddCommandAdapter
    {
        ICommandConfigurator AddCommand(IConfigurator configurator);
    }
}
