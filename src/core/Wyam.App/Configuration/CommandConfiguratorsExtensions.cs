using Spectre.Cli;
using Wyam.App.Configuration;

// Keep in the Wyam.App namespace so extensions come into scope
namespace Wyam.App
{
    public static class CommandConfiguratorsExtensions
    {
        public static void AddCommand<TCommand>(this ConfiguratorCollection<IConfigurator> commandConfigurators, string name)
            where TCommand : class, ICommand =>
            commandConfigurators.Add(new AddCommandConfigurator<TCommand>(name));
    }
}
