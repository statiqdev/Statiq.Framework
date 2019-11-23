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
            ConfigureCommands(x => x.AddCommand<TCommand>());

        public IBootstrapper AddCommand<TCommand>(string name)
            where TCommand : class, ICommand =>
            ConfigureCommands(x => x.AddCommand<TCommand>(name));

        public IBootstrapper AddCommand(Type commandType) =>
            ConfigureCommands(x => x.AddCommand(commandType));

        public IBootstrapper AddCommand(Type commandType, string name) =>
            ConfigureCommands(x => x.AddCommand(commandType, name));

        public IBootstrapper AddCommand(Type commandType, string name, string description) =>
            ConfigureCommands(x => x.AddCommand(commandType, name).WithDescription(description));

        public IBootstrapper AddPipelineCommand(
            string name,
            params string[] pipelines) =>
            ConfigureCommands(x => x.AddPipelineCommand(name, pipelines));

        public IBootstrapper AddPipelineCommand(
            string name,
            string description,
            params string[] pipelines) =>
            ConfigureCommands(x => x.AddPipelineCommand(name, pipelines).WithDescription(description));

        public IBootstrapper AddPipelineCommand(
            string name,
            bool defaultPipelines,
            params string[] pipelines) =>
            ConfigureCommands(x => x.AddPipelineCommand(name, defaultPipelines, pipelines));

        public IBootstrapper AddPipelineCommand(
            string name,
            string description,
            bool defaultPipelines,
            params string[] pipelines) =>
            ConfigureCommands(x => x.AddPipelineCommand(name, defaultPipelines, pipelines).WithDescription(description));

        public IBootstrapper AddPipelineCommand(string name, EngineCommandSettings commandSettings) =>
            ConfigureCommands(x => x.AddPipelineCommand(name, commandSettings));

        public IBootstrapper AddPipelineCommand(string name, string description, EngineCommandSettings commandSettings) =>
            ConfigureCommands(x => x.AddPipelineCommand(name, commandSettings).WithDescription(description));

        public IBootstrapper AddDelegateCommand(string name, Func<CommandContext, int> func) =>
            ConfigureCommands(x => x.AddDelegateCommand(name, func));

        public IBootstrapper AddDelegateCommand(string name, string description, Func<CommandContext, int> func) =>
            ConfigureCommands(x => x.AddDelegateCommand(name, func).WithDescription(description));

        public IBootstrapper AddDelegateCommand<TSettings>(string name, Func<CommandContext, TSettings, int> func)
            where TSettings : CommandSettings =>
            ConfigureCommands(x => x.AddDelegate(name, func));

        public IBootstrapper AddDelegateCommand<TSettings>(string name, string description, Func<CommandContext, TSettings, int> func)
            where TSettings : CommandSettings =>
            ConfigureCommands(x => x.AddDelegate(name, func).WithDescription(description));

        /// <summary>
        /// Adds all commands that implement <see cref="ICommand"/> from the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly to add commands from.</param>
        /// <returns>The current bootstrapper.</returns>
        public IBootstrapper AddCommands(Assembly assembly)
        {
            _ = assembly ?? throw new ArgumentNullException(nameof(assembly));
            foreach (Type commandType in ClassCatalog.GetTypesAssignableTo<ICommand>().Where(x => x.Assembly.Equals(assembly)))
            {
                AddCommand(commandType);
            }
            return this;
        }

        /// <summary>
        /// Adds all commands that implement <see cref="ICommand"/> from the entry assembly.
        /// </summary>
        /// <returns>The current bootstrapper.</returns>
        public IBootstrapper AddCommands() => AddCommands(Assembly.GetEntryAssembly());

        public IBootstrapper AddCommands<TParent>()
        {
            foreach (Type commandType in typeof(TParent).GetNestedTypes().Where(x => typeof(ICommand).IsAssignableFrom(x)))
            {
                AddCommand(commandType);
            }
            return this;
        }
    }
}
