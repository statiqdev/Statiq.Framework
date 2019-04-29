using Spectre.Cli;

namespace Wyam.App.Configuration
{
    /// <summary>
    /// Adds a specified command type to the CLI command set.
    /// </summary>
    /// <typeparam name="TCommand">The type of CLI command to add.</typeparam>
    public class AddCommandConfigurator<TCommand> : Common.Configuration.IConfigurator<ConfigurableCommands>
        where TCommand : class, ICommand
    {
        private readonly string _name;

        public AddCommandConfigurator(string name)
        {
            _name = name;
        }

        public void Configure(ConfigurableCommands configurableCommands) =>
            configurableCommands.CommandConfigurator.AddCommand<TCommand>(_name);
    }
}
