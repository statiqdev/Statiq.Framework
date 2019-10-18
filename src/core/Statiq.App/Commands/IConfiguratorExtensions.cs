using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Cli;
using Statiq.Common;

namespace Statiq.App
{
    public static class IConfiguratorExtensions
    {
        public static ICommandConfigurator AddCommand<TCommand>(this IConfigurator configurator)
            where TCommand : class, ICommand =>
            configurator.AddCommand<TCommand>(typeof(TCommand).Name.RemoveEnd("Command", StringComparison.OrdinalIgnoreCase).ToKebab());

        public static ICommandConfigurator AddCommand(this IConfigurator configurator, Type commandType) =>
            configurator.AddCommand(
                commandType ?? throw new ArgumentNullException(nameof(commandType)),
                commandType.Name.RemoveEnd("Command", StringComparison.OrdinalIgnoreCase).ToKebab());

        public static ICommandConfigurator AddCommand(this IConfigurator configurator, Type commandType, string name)
        {
            _ = commandType ?? throw new ArgumentNullException(nameof(commandType));
            _ = name ?? throw new ArgumentNullException(nameof(name));
            if (!typeof(ICommand).IsAssignableFrom(commandType))
            {
                throw new ArgumentException("Provided type is not a command");
            }
            Type openAdapter = typeof(AddCommandAdapter<>);
            Type closedAdapter = openAdapter.MakeGenericType(commandType);
            IAddCommandAdapter adapter = (IAddCommandAdapter)Activator.CreateInstance(closedAdapter, new object[] { name });
            return adapter.AddCommand(configurator);
        }

        public static ICommandConfigurator AddBuild(
            this IConfigurator configurator,
            string name,
            params string[] pipelines) =>
            configurator.AddBuild(name, false, pipelines);

        public static ICommandConfigurator AddBuild(
            this IConfigurator configurator,
            string name,
            bool defaultPipelines,
            params string[] pipelines) =>
                configurator.AddBuild(name, new EngineCommandSettings
                {
                    Pipelines = pipelines,
                    DefaultPipelines = defaultPipelines
                });

        public static ICommandConfigurator AddBuild(
            this IConfigurator configurator,
            string name,
            EngineCommandSettings commandSettings)
        {
            _ = name ?? throw new ArgumentNullException(nameof(name));
            _ = commandSettings ?? throw new ArgumentNullException(nameof(commandSettings));
            return configurator
                .AddCommand<BuildCommand<BaseCommandSettings>>(name)
                .WithData(commandSettings);
        }

        public static ICommandConfigurator AddDelegate(
            this IConfigurator configurator,
            string name,
            Func<CommandContext, int> func) =>
            configurator.AddDelegate<EmptyCommandSettings>(name, (c, _) => func(c));
    }
}
