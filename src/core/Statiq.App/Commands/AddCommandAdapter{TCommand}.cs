using System;
using Spectre.Cli;

namespace Statiq.App
{
    /// <summary>
    /// Adapts a command type to the generic <see cref="IConfigurator.AddCommand{TCommand}(string)"/> call.
    /// </summary>
    /// <typeparam name="TCommand">The type of CLI command to add.</typeparam>
    internal class AddCommandAdapter<TCommand> : IAddCommandAdapter
        where TCommand : class, ICommand
    {
        private readonly string _name;

        public AddCommandAdapter(string name)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public ICommandConfigurator AddCommand(IConfigurator configurator) => configurator.AddCommand<TCommand>(_name);
    }
}
