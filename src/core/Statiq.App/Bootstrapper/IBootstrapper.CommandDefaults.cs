using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Cli;
using Statiq.Common;

namespace Statiq.App
{
    public partial interface IBootstrapper
    {
        public IBootstrapper AddCommand<TCommand>()
            where TCommand : class, ICommand =>
            AddCommand<TCommand>(typeof(TCommand).Name.RemoveEnd("Command", StringComparison.OrdinalIgnoreCase).ToKebab());

        public IBootstrapper AddCommand<TCommand>(string name)
            where TCommand : class, ICommand =>
            ConfigureCommands(x => x.AddCommand<TCommand>(name));

        public IBootstrapper AddCommand(Type commandType) =>
            AddCommand(
                commandType ?? throw new ArgumentNullException(nameof(commandType)),
                commandType.Name.RemoveEnd("Command", StringComparison.OrdinalIgnoreCase).ToKebab());

        public IBootstrapper AddCommand(Type commandType, string name)
        {
            _ = commandType ?? throw new ArgumentNullException(nameof(commandType));
            _ = name ?? throw new ArgumentNullException(nameof(name));
            if (!typeof(ICommand).IsAssignableFrom(commandType))
            {
                throw new ArgumentException("Provided type is not a command");
            }
            Type openConfiguratorType = typeof(AddCommandConfigurator<>);
            Type configuratorType = openConfiguratorType.MakeGenericType(commandType);
            Common.IConfigurator<ConfigurableCommands> configurator = (Common.IConfigurator<ConfigurableCommands>)Activator.CreateInstance(configuratorType, new object[] { name });
            Configurators.Add(configurator);
            return this;
        }

        public IBootstrapper AddPipelineCommand() =>
            ConfigureCommands(x => x.)

        public IBootstrapper AddCommands(Assembly assembly)
        {
            _ = assembly ?? throw new ArgumentNullException(nameof(assembly));
            foreach (Type commandType in ClassCatalog.GetTypesAssignableTo<ICommand>().Where(x => x.Assembly.Equals(assembly)))
            {
                AddCommand(commandType);
            }
            return this;
        }

        public IBootstrapper AddCommands() => AddCommands(Assembly.GetEntryAssembly());
    }
}
