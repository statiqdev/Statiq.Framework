using Spectre.Cli;
using Wyam.App.Configuration;
using Wyam.Common.Configuration;

namespace Wyam.App
{
    public static class BootstrapperExtensions
    {
        public static void AddCommand<TCommand>(this IBootstrapper bootstrapper, string name)
            where TCommand : class, ICommand =>
            bootstrapper.Configurators.Add(new AddCommandConfigurator<TCommand>(name));
    }
}
