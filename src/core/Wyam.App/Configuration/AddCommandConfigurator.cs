using Spectre.Cli;

namespace Wyam.App.Configuration
{
    public class AddCommandConfigurator<TCommand> : ICommandConfigurator
        where TCommand : class, ICommand
    {
        private readonly string _name;

        public AddCommandConfigurator(string name)
        {
            _name = name;
        }

        public void Configure(IConfigurator item) =>
            item.AddCommand<TCommand>(_name);
    }
}
