using System;
using System.Collections.Generic;
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

        public IBootstrapper AddBuildCommand(
            string name,
            params string[] pipelines) =>
            AddBuildCommand(name, null, false, pipelines);

        public IBootstrapper AddBuildCommand(
            string name,
            string description,
            params string[] pipelines) =>
            AddBuildCommand(name, description, false, pipelines);

        public IBootstrapper AddBuildCommand(
            string name,
            bool defaultPipelines,
            params string[] pipelines) =>
            AddBuildCommand(name, null, defaultPipelines, pipelines);

        public IBootstrapper AddBuildCommand(
            string name,
            string description,
            bool defaultPipelines,
            params string[] pipelines) =>
                AddBuildCommand(name, description, new EngineCommandSettings
                {
                    Pipelines = pipelines,
                    DefaultPipelines = defaultPipelines
                });

        public IBootstrapper AddBuildCommand(string name, EngineCommandSettings commandSettings) =>
            AddBuildCommand(name, null, commandSettings);

        public IBootstrapper AddBuildCommand(string name, string description, EngineCommandSettings commandSettings)
        {
            _ = name ?? throw new ArgumentNullException(nameof(name));
            _ = commandSettings ?? throw new ArgumentNullException(nameof(commandSettings));
            return ConfigureCommands(x => x
                .AddCommand<BuildCommand<BaseCommandSettings>>(name)
                .WithData(commandSettings)
                .WithDescription(description));
        }

        public IBootstrapper AddDelegateCommand(string name, Func<CommandContext, int> func) =>
            AddDelegateCommand<EmptyCommandSettings>(name, null, (c, _) => func(c));

        public IBootstrapper AddDelegateCommand(string name, string description, Func<CommandContext, int> func) =>
            AddDelegateCommand<EmptyCommandSettings>(name, description, (c, _) => func(c));

        public IBootstrapper AddDelegateCommand<TSettings>(string name, Func<CommandContext, TSettings, int> func)
            where TSettings : CommandSettings =>
            AddDelegateCommand(name, null, func);

        public IBootstrapper AddDelegateCommand<TSettings>(string name, string description, Func<CommandContext, TSettings, int> func)
            where TSettings : CommandSettings =>
            ConfigureCommands(x => x.AddDelegate(name, func).WithDescription(description));

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
